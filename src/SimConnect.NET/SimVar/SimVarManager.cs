// <copyright file="SimVarManager.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.SimVar.Internal;

namespace SimConnect.NET.SimVar
{
    /// <summary>
    /// Manages dynamic SimVar get/set operations with automatic data definition management.
    /// </summary>
    public sealed class SimVarManager : IDisposable
    {
        private const uint SimConnectObjectIdUser = 0;
        private const uint BaseDefinitionId = 10000;
        private const uint BaseRequestId = 20000;

        private readonly IntPtr simConnectHandle;
        private readonly ConcurrentDictionary<uint, ISimVarRequest> pendingRequests = new();
        private readonly ConcurrentDictionary<(string Name, string Unit, SimConnectDataType DataType), uint> dataDefinitions = new();
        private readonly ConcurrentDictionary<Type, uint> typeToDefIndex = new();
        private readonly ConcurrentDictionary<uint, Action<IntPtr, ISimVarRequest>> defToParser = new();
        private readonly ConcurrentDictionary<uint, (Delegate Write, int TotalSize)> defToWriter = new();
        private readonly ConcurrentDictionary<uint, SimVarSubscription> subscriptions = new();
        private readonly object typeDefinitionSync = new();

        // Removed reflection caches by switching to ISimVarRequest hot-path
        private uint nextDefinitionId;
        private uint nextRequestId;
        private bool disposed;
        private TimeSpan requestTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="SimVarManager"/> class.
        /// </summary>
        /// <param name="simConnectHandle">The SimConnect handle.</param>
        public SimVarManager(IntPtr simConnectHandle)
        {
            this.simConnectHandle = simConnectHandle != IntPtr.Zero ? simConnectHandle : throw new ArgumentException("Invalid SimConnect handle", nameof(simConnectHandle));
            this.nextDefinitionId = BaseDefinitionId;
            this.nextRequestId = BaseRequestId;
        }

        /// <summary>
        /// Gets or sets the default timeout applied to SimVar requests that do not complete.
        /// Defaults to 10 seconds. Set to <see cref="Timeout.InfiniteTimeSpan"/> to disable.
        /// </summary>
        public TimeSpan RequestTimeout
        {
            get => this.requestTimeout;
            set
            {
                if (value < Timeout.InfiniteTimeSpan)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Timeout must be non-negative or Timeout.InfiniteTimeSpan");
                }

                this.requestTimeout = value;
            }
        }

