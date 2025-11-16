// <copyright file="FacilityManager.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.Internal;

namespace SimConnect.NET.Facilities
{
    /// <summary>
    /// Provides high-level helpers for working with the SimConnect facility APIs, including
    /// dynamic data definitions, ad-hoc facility data requests, and in-range subscriptions.
    /// </summary>
    public sealed class FacilityManager : IDisposable
    {
        private const uint BaseDefinitionId = 50000;
        private const uint BaseRequestId = 60000;

        private readonly IntPtr simConnectHandle;
        private readonly ISimConnectFacilityApi facilityApi;
        private readonly ConcurrentDictionary<Type, FacilityDefinition> definitionCache = new();
        private readonly ConcurrentDictionary<uint, FacilityMinimalListRequest> minimalListRequests = new();
        private readonly ConcurrentDictionary<uint, FacilityDataRequestState> facilityDataRequests = new();
        private readonly ConcurrentDictionary<SimConnectFacilityListType, FacilitySubscription> activeSubscriptions = new();
        private readonly ConcurrentDictionary<uint, FacilitySubscription> subscriptionLookup = new();
        private int definitionCounter = (int)BaseDefinitionId - 1;
        private int requestCounter = (int)BaseRequestId - 1;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacilityManager"/> class.
        /// </summary>
        /// <param name="simConnectHandle">The active SimConnect handle.</param>
        public FacilityManager(IntPtr simConnectHandle)
            : this(simConnectHandle, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FacilityManager"/> class for testing.
        /// </summary>
        /// <param name="simConnectHandle">The active SimConnect handle.</param>
        /// <param name="facilityApi">Optional facility API abstraction used for testing.</param>
        internal FacilityManager(IntPtr simConnectHandle, ISimConnectFacilityApi? facilityApi)
        {
            if (simConnectHandle == IntPtr.Zero)
            {
                throw new ArgumentException("SimConnect handle must be initialized.", nameof(simConnectHandle));
            }

            this.simConnectHandle = simConnectHandle;
            this.facilityApi = facilityApi ?? SimConnectFacilityApi.Instance;
        }

        /// <summary>
        /// Creates a facility definition from the supplied builder configuration.
        /// </summary>
        /// <param name="configure">Action that defines the desired facility fields.</param>
        /// <returns>A <see cref="FacilityDefinition"/> instance bound to a SimConnect definition ID.</returns>
        public FacilityDefinition CreateDefinition(Action<FacilityDefinitionBuilder> configure)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(FacilityManager));
            ArgumentNullException.ThrowIfNull(configure);

            var builder = new FacilityDefinitionBuilder();
            configure(builder);
            if (builder.Fields.Count == 0)
            {
                throw new InvalidOperationException("At least one facility field must be added to the definition.");
            }

            var definitionId = this.GetNextDefinitionId();
            foreach (var field in builder.Fields)
            {
                ThrowIfError(
                    this.facilityApi.AddToFacilityDefinition(this.simConnectHandle, definitionId, field),
                    $"Add facility field '{field}'");
            }

            return new FacilityDefinition(definitionId, builder.Fields.ToArray(), null);
        }

