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
        private static readonly ThreadLocal<byte[]> Float32Bytes = new(() => new byte[4]);
        private static readonly ThreadLocal<byte[]> Float64Bytes = new(() => new byte[8]);

        private readonly IntPtr simConnectHandle;
        private readonly ConcurrentDictionary<uint, object> pendingRequests;
        private readonly ConcurrentDictionary<(string Name, string Unit), uint> dataDefinitions;
        private readonly ConcurrentDictionary<uint, (System.Reflection.FieldInfo Field, SimConnectAttribute Attr)[]> defIndex = new();
        private readonly ConcurrentDictionary<Type, uint> typeToDefIndex = new();

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
            this.pendingRequests = new ConcurrentDictionary<uint, object>();
            this.dataDefinitions = new ConcurrentDictionary<(string, string), uint>();
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
        public async Task<T> GetAsync<T>(string simVarName, string unit, uint objectId = SimConnectObjectIdUser, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            ArgumentException.ThrowIfNullOrEmpty(simVarName);
            ArgumentException.ThrowIfNullOrEmpty(unit);

            // Try to get definition from registry first
            var definition = SimVarRegistry.Get(simVarName);
            if (definition != null)
            {
                // Validate the requested type matches the definition
                if (!IsTypeCompatible(typeof(T), definition.NetType))
                {
                    throw new ArgumentException($"Type {typeof(T).Name} is not compatible with SimVar {simVarName} which expects {definition.NetType.Name}");
                }

                return await this.GetWithDefinitionAsync<T>(definition, objectId, cancellationToken).ConfigureAwait(false);
            }

            // If not in registry, create a dynamic definition
            var dataType = InferDataType<T>();
            var dynamicDefinition = new SimVarDefinition(simVarName, unit, dataType, false, "Dynamically created definition");

            return await this.GetWithDefinitionAsync<T>(dynamicDefinition, objectId, cancellationToken).ConfigureAwait(false);
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
            ArgumentException.ThrowIfNullOrEmpty(unit);

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
        /// Gets a full struct from SimConnect as a strongly-typed object using a dynamically built data definition.
        /// </summary>
        /// <typeparam name="T">The struct type to request. Must be blittable/marshalable.</typeparam>
        /// <param name="objectId">The SimConnect object ID (defaults to user aircraft).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation and returns the requested struct.</returns>
        public async Task<T> GetAsync<T>(
            uint objectId = SimConnectObjectIdUser,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimVarManager));
            cancellationToken.ThrowIfCancellationRequested();

            if (this.simConnectHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("SimConnect handle is not initialized.");
            }

            // Build definition for the struct using the native handle directly to avoid client coupling.
            if (!this.typeToDefIndex.TryGetValue(typeof(T), out var defId))
            {
                var (newDefId, fields) = SimVarStructBinder.BuildAndRegisterFromStruct<T>(this.simConnectHandle);
                this.defIndex[newDefId] = fields;
                this.typeToDefIndex[typeof(T)] = newDefId;
                defId = newDefId;
            }

            var requestId = Interlocked.Increment(ref this.nextRequestId);
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.pendingRequests[requestId] = tcs;

            // Request data once for the specified object.
            var hr = SimConnectNative.SimConnect_RequestDataOnSimObject(
                this.simConnectHandle,
                requestId,
                defId,
                objectId,
                (uint)SimConnectPeriod.Once);

            if (hr != (int)SimConnectError.None)
            {
                this.pendingRequests.TryRemove(requestId, out _);
                throw new SimConnectException($"Failed to request struct {typeof(T).Name}: {(SimConnectError)hr}", (SimConnectError)hr);
            }

            using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
            {
                // Apply optional manager timeout consistent with SimVar value requests
                if (this.requestTimeout != Timeout.InfiniteTimeSpan)
                {
                    var timeoutTask = Task.Delay(this.requestTimeout, CancellationToken.None);
                    var completed = await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);
                    if (completed == timeoutTask)
                    {
                        this.pendingRequests.TryRemove(requestId, out _);
                        throw new TimeoutException($"Struct request '{typeof(T).Name}' timed out after {this.requestTimeout} (RequestId={requestId})");
                    }
                }

                return await tcs.Task.ConfigureAwait(false);
            }
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

                if (recv.Id == (uint)SimConnectRecvId.SimobjectData)
                {
                    // Parse the full SimConnectRecvSimObjectData structure (which includes the base SimConnectRecv)
                    var objectData = Marshal.PtrToStructure<SimConnectRecvSimObjectData>(data);
                    var requestId = objectData.RequestId;

                    if (this.pendingRequests.TryRemove(requestId, out var request))
                    {
                        // If this is a simple SimVarRequest<>, use existing completion logic
                        if (request.GetType().IsGenericType && request.GetType().GetGenericTypeDefinition() == typeof(SimVarRequest<>))
                        {
                            CompleteRequest(request, data, dataSize);
                        }
                        else
                        {
                            // Otherwise, treat as a struct T TaskCompletionSource stored by GetAsync<T>() struct overload
                            try
                            {
                                var definitionId = objectData.DefineId;
                                if (!this.defIndex.TryGetValue(definitionId, out var fields))
                                {
                                    throw new InvalidOperationException($"No struct definition found for DefineId={definitionId}");
                                }
                                else
                                {
                                    var headerSize = Marshal.SizeOf<SimConnectRecvSimObjectData>() - sizeof(ulong);
                                    var dataPtr = IntPtr.Add(data, headerSize);
                                    var currentPtr = dataPtr;
                                    var tcsType = request.GetType();
                                    if (tcsType.IsGenericType && tcsType.GetGenericTypeDefinition() == typeof(TaskCompletionSource<>))
                                    {
                                        var structType = tcsType.GetGenericArguments()[0];
                                        var boxed = Activator.CreateInstance(structType)!;
                                        foreach (var (field, attr) in fields)
                                        {
                                            var simConnectType = attr.DataType;
                                            var fieldType = field.FieldType;
                                            var simConnectSize = SimVarDataTypeSizing.GetDatumSizeBytes(simConnectType);
                                            var fieldName = field.Name;
                                            object? value = ParseType(currentPtr, simConnectType, fieldType);
                                            field.SetValue(boxed, value);
                                            currentPtr = IntPtr.Add(currentPtr, simConnectSize);
                                        }

                                        var trySetResultMethod = tcsType.GetMethod("TrySetResult");
                                        trySetResultMethod?.Invoke(request, new[] { boxed });
                                    }
                                    else
                                    {
                                        SimConnectLogger.Warning("Unexpected TCS type for struct response.");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                SimConnectLogger.Error("Error completing struct request", ex);
                                var tcsType = request.GetType();
                                var setEx = tcsType.GetMethod("TrySetException", new[] { typeof(Exception) });
                                setEx?.Invoke(request, new object[] { ex });
                            }
                        }
                    }
                    else
                    {
                        SimConnectLogger.Warning($"No pending request found for RequestId={requestId}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - this shouldn't break the message processing loop
                SimConnectLogger.Error("Error processing SimVar data", ex);
            }
        }

        /// <summary>
        /// Disposes the SimVar manager and cancels all pending requests.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                // Cancel all pending requests
                foreach (var kvp in this.pendingRequests)
                {
                    CancelRequest(kvp.Value);
                }

                this.pendingRequests.Clear();
                this.dataDefinitions.Clear();
                this.disposed = true;
            }
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

        private static void CompleteRequest(object request, IntPtr data, uint dataSize)
        {
            var requestType = request.GetType();
            if (!requestType.IsGenericType || requestType.GetGenericTypeDefinition() != typeof(SimVarRequest<>))
            {
                return;
            }

            try
            {
                var valueType = requestType.GetGenericArguments()[0];
                var simVarRequest = request as dynamic;
                var definition = simVarRequest?.Definition as SimVarDefinition;

                if (definition == null)
                {
                    var setExceptionMethod = requestType.GetMethod("SetException");
                    setExceptionMethod?.Invoke(request, new object[] { new InvalidOperationException("SimVar definition is null") });
                    return;
                }

                // Calculate the offset to the actual data
                // The data follows the SimConnectRecvSimObjectData structure
                // The actual data starts after the fixed header (Size, Version, Id, RequestId, ObjectId, DefineId, Flags, EntryNumber, OutOf, DefineCount)
                var headerSize = Marshal.SizeOf<SimConnectRecvSimObjectData>() - sizeof(ulong); // Subtract the Data field which is part of the actual data
                var dataPtr = IntPtr.Add(data, headerSize);

                SimConnectLogger.Debug($"Completing request for {definition.Name}, DataType={definition.DataType}, ValueType={valueType.Name}, HeaderSize={headerSize}");

                // Parse the data based on the definition's data type
                var parsedValue = ParseDataByType(dataPtr, definition.DataType, valueType);

                // Use reflection to call SetResult directly, avoiding dynamic
                var setResultMethod = requestType.GetMethod("SetResult");
                setResultMethod?.Invoke(request, new[] { parsedValue });
            }
            catch (Exception ex)
            {
                SimConnectLogger.Error("Error completing request", ex);
                var setExceptionMethod = requestType.GetMethod("SetException");
                setExceptionMethod?.Invoke(request, new object[] { ex });
            }
        }

        private static object? ParseDataByType(IntPtr dataPtr, SimConnectDataType dataType, Type expectedType)
        {
            return dataType switch
            {
                SimConnectDataType.Integer32 => ParseInteger32Value(dataPtr, expectedType),
                SimConnectDataType.Integer64 => (object)ParseInteger64(dataPtr),
                SimConnectDataType.FloatSingle => (object)ParseFloat32(dataPtr),
                SimConnectDataType.FloatDouble => ParseFloat64Value(dataPtr, expectedType),
                SimConnectDataType.String8 or
                SimConnectDataType.String32 or
                SimConnectDataType.String64 or
                SimConnectDataType.String128 or
                SimConnectDataType.String256 or
                SimConnectDataType.String260 or
                SimConnectDataType.StringV => ParseString(dataPtr, dataType),
                SimConnectDataType.LatLonAlt => Marshal.PtrToStructure<SimConnectDataLatLonAlt>(dataPtr),
                SimConnectDataType.Xyz => Marshal.PtrToStructure<SimConnectDataXyz>(dataPtr),
                _ => throw new NotSupportedException($"Data type {dataType} is not supported"),
            };
        }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        private static object ParseInteger32Value(IntPtr dataPtr, Type expectedType)
        {
            var value = ParseInteger32(dataPtr);

            // Handle boolean conversion
            if (expectedType == typeof(bool))
            {
                return value != 0;
            }

            return value;
        }
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

        private static int ParseInteger32(IntPtr dataPtr)
        {
            return Marshal.ReadInt32(dataPtr);
        }

        private static long ParseInteger64(IntPtr dataPtr)
        {
            return Marshal.ReadInt64(dataPtr);
        }

        private static float ParseFloat32(IntPtr dataPtr)
        {
            var bytes = Float32Bytes.Value!;
            Marshal.Copy(dataPtr, bytes, 0, 4);
            return BitConverter.ToSingle(bytes, 0);
        }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        private static object ParseFloat64Value(IntPtr dataPtr, Type expectedType)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        {
            var value = ParseFloat64(dataPtr);

            // Handle float conversion
            if (expectedType == typeof(float))
            {
                return (float)value;
            }

            return value;
        }

        private static object? ParseType(IntPtr dataPtr, SimConnectDataType simConnectType, Type fieldType)
        {
            return fieldType switch
            {
            Type t when t == typeof(int) => ParseInteger32(dataPtr),
            Type t when t == typeof(long) => ParseInteger64(dataPtr),
            Type t when t == typeof(float) => ParseFloat32(dataPtr),
            Type t when t == typeof(double) => ParseFloat64(dataPtr),
            Type t when t == typeof(string) => ParseString(dataPtr, simConnectType),
            Type t when t == typeof(bool) => ParseInteger32(dataPtr) != 0,
            _ => Marshal.PtrToStructure(dataPtr, fieldType),
            };
        }

        private static double ParseFloat64(IntPtr dataPtr)
        {
            var bytes = Float64Bytes.Value!;
            Marshal.Copy(dataPtr, bytes, 0, 8);
            return BitConverter.ToDouble(bytes, 0);
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
                SimConnectDataType.StringV => 256, // Default for variable length
                _ => 256,
            };

            return Marshal.PtrToStringAnsi(dataPtr, maxLength)?.TrimEnd('\0') ?? string.Empty;
        }

        private static void CancelRequest(object request)
        {
            var requestType = request.GetType();
            if (requestType.IsGenericType && requestType.GetGenericTypeDefinition() == typeof(SimVarRequest<>))
            {
                var method = requestType.GetMethod("SetCanceled");
                method?.Invoke(request, null);
            }
        }

        private async Task<T> GetWithDefinitionAsync<T>(SimVarDefinition definition, uint objectId, CancellationToken cancellationToken)
        {
            var definitionId = await this.EnsureDataDefinitionAsync(definition, cancellationToken).ConfigureAwait(false);
            var requestId = Interlocked.Increment(ref this.nextRequestId);

            var request = new SimVarRequest<T>(requestId, definition, objectId);
            this.pendingRequests[requestId] = request;

            SimConnectLogger.Debug($"Making SimVar request: {definition.Name}, RequestId={requestId}, DefinitionId={definitionId}");

            try
            {
                var result = SimConnectNative.SimConnect_RequestDataOnSimObject(
                    this.simConnectHandle,
                    requestId,
                    definitionId,
                    objectId,
                    (uint)SimConnectPeriod.Once);

                if (result != (int)SimConnectError.None)
                {
                    this.pendingRequests.TryRemove(requestId, out _);
                    throw new SimConnectException($"Failed to request SimVar {definition.Name}: {(SimConnectError)result}", (SimConnectError)result);
                }

                SimConnectLogger.Debug($"SimConnect_RequestDataOnSimObject succeeded for RequestId={requestId}");

                // Wait for the response with cancellation support
                using (cancellationToken.Register(() => request.SetCanceled()))
                {
                    Task<T> taskToAwait = request.Task;
                    if (this.requestTimeout != Timeout.InfiniteTimeSpan)
                    {
                        var timeoutTask = Task.Delay(this.requestTimeout, CancellationToken.None);
                        var completed = await Task.WhenAny(taskToAwait, timeoutTask).ConfigureAwait(false);
                        if (completed == timeoutTask)
                        {
                            this.pendingRequests.TryRemove(requestId, out _);
                            request.SetException(new TimeoutException($"SimVar request '{definition.Name}' timed out after {this.requestTimeout} (RequestId={requestId})"));
                        }
                    }

                    var result_task = await taskToAwait.ConfigureAwait(false);
                    SimConnectLogger.Info($"SimVar request completed successfully: {definition.Name} = {result_task}");
                    return result_task;
                }
            }
            catch
            {
                this.pendingRequests.TryRemove(requestId, out _);
                throw;
            }
        }

        private async Task SetWithDefinitionAsync<T>(SimVarDefinition definition, T value, uint objectId, CancellationToken cancellationToken)
        {
            var definitionId = await this.EnsureDataDefinitionAsync(definition, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

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
        }

        private Task<uint> EnsureDataDefinitionAsync(SimVarDefinition definition, CancellationToken cancellationToken)
        {
            var key = (definition.Name, definition.Unit);

            if (this.dataDefinitions.TryGetValue(key, out var existingId))
            {
                SimConnectLogger.Debug($"Reusing existing definition ID {existingId} for {key.Name}|{key.Unit}");
                return Task.FromResult(existingId);
            }

            return Task.Run(
                () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Re-check in case another thread added it while we were waiting to run
                if (this.dataDefinitions.TryGetValue(key, out var recheckedId))
                {
                    return recheckedId;
                }

                var definitionId = Interlocked.Increment(ref this.nextDefinitionId);
                SimConnectLogger.Debug($"Creating new definition ID {definitionId} for {key.Name}|{key.Unit}");

                var result = SimConnectNative.SimConnect_AddToDataDefinition(
                    this.simConnectHandle,
                    definitionId,
                    definition.Name,
                    definition.Unit,
                    (uint)definition.DataType);

                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to add data definition for {definition.Name}: {(SimConnectError)result}", (SimConnectError)result);
                }

                this.dataDefinitions[key] = definitionId;
                return definitionId;
            },
                cancellationToken);
        }
    }
}