        /// <summary>
        /// Gets a SimVar value asynchronously.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="simVarName">The SimVar name (e.g., "PLANE LATITUDE").</param>
        /// <param name="unit">The unit of measurement (e.g., "degrees").</param>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the operation fails.</exception>
        /// <exception cref="ArgumentException">Thrown when the SimVar type doesn't match the expected type.</exception>
        public async Task<T> GetAsync<T>(string simVarName, string unit = "", uint objectId = SimConnectObjectIdUser, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            ArgumentException.ThrowIfNullOrEmpty(simVarName);

            // Resolve to a definition ID (registry-backed or dynamic)
            uint definitionId = this.EnsureScalarDefinition(simVarName, unit, InferDataType<T>(), cancellationToken);
            return await this.GetAsyncCore<T>(definitionId, objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets a SimVar value asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of value to set.</typeparam>
        /// <param name="simVarName">The SimVar name (e.g., "PLANE LATITUDE").</param>
        /// <param name="unit">The unit of measurement (e.g., "degrees").</param>
        /// <param name="value">The value to set.</param>
        /// <param name="objectId">The object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the operation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the SimVar is not settable.</exception>
        public async Task SetAsync<T>(string simVarName, string unit, T value, uint objectId = SimConnectObjectIdUser, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            ArgumentException.ThrowIfNullOrEmpty(simVarName);
            ArgumentNullException.ThrowIfNull(unit);

            // Try to get definition from registry first
            var definition = SimVarRegistry.Get(simVarName);
            if (definition != null)
            {
                if (!definition.IsSettable)
                {
                    throw new InvalidOperationException($"SimVar {simVarName} is not settable");
                }

                // Validate the type matches the definition
                if (!IsTypeCompatible(typeof(T), definition.NetType))
                {
                    throw new ArgumentException($"Type {typeof(T).Name} is not compatible with SimVar {simVarName} which expects {definition.NetType.Name}");
                }

                await this.SetWithDefinitionAsync(definition, value, objectId, cancellationToken).ConfigureAwait(false);
                return;
            }

            // If not in registry, create a dynamic definition (assume settable)
            var dataType = InferDataType<T>();
            var dynamicDefinition = new SimVarDefinition(simVarName, unit, dataType, true, "Dynamically created definition");

            await this.SetWithDefinitionAsync(dynamicDefinition, value, objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets multiple SimVars in one call by passing a struct annotated with <see cref="SimConnectAttribute"/> fields.
        /// This mirrors the struct-based GetAsync and uses the same definition/layout.
        /// </summary>
        /// <typeparam name="T">The struct type to write. Must have public fields annotated with <see cref="SimConnectAttribute"/>.</typeparam>
        /// <param name="value">The struct instance whose fields should be written.</param>
        /// <param name="objectId">The SimConnect object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        public async Task SetAsync<T>(
            T value,
            uint objectId = SimConnectObjectIdUser,
            CancellationToken cancellationToken = default)
            where T : struct
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            cancellationToken.ThrowIfCancellationRequested();

            if (this.simConnectHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("SimConnect handle is not initialized.");
            }

            var defId = this.EnsureTypeDefinition<T>(cancellationToken);
            await this.SetStructAsync(defId, value, objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a full struct from SimConnect as a strongly-typed object using a dynamically built data definition.
        /// </summary>
        /// <typeparam name="T">The struct type to request. Must be blittable/marshalable.</typeparam>
        /// <param name="objectId">The SimConnect object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation and returns the requested struct.</returns>
        public async Task<T> GetAsync<T>(
            uint objectId = SimConnectObjectIdUser,
            CancellationToken cancellationToken = default)
            where T : struct
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            cancellationToken.ThrowIfCancellationRequested();

            if (this.simConnectHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("SimConnect handle is not initialized.");
            }

            var defId = this.EnsureTypeDefinition<T>(cancellationToken);
            return await this.GetAsyncCore<T>(defId, objectId, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes a received SimConnect message and completes any pending requests.
        /// </summary>
        /// <param name="data">The received data pointer.</param>
        /// <param name="dataSize">The size of the received data.</param>
        public void ProcessReceivedData(IntPtr data, uint dataSize)
        {
            if (data == IntPtr.Zero || dataSize == 0)
            {
                return;
            }

            try
            {
                // Read the basic SIMCONNECT_RECV structure to get the message type
                var recv = Marshal.PtrToStructure<SimConnectRecv>(data);

                switch ((SimConnectRecvId)recv.Id)
                {
                    case SimConnectRecvId.SimobjectData:
                    case SimConnectRecvId.SimobjectDataByType:
                        {
                            // Both structures are layout-identical; use SimConnectRecvSimObjectData header
                            var objectData = Marshal.PtrToStructure<SimConnectRecvSimObjectData>(data);

                            // Validate header vs buffer sizes
                            var headerSize = Marshal.SizeOf<SimConnectRecvSimObjectData>() - sizeof(ulong); // data payload starts after the Data field offset
                            if (dataSize < headerSize || objectData.Size < headerSize)
                            {
                                SimConnectLogger.Warning($"SimObjectData header shorter than expected (DataSize={dataSize}, ReportedSize={objectData.Size}, HeaderSize={headerSize})");
                                return;
                            }

                            var requestId = objectData.RequestId;
                            if (!this.pendingRequests.TryGetValue(requestId, out var request))
                            {
                                SimConnectLogger.Warning($"No pending request found for RequestId={requestId}");
                                return;
                            }

                            var definitionId = objectData.DefineId;
                            if (!this.defToParser.TryGetValue(definitionId, out var parse))
                            {
                                SimConnectLogger.Error($"No parser found for DefinitionId={definitionId} (RequestId={requestId})");
                                return;
                            }

                            // Compute payload ptr and perform a minimal sanity check using reported size
                            var dataPtr = IntPtr.Add(data, headerSize);
                            var reportedPayload = (int)objectData.Size - headerSize;
                            if (reportedPayload <= 0)
                            {
                                SimConnectLogger.Warning($"Empty payload for DefinitionId={definitionId}, RequestId={requestId}");
                                return;
                            }

                            try
                            {
                                parse(dataPtr, request);
                            }
                            catch (Exception ex)
                            {
                                SimConnectLogger.Error($"Error completing SimVar request (RequestId={requestId}, DefinitionId={definitionId})", ex);
                                if (request is ISimVarRequest req)
                                {
                                    req.SetException(ex);
                                }
                            }
                            finally
                            {
                                // Ensure non-recurring requests are always removed from the pending table so
                                // they do not remain referenced indefinitely in error or cancellation paths.
                                if (!request.IsRecurring)
                                {
                                    this.pendingRequests.TryRemove(requestId, out _);
                                }
                            }

                            break;
                        }

                    default:
                        // Not a SimVar data message we handle here
                        break;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - this shouldn't break the message processing loop
                SimConnectLogger.Error("Error processing SimVar data", ex);
            }
        }

        /// <summary>
        /// Starts a recurring SimVar subscription (typed struct) and returns a disposable handle immediately.
        /// </summary>
        /// <typeparam name="T">Struct type representing the aggregated data.</typeparam>
        /// <param name="period">Update period (must be recurring, i.e. not Once/Never).</param>
        /// <param name="onValue">Callback invoked for each received value.</param>
        /// <param name="objectId">Sim object id; defaults to user aircraft.</param>
        /// <param name="cancellationToken">Cancellation token to auto-dispose the subscription.</param>
        /// <returns>Subscription handle.</returns>
        public ISimVarSubscription Subscribe<T>(
            SimConnectPeriod period,
            Action<T> onValue,
            uint objectId = SimConnectObjectIdUser,
            CancellationToken cancellationToken = default)
            where T : struct
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            ArgumentNullException.ThrowIfNull(onValue);
            if (period == SimConnectPeriod.Once || period == SimConnectPeriod.Never)
            {
                throw new ArgumentException("Use GetAsync for one-shot requests; subscription requires a recurring period.", nameof(period));
            }

            var defId = this.EnsureTypeDefinition<T>(cancellationToken);
            var request = this.StartRequest<T>(defId, objectId, period, onValue);
            var subscription = new SimVarSubscription(this, request, cancellationToken);
            this.subscriptions[request.RequestId] = subscription;
            return subscription;
        }

        /// <summary>
        /// Starts a recurring SimVar subscription (scalar by name/unit) and returns a disposable handle immediately.
        /// </summary>
        /// <typeparam name="T">Expected scalar type.</typeparam>
        /// <param name="simVarName">SimVar name.</param>
        /// <param name="unit">Unit string.</param>
        /// <param name="period">Update period (must be recurring).</param>
        /// <param name="onValue">Callback for each received value.</param>
        /// <param name="objectId">Target object id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Subscription handle.</returns>
        public ISimVarSubscription Subscribe<T>(
            string simVarName,
            string unit,
            SimConnectPeriod period,
            Action<T> onValue,
            uint objectId = SimConnectObjectIdUser,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            ArgumentException.ThrowIfNullOrEmpty(simVarName);
            ArgumentException.ThrowIfNullOrEmpty(unit);
            ArgumentNullException.ThrowIfNull(onValue);
            if (period == SimConnectPeriod.Once || period == SimConnectPeriod.Never)
            {
                throw new ArgumentException("Use GetAsync for one-shot requests; subscription requires a recurring period.", nameof(period));
            }

            var defId = this.EnsureScalarDefinition(simVarName, unit, InferDataType<T>(), cancellationToken);
            var request = this.StartRequest<T>(defId, objectId, period, onValue);
            var subscription = new SimVarSubscription(this, request, cancellationToken);
            this.subscriptions[request.RequestId] = subscription;
            return subscription;
        }

        /// <summary>
        /// Disposes the SimVar manager and cancels all pending requests.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                // Dispose all active subscriptions (will invoke CancelRequest and remove themselves)
                foreach (var kvp in this.subscriptions)
                {
                    try
                    {
                        kvp.Value.Dispose();
                    }
                    catch (Exception ex)
                    {
                        if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                        {
                            SimConnectLogger.Debug($"Suppressing error disposing subscription {kvp.Key}: {ex.Message}");
                        }
                    }
                }

                this.subscriptions.Clear();

                foreach (var kvp in this.pendingRequests)
                {
                    kvp.Value.SetCanceled();
                }

                this.pendingRequests.Clear();
                this.dataDefinitions.Clear();
            }
        }

        /// <summary>
        /// Ensures a data definition exists and is registered with a parser for type T.
        /// - For typed structs, builds descriptor via SimVarDescriptorFactory and registers parser and def index once.
        /// </summary>
        /// <typeparam name="T">struct type described by the definition.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Definition ID for the SimConnect data definition.</returns>
        internal uint EnsureTypeDefinition<T>(CancellationToken cancellationToken)
            where T : struct
        {
            if (this.typeToDefIndex.TryGetValue(typeof(T), out var existing))
            {
                return existing;
            }

            lock (this.typeDefinitionSync)
            {
                if (this.typeToDefIndex.TryGetValue(typeof(T), out existing))
                {
                    return existing;
                }

                cancellationToken.ThrowIfCancellationRequested();

                var definitionId = Interlocked.Increment(ref this.nextDefinitionId);

                var readers = SimVarFieldReaderFactory.Build<T>(
                    (name, unit, dataType) =>
                    {
                        var hr = SimConnectNative.SimConnect_AddToDataDefinition(
                            this.simConnectHandle,
                            definitionId,
                            name,
                            unit ?? string.Empty,
                            (uint)dataType);

                        if (hr != (int)SimConnectError.None)
                        {
                            throw new SimConnectException($"Failed to add data definition for {name}: {(SimConnectError)hr}", (SimConnectError)hr);
                        }
                    });

                this.defToParser[definitionId] = (IntPtr basePtr, ISimVarRequest req) =>
                {
                    var result = default(T);
                    foreach (var reader in readers)
                    {
                        reader.ReadInto(ref result, basePtr);
                    }

                    if (!req.TrySetResult(result))
                    {
                        req.SetResultBoxed(result);
                    }
                };
                this.typeToDefIndex[typeof(T)] = definitionId;

                var writerBuild = SimVarFieldWriterFactory.Build<T>(addToDefinition: null);
                Action<IntPtr, T> write = (basePtr, v) =>
                {
                    foreach (var w in writerBuild.Writers)
                    {
                        w.WriteFrom(in v, basePtr);
                    }
                };

                this.defToWriter[definitionId] = (write, writerBuild.TotalSize);

                return definitionId;
            }
        }

        /// <summary>
        /// Cancels a pending or recurring SimVar request and removes it from tracking.
        /// For recurring requests a SimConnect 'Never' period request is issued to stop further updates.
        /// Safe to call multiple times; subsequent calls are no-ops after removal.
        /// </summary>
        /// <param name="request">The request to cancel.</param>
        internal void CancelRequest(ISimVarRequest request)
        {
            if (request is null)
            {
                return;
            }

            this.pendingRequests.TryRemove(request.RequestId, out _);
            this.subscriptions.TryRemove(request.RequestId, out _);

            if (request.IsRecurring)
            {
                try
                {
                    if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                    {
                        SimConnectLogger.Debug($"Canceling recurring request {request.RequestId} (Def={request.DefinitionId}, Obj={request.ObjectId})");
                    }

                    this.RequestDataOnSimObject(request.RequestId, request.DefinitionId, request.ObjectId, SimConnectPeriod.Never);
                }
                catch (Exception ex)
                {
                    if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                    {
                        SimConnectLogger.Debug($"Suppressing error during CancelRequest for {request.RequestId}: {ex.Message}");
                    }
                }
            }

            request.SetCanceled();
        }

        private static SimConnectDataType InferDataType<T>()
        {
            var type = typeof(T);

            return type switch
            {
                Type t when t == typeof(int) || t == typeof(bool) => SimConnectDataType.Integer32,
                Type t when t == typeof(long) => SimConnectDataType.Integer64,
                Type t when t == typeof(float) => SimConnectDataType.FloatSingle,
                Type t when t == typeof(double) => SimConnectDataType.FloatDouble,
                Type t when t == typeof(string) => SimConnectDataType.String256, // Default string size
                Type t when t == typeof(SimConnectDataInitPosition) => SimConnectDataType.InitPosition,
                Type t when t == typeof(SimConnectDataLatLonAlt) => SimConnectDataType.LatLonAlt,
                Type t when t == typeof(SimConnectDataXyz) => SimConnectDataType.Xyz,
                _ => throw new ArgumentException($"Unsupported type for SimVar: {type.Name}"),
            };
        }

        private static bool IsTypeCompatible(Type requestedType, Type definitionType)
        {
            if (requestedType == definitionType)
            {
                return true;
            }

            // Allow some implicit conversions
            if (requestedType == typeof(bool) && definitionType == typeof(int))
            {
                return true;
            }

            if (requestedType == typeof(float) && definitionType == typeof(double))
            {
                return true;
            }

            return false;
        }

        private static int GetDataSize<T>()
        {
            var type = typeof(T);

            if (type == typeof(int) || type == typeof(bool) || type == typeof(float))
            {
                return 4;
            }

            if (type == typeof(long) || type == typeof(double))
            {
                return 8;
            }

            if (type == typeof(string))
            {
                return 256; // Default string buffer size
            }

            if (type == typeof(SimConnectDataLatLonAlt))
            {
                return Marshal.SizeOf<SimConnectDataLatLonAlt>();
            }

            if (type == typeof(SimConnectDataXyz))
            {
                return Marshal.SizeOf<SimConnectDataXyz>();
            }

            return Marshal.SizeOf<T>();
        }

        private static void MarshalValue<T>(T value, IntPtr ptr)
        {
            var type = typeof(T);

            if (type == typeof(int))
            {
                Marshal.WriteInt32(ptr, (int)(object)value!);
            }
            else if (type == typeof(bool))
            {
                Marshal.WriteInt32(ptr, (bool)(object)value! ? 1 : 0);
            }
            else if (type == typeof(long))
            {
                Marshal.WriteInt64(ptr, (long)(object)value!);
            }
            else if (type == typeof(float))
            {
                var bytes = BitConverter.GetBytes((float)(object)value!);
                Marshal.Copy(bytes, 0, ptr, 4);
            }
            else if (type == typeof(double))
            {
                var bytes = BitConverter.GetBytes((double)(object)value!);
                Marshal.Copy(bytes, 0, ptr, 8);
            }
            else if (type == typeof(string))
            {
                var str = (string)(object)value!;
                var bytes = System.Text.Encoding.ASCII.GetBytes(str);
                Marshal.Copy(bytes, 0, ptr, Math.Min(bytes.Length, 256));
            }
            else
            {
                Marshal.StructureToPtr(value!, ptr, false);
            }
        }

        private static string ParseString(IntPtr dataPtr, SimConnectDataType dataType)
        {
            var maxLength = dataType switch
            {
                SimConnectDataType.String8 => 8,
                SimConnectDataType.String32 => 32,
                SimConnectDataType.String64 => 64,
                SimConnectDataType.String128 => 128,
                SimConnectDataType.String256 => 256,
                SimConnectDataType.String260 => 260,
                SimConnectDataType.StringV => -1,
                _ => -1,
            };

            if (maxLength == -1)
            {
                throw new NotSupportedException("Variable-length SimVar strings (StringV) are not supported. Use a fixed-length string type (e.g., String256).");
            }

            return SimVarMemoryReader.ReadFixedString(dataPtr, maxLength);
        }

        /// <summary>
        /// Wrapper to call SimConnect_RequestDataOnSimObject with consistent error handling.
        /// A local context string is generated from the parameters for logging and exception messages.
        /// </summary>
        /// <param name="requestId">The SimConnect request identifier.</param>
        /// <param name="definitionId">The data definition identifier.</param>
        /// <param name="objectId">The target object identifier.</param>
        /// <param name="period">The request period.</param>
        /// <remarks>Throws a SimConnectException on error (except when period == Never which is used internally for cancellation).</remarks>
        private void RequestDataOnSimObject(
            uint requestId,
            uint definitionId,
            uint objectId,
            SimConnectPeriod period)
        {
            var hr = SimConnectNative.SimConnect_RequestDataOnSimObject(
                this.simConnectHandle,
                requestId,
                definitionId,
                objectId,
                (uint)period);

            // Build a local context string from parameters for logging and exceptions
            var localContext = period != SimConnectPeriod.Once && period != SimConnectPeriod.Never
                ? $"subscribe (RequestId={requestId}, DefinitionId={definitionId}, ObjectId={objectId})"
                : $"request (RequestId={requestId}, DefinitionId={definitionId}, ObjectId={objectId})";

            if (hr != (int)SimConnectError.None && period != SimConnectPeriod.Never)
            {
                throw new SimConnectException($"Failed to {localContext}: {(SimConnectError)hr}", (SimConnectError)hr);
            }
        }

        /// <summary>
        /// Creates and registers a SimVar request, then performs the SimConnect request call atomically.
        /// Ensures the pending request is visible before SimConnect may deliver a response to avoid race conditions.
        /// Subscription/one-shot path (no descriptor).
        /// </summary>
        /// <typeparam name="T">The value or struct type of the request.</typeparam>
        /// <param name="definitionId">The data definition identifier.</param>
        /// <param name="objectId">The target object identifier.</param>
        /// <param name="period">The request period.</param>
        /// <param name="onValue">Optional callback for recurring subscriptions.</param>
        /// <returns>The created SimVarRequest instance.</returns>
        private SimVarRequest<T> StartRequest<T>(
            uint definitionId,
            uint objectId,
            SimConnectPeriod period,
            Action<T>? onValue)
        {
            var requestId = Interlocked.Increment(ref this.nextRequestId);
            var request = new SimVarRequest<T>(requestId, objectId, definitionId, period, onValue);
            this.pendingRequests[requestId] = request;

            try
            {
                this.RequestDataOnSimObject(requestId, definitionId, objectId, period);
                return request;
            }
            catch
            {
                this.pendingRequests.TryRemove(requestId, out _);
                throw;
            }
        }

        /// <summary>
        /// Core handler for one-shot Get requests (always SimConnectPeriod.Once) that returns the retrieved value.
        /// </summary>
        /// <typeparam name="T">The value or struct type.</typeparam>
        /// <param name="definitionId">The SimConnect data definition id.</param>
        /// <param name="objectId">The target object id (usually SIMCONNECT_OBJECT_ID_USER).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The retrieved value.</returns>
        private async Task<T> GetAsyncCore<T>(uint definitionId, uint objectId, CancellationToken cancellationToken)
        {
            var request = this.StartRequest<T>(definitionId, objectId, SimConnectPeriod.Once, onValue: null);

            using (cancellationToken.Register(() => this.CancelRequest(request)))
            {
                Task<T> awaited = request.Task;
                if (this.requestTimeout != Timeout.InfiniteTimeSpan)
                {
                    var timeoutTask = Task.Delay(this.requestTimeout, CancellationToken.None);
                    var completed = await Task.WhenAny(awaited, timeoutTask).ConfigureAwait(false);
                    if (completed == timeoutTask)
                    {
                        this.pendingRequests.TryRemove(request.RequestId, out _);
                        throw new TimeoutException($"Request '{typeof(T).Name}' timed out after {this.requestTimeout} (RequestId={request.RequestId})");
                    }
                }

                var value = await awaited.ConfigureAwait(false);
                return value;
            }
        }

        private async Task SetWithDefinitionAsync<T>(SimVarDefinition definition, T value, uint objectId, CancellationToken cancellationToken)
        {
            var definitionId = this.EnsureDataDefinition(definition, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            // Offload blocking marshaling + native call to thread-pool so the async method can await
            await Task.Run(
                () =>
                {
                    // Allocate memory for the value
                    var dataSize = GetDataSize<T>();
                    var dataPtr = Marshal.AllocHGlobal(dataSize);

                    try
                    {
                        // Marshal the value to unmanaged memory
                        MarshalValue(value, dataPtr);

                        var result = SimConnectNative.SimConnect_SetDataOnSimObject(
                            this.simConnectHandle,
                            definitionId,
                            objectId,
                            0, // flags
                            1, // arrayCount
                            (uint)dataSize,
                            dataPtr);

                        if (result != (int)SimConnectError.None)
                        {
                            throw new SimConnectException($"Failed to set SimVar {definition.Name}: {(SimConnectError)result}", (SimConnectError)result);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(dataPtr);
                    }
                },
                cancellationToken).ConfigureAwait(false);
        }

        private uint EnsureDataDefinition(SimVarDefinition definition, CancellationToken cancellationToken)
        {
            var key = (Name: definition.Name, Unit: definition.Unit, DataType: definition.DataType);

            if (this.dataDefinitions.TryGetValue(key, out var existingId))
            {
                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"Reusing existing definition ID {existingId} for {key.Name}|{key.Unit}");
                }

                return existingId;
            }

            return this.EnsureScalarDefinition(definition.Name, definition.Unit, definition.DataType, cancellationToken);
        }

        /// <summary>
        /// Ensures a scalar (name + unit) data definition exists and registers its parser.
        /// Now encapsulates lookup and creation using primitive inputs.
        /// </summary>
        /// <param name="name">SimVar name.</param>
        /// <param name="unit">Unit string.</param>
        /// <param name="dataType">SimConnect data type.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Definition ID.</returns>
        private uint EnsureScalarDefinition(string name, string? unit = null, SimConnectDataType? dataType = null, CancellationToken cancellationToken = default)
        {
            if (unit == null || dataType == null)
            {
                var definition = SimVarRegistry.Get(name);

                if (definition != null)
                {
                    unit ??= definition.Unit;
                    dataType ??= definition.DataType;
                }
            }

            if (unit == null || dataType is not SimConnectDataType dt)
            {
                throw new ArgumentException($"Unit and dataType must not be null for SimVar '{name}'.");
            }

            var key = (Name: name, Unit: unit, DataType: dt);

            if (this.dataDefinitions.TryGetValue(key, out var existingId))
            {
                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"Reusing existing definition ID {existingId} for {key.Name}|{key.Unit}");
                }

                return existingId;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Re-check after potential waiting (none here but pattern consistent)
            if (this.dataDefinitions.TryGetValue(key, out existingId))
            {
                return existingId;
            }

            var definitionId = Interlocked.Increment(ref this.nextDefinitionId);
            if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
            {
                SimConnectLogger.Debug($"Creating new definition ID {definitionId} for {name}|{unit}");
            }

            var result = SimConnectNative.SimConnect_AddToDataDefinition(
                this.simConnectHandle,
                definitionId,
                name,
                unit,
                (uint)dataType);

            if (result != (int)SimConnectError.None)
            {
                throw new SimConnectException($"Failed to add data definition for {name}: {(SimConnectError)result}", (SimConnectError)result);
            }

            this.defToParser[definitionId] = (IntPtr ptr, ISimVarRequest req) =>
            {
                switch (dt)
                {
                    case SimConnectDataType.Integer32:
                        {
                            int v = SimVarMemoryReader.ReadInt32(ptr);
                            if (req.TrySetResult(v))
                            {
                                return;
                            }

                            if (req.TrySetResult(v != 0))
                            {
                                return;
                            }

                            req.SetResultBoxed(v);
                            return;
                        }

                    case SimConnectDataType.Integer64:
                        {
                            long v = SimVarMemoryReader.ReadInt64(ptr);
                            if (!req.TrySetResult(v))
                            {
                                req.SetResultBoxed(v);
                            }

                            return;
                        }

                    case SimConnectDataType.FloatSingle:
                        {
                            float v = SimVarMemoryReader.ReadFloat(ptr);
                            if (!req.TrySetResult(v))
                            {
                                req.SetResultBoxed((double)v);
                            }

                            return;
                        }

                    case SimConnectDataType.FloatDouble:
                        {
                            double v = SimVarMemoryReader.ReadDouble(ptr);
                            if (req.TrySetResult(v))
                            {
                                return;
                            }

                            if (req.TrySetResult((float)v))
                            {
                                return;
                            }

                            req.SetResultBoxed(v);
                            return;
                        }

                    case SimConnectDataType.String8:
                    case SimConnectDataType.String32:
                    case SimConnectDataType.String64:
                    case SimConnectDataType.String128:
                    case SimConnectDataType.String256:
                    case SimConnectDataType.String260:
                        {
                            string s = ParseString(ptr, dt);
                            if (!req.TrySetResult(s))
                            {
                                req.SetResultBoxed(s);
                            }

                            return;
                        }

                    case SimConnectDataType.LatLonAlt:
                        {
                            var v = Marshal.PtrToStructure<SimConnectDataLatLonAlt>(ptr);
                            if (!req.TrySetResult(v))
                            {
                                req.SetResultBoxed(v);
                            }

                            return;
                        }

                    case SimConnectDataType.Xyz:
                        {
                            var v = Marshal.PtrToStructure<SimConnectDataXyz>(ptr);
                            if (!req.TrySetResult(v))
                            {
                                req.SetResultBoxed(v);
                            }

                            return;
                        }

                    default:
                        throw new NotSupportedException($"Data type {dataType} is not supported");
                }
            };

            this.dataDefinitions[key] = definitionId;
            return definitionId;
        }

        /// <summary>
        /// Core handler that writes a struct T using the same field layout as EnsureTypeDefinition created.
        /// </summary>
        private async Task SetStructAsync<T>(uint definitionId, T value, uint objectId, CancellationToken cancellationToken)
            where T : struct
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Use cached write delegate/layout keyed by definitionId (must exist if EnsureTypeDefinition was used)
            if (!this.defToWriter.TryGetValue(definitionId, out var cache))
            {
                throw new InvalidOperationException($"No struct writer found for DefinitionId={definitionId}. EnsureTypeDefinition must be called first.");
            }

            await Task.Run(
                () =>
                {
                    var dataPtr = Marshal.AllocHGlobal(cache.TotalSize);
                    try
                    {
                        // Fill the buffer using the cached writer delegate for this definition
                        if (cache.Write is not Action<IntPtr, T> write)
                        {
                            throw new InvalidOperationException($"Cached writer has unexpected type for DefinitionId={definitionId} and T={typeof(T).Name}.");
                        }

                        write(dataPtr, value);

                        var hr = SimConnectNative.SimConnect_SetDataOnSimObject(
                            this.simConnectHandle,
                            definitionId,
                            objectId,
                            0,
                            1,
                            (uint)cache.TotalSize,
                            dataPtr);

                        if (hr != (int)SimConnectError.None)
                        {
                            throw new SimConnectException($"Failed to set struct '{typeof(T).Name}': {(SimConnectError)hr}", (SimConnectError)hr);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(dataPtr);
                    }
                },
                cancellationToken).ConfigureAwait(false);
        }
    }
}