        /// <summary>
        /// Creates or retrieves a cached facility definition using <see cref="FacilityFieldAttribute"/> annotations.
        /// </summary>
        /// <typeparam name="T">Struct containing annotated fields.</typeparam>
        /// <returns>The cached <see cref="FacilityDefinition"/>.</returns>
        public FacilityDefinition GetOrCreateDefinition<T>()
            where T : struct
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(FacilityManager));
            return this.definitionCache.GetOrAdd(typeof(T), _ => this.CreateDefinitionFromStruct<T>());
        }

        /// <summary>
        /// Requests facility data using a previously created definition.
        /// </summary>
        /// <param name="definition">The facility definition to use.</param>
        /// <param name="icao">The ICAO identifier of the facility.</param>
        /// <param name="region">Optional region string.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task returning the full response payload.</returns>
        public Task<FacilityDataResponse> RequestFacilityDataAsync(
            FacilityDefinition definition,
            string icao,
            string? region = null,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(FacilityManager));
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentException.ThrowIfNullOrEmpty(icao);

            var requestId = this.GetNextRequestId();
            var requestState = new FacilityDataRequestState(requestId);
            if (!this.facilityDataRequests.TryAdd(requestId, requestState))
            {
                throw new InvalidOperationException("Failed to track facility data request state.");
            }

            cancellationToken.Register(() =>
            {
                if (this.facilityDataRequests.TryRemove(requestId, out var canceled))
                {
                    canceled.TrySetCanceled();
                }
            });

            ThrowIfError(
                this.facilityApi.RequestFacilityData(this.simConnectHandle, definition.DefinitionId, requestId, icao, region ?? string.Empty),
                "RequestFacilityData");

            return requestState.Task;
        }

        /// <summary>
        /// Requests facility data for the specified annotated struct type.
        /// </summary>
        /// <typeparam name="T">The annotated struct type describing the desired fields.</typeparam>
        /// <param name="icao">The ICAO identifier.</param>
        /// <param name="region">Optional region code.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>The complete <see cref="FacilityDataResponse"/>.</returns>
        public Task<FacilityDataResponse> RequestFacilityDataAsync<T>(
            string icao,
            string? region = null,
            CancellationToken cancellationToken = default)
            where T : struct
            => this.RequestFacilityDataAsync(this.GetOrCreateDefinition<T>(), icao, region, cancellationToken);

        /// <summary>
        /// Requests the minimal facility list (ICAO and Lat/Lon/Alt) for the specified type.
        /// </summary>
        /// <param name="type">The facility list type (airport, waypoint, etc.).</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        /// <returns>A task that completes with the full list of facilities.</returns>
        public Task<IReadOnlyList<SimConnectFacilityMinimal>> RequestMinimalListAsync(
            SimConnectFacilityListType type,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(FacilityManager));
            var requestId = this.GetNextRequestId();
            var state = new FacilityMinimalListRequest(requestId);
            if (!this.minimalListRequests.TryAdd(requestId, state))
            {
                throw new InvalidOperationException("Failed to track facility list request state.");
            }

            cancellationToken.Register(() =>
            {
                if (this.minimalListRequests.TryRemove(requestId, out var canceled))
                {
                    canceled.TrySetCanceled();
                }
            });

            ThrowIfError(
                this.facilityApi.RequestFacilitiesListEx1(this.simConnectHandle, (uint)type, requestId),
                "RequestFacilitiesList_EX1");

            return state.Task;
        }

        /// <summary>
        /// Subscribes to facilities entering and leaving the user's reality bubble.
        /// </summary>
        /// <param name="type">Facility list type to monitor.</param>
        /// <param name="onEntered">Callback invoked when facilities enter range.</param>
        /// <param name="onExited">Optional callback for facilities leaving range.</param>
        /// <returns>A disposable subscription object.</returns>
        public FacilitySubscription SubscribeToMinimalFacilities(
            SimConnectFacilityListType type,
            Action<IReadOnlyList<SimConnectFacilityMinimal>> onEntered,
            Action<IReadOnlyList<SimConnectFacilityMinimal>>? onExited = null)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(FacilityManager));
            ArgumentNullException.ThrowIfNull(onEntered);

            var inRangeRequestId = this.GetNextRequestId();
            var outRangeRequestId = this.GetNextRequestId();
            var subscription = new FacilitySubscription(this, type, inRangeRequestId, outRangeRequestId, onEntered, onExited);

            if (!this.activeSubscriptions.TryAdd(type, subscription))
            {
                throw new InvalidOperationException($"A subscription for {type} already exists. Dispose the existing subscription before creating a new one.");
            }

            this.subscriptionLookup[inRangeRequestId] = subscription;
            this.subscriptionLookup[outRangeRequestId] = subscription;

            try
            {
                ThrowIfError(
                    this.facilityApi.SubscribeToFacilitiesEx1(this.simConnectHandle, (uint)type, inRangeRequestId, outRangeRequestId),
                    "SubscribeToFacilities_EX1");
            }
            catch
            {
                this.subscriptionLookup.TryRemove(inRangeRequestId, out _);
                this.subscriptionLookup.TryRemove(outRangeRequestId, out _);
                this.activeSubscriptions.TryRemove(type, out _);
                throw;
            }

            return subscription;
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            foreach (var subscription in this.activeSubscriptions.Values)
            {
                subscription?.Dispose();
            }

            this.disposed = true;
        }

        internal void RemoveSubscription(FacilitySubscription subscription)
        {
            if (subscription == null)
            {
                return;
            }

            this.subscriptionLookup.TryRemove(subscription.EnteredRequestId, out _);
            this.subscriptionLookup.TryRemove(subscription.ExitedRequestId, out _);
            this.activeSubscriptions.TryRemove(subscription.Type, out _);
            try
            {
                ThrowIfError(
                    this.facilityApi.UnsubscribeFromFacilitiesEx1(this.simConnectHandle, (uint)subscription.Type, true, true),
                    "UnsubscribeToFacilities_EX1");
            }
            catch (SimConnectException ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Warning($"Failed to unsubscribe from facilities: {ex.Message}");
            }
        }

        internal void ProcessFacilityMinimalList(IntPtr data)
        {
            if (data == IntPtr.Zero)
            {
                return;
            }

            var header = Marshal.PtrToStructure<FacilityMinimalListHeader>(data)!;
            var dataStart = IntPtr.Add(data, FacilityMinimalListHeader.SizeInBytes);
            var list = FacilityManager.MarshalArray<SimConnectFacilityMinimal>(dataStart, (int)header.ArraySize);

            if (this.subscriptionLookup.TryGetValue(header.RequestId, out var subscription))
            {
                subscription.Dispatch(header.RequestId, list);
                return;
            }

            if (this.minimalListRequests.TryGetValue(header.RequestId, out var request))
            {
                request.AddChunk(list, header.EntryNumber, header.OutOf);
                if (request.IsComplete)
                {
                    this.minimalListRequests.TryRemove(header.RequestId, out _);
                }

                return;
            }
        }

        internal void ProcessFacilityData(IntPtr data)
        {
            if (data == IntPtr.Zero)
            {
                return;
            }

            var recv = Marshal.PtrToStructure<SimConnectRecvFacilityDataInternal>(data)!;
            if (this.facilityDataRequests.TryGetValue(recv.UserRequestId, out var state))
            {
                state.AddPacket(data, recv);
            }
        }

        internal void ProcessFacilityDataEnd(IntPtr data)
        {
            if (data == IntPtr.Zero)
            {
                return;
            }

            var recvEnd = Marshal.PtrToStructure<SimConnectRecvFacilityDataEnd>(data)!;
            if (this.facilityDataRequests.TryRemove(recvEnd.RequestId, out var state))
            {
                state.Complete();
            }
        }

        private static void ThrowIfError(int result, string operation)
        {
            if (result == (int)SimConnectError.None)
            {
                return;
            }

            var error = (SimConnectError)result;
            var message = $"{operation} failed: {SimConnectErrorMapper.Describe(error)}";
            throw new SimConnectException(message, error);
        }

        private static T[] MarshalArray<T>(IntPtr dataStart, int count)
            where T : struct
        {
            if (count <= 0 || dataStart == IntPtr.Zero)
            {
                return Array.Empty<T>();
            }

            var elementSize = Marshal.SizeOf<T>();
            var result = new T[count];
            for (var i = 0; i < count; i++)
            {
                var elementPtr = IntPtr.Add(dataStart, i * elementSize);
                result[i] = Marshal.PtrToStructure<T>(elementPtr)!;
            }

            return result;
        }

        private FacilityDefinition CreateDefinitionFromStruct<T>()
            where T : struct
        {
            var fields = typeof(T)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(f => (Field: f, Attribute: f.GetCustomAttribute<FacilityFieldAttribute>()))
                .Where(tuple => tuple.Attribute != null)
                .OrderBy(tuple => tuple.Attribute!.Order)
                .ThenBy(tuple => tuple.Field.MetadataToken)
                .Select(tuple => tuple.Attribute!.Path)
                .ToList();

            if (fields.Count == 0)
            {
                throw new InvalidOperationException($"Type {typeof(T).FullName} does not define any FacilityFieldAttribute annotations.");
            }

            var definitionId = this.GetNextDefinitionId();
            foreach (var field in fields)
            {
                ThrowIfError(
                    this.facilityApi.AddToFacilityDefinition(this.simConnectHandle, definitionId, field),
                    $"Add facility field '{field}'");
            }

            return new FacilityDefinition(definitionId, fields, typeof(T));
        }

        private uint GetNextDefinitionId() => (uint)Interlocked.Increment(ref this.definitionCounter);

        private uint GetNextRequestId() => (uint)Interlocked.Increment(ref this.requestCounter);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FacilityMinimalListHeader
        {
            public static readonly int SizeInBytes = Marshal.SizeOf<FacilityMinimalListHeader>();

            public uint Size;
            public uint Version;
            public uint Id;
            public uint RequestId;
            public uint ArraySize;
            public uint EntryNumber;
            public uint OutOf;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SimConnectRecvFacilityDataInternal
        {
            public static readonly int DataOffset = Marshal.OffsetOf<SimConnectRecvFacilityDataInternal>(nameof(Data)).ToInt32();

            public uint Size;
            public uint Version;
            public uint Id;
            public uint UserRequestId;
            public uint UniqueRequestId;
            public uint ParentUniqueRequestId;
            public SimConnectFacilityDataType Type;
            public uint IsListItem;
            public uint ItemIndex;
            public uint ListSize;
            public uint Data;
        }

        private sealed class FacilityMinimalListRequest
        {
            private readonly TaskCompletionSource<IReadOnlyList<SimConnectFacilityMinimal>> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly List<SimConnectFacilityMinimal> buffer = new();
            private uint expectedPackets;
            private uint receivedPackets;

            internal FacilityMinimalListRequest(uint requestId)
            {
            }

            internal Task<IReadOnlyList<SimConnectFacilityMinimal>> Task => this.tcs.Task;

            internal bool IsComplete => this.expectedPackets != 0 && this.receivedPackets >= this.expectedPackets;

            internal void AddChunk(SimConnectFacilityMinimal[] chunk, uint entryNumber, uint outOf)
            {
                if (chunk.Length > 0)
                {
                    this.buffer.AddRange(chunk);
                }

                this.receivedPackets++;
                this.expectedPackets = outOf == 0 ? 1u : outOf;

                if (this.IsComplete)
                {
                    this.tcs.TrySetResult(this.buffer);
                }
            }

            internal void TrySetCanceled()
            {
                this.tcs.TrySetCanceled();
            }
        }

        private sealed class FacilityDataRequestState
        {
            private readonly TaskCompletionSource<FacilityDataResponse> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly List<FacilityDataResult> packets = new();
            private readonly uint requestId;

            internal FacilityDataRequestState(uint requestId)
            {
                this.requestId = requestId;
            }

            internal Task<FacilityDataResponse> Task => this.tcs.Task;

            internal void AddPacket(IntPtr basePtr, SimConnectRecvFacilityDataInternal data)
            {
                var payloadSize = (int)data.Size - SimConnectRecvFacilityDataInternal.DataOffset;
                if (payloadSize < 0)
                {
                    payloadSize = 0;
                }

                var payload = new byte[payloadSize];
                if (payloadSize > 0)
                {
                    var payloadPtr = IntPtr.Add(basePtr, SimConnectRecvFacilityDataInternal.DataOffset);
                    Marshal.Copy(payloadPtr, payload, 0, payloadSize);
                }

                var result = new FacilityDataResult(
                    data.UniqueRequestId,
                    data.ParentUniqueRequestId,
                    data.Type,
                    data.IsListItem != 0,
                    data.ItemIndex,
                    data.ListSize,
                    payload);

                this.packets.Add(result);
            }

            internal void Complete()
            {
                this.tcs.TrySetResult(new FacilityDataResponse(this.requestId, this.packets));
            }

            internal void TrySetCanceled()
            {
                this.tcs.TrySetCanceled();
            }
        }
    }
}
