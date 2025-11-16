// <copyright file="FacilityManagerTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SimConnect.NET;
using SimConnect.NET.Facilities;
using SimConnect.NET.Internal;
using Xunit;

namespace SimConnect.NET.UnitTests
{
    /// <summary>
    /// Unit tests exercising the <see cref="FacilityManager"/> surface.
    /// </summary>
    public class FacilityManagerTests
    {
        /// <summary>
        /// Verifies that custom definitions register every requested field.
        /// </summary>
        [Fact]
        public void CreateDefinition_RegistersAllFields()
        {
            var api = new FakeFacilityApi();
            using var manager = new FacilityManager(new IntPtr(1), api);

            var definition = manager.CreateDefinition(builder => builder.AddField("Airport.Latitude").AddField("Airport.Longitude"));

            Assert.Equal(2, api.AddedFields.Count);
            Assert.Equal(definition.DefinitionId, api.AddedFields[0].DefinitionId);
        }

        /// <summary>
        /// Ensures that minimal facility list requests complete when data arrives.
        /// </summary>
        /// <returns>A task that completes when the test has validated the behavior.</returns>
        [Fact]
        public async Task MinimalListRequest_CompletesWhenMessageArrives()
        {
            var api = new FakeFacilityApi();
            using var manager = new FacilityManager(new IntPtr(1), api);

            var requestTask = manager.RequestMinimalListAsync(SimConnectFacilityListType.Airport);
            var requestId = api.MinimalRequests.Single().RequestId;
            var facilities = new[]
            {
                new SimConnectFacilityMinimal
                {
                    Icao = SimConnectIcao.FromStrings("KJFK"),
                    LatLonAlt = new SimConnectDataLatLonAlt { Latitude = 40.64, Longitude = -73.78, Altitude = 13 },
                },
            };

            using (var message = FacilityMessageBuilder.CreateMinimalListMessage(requestId, facilities))
            {
                manager.ProcessFacilityMinimalList(message.Pointer);
            }

            var result = await requestTask;
            Assert.Single(result);
            Assert.Equal("KJFK", result[0].Icao.Icao);
        }

        /// <summary>
        /// Validates that facility data requests return all received payloads.
        /// </summary>
        /// <returns>A task that completes when the test has validated the behavior.</returns>
        [Fact]
        public async Task FacilityDataRequest_ReturnsPayloads()
        {
            var api = new FakeFacilityApi();
            using var manager = new FacilityManager(new IntPtr(1), api);

            var definition = manager.CreateDefinition(b => b.AddField("Airport.Name"));
            var task = manager.RequestFacilityDataAsync(definition, "TEST");
            var request = api.DataRequests.Single();
            Assert.Equal("TEST", request.Icao);

            var payload = new byte[16];
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)(i + 1);
            }

            using (var message = FacilityMessageBuilder.CreateFacilityDataMessage(request.RequestId, SimConnectFacilityDataType.Airport, payload))
            {
                manager.ProcessFacilityData(message.Pointer);
            }

            var end = new SimConnectRecvFacilityDataEnd
            {
                Size = (uint)Marshal.SizeOf<SimConnectRecvFacilityDataEnd>(),
                Version = 1,
                Id = (uint)SimConnectRecvId.FacilityDataEnd,
                RequestId = request.RequestId,
            };

            IntPtr endPtr = Marshal.AllocHGlobal(Marshal.SizeOf<SimConnectRecvFacilityDataEnd>());
            try
            {
                Marshal.StructureToPtr(end, endPtr, false);
                manager.ProcessFacilityDataEnd(endPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(endPtr);
            }

            var response = await task;
            Assert.Single(response.Results);
            Assert.Equal(payload, response.Results[0].Payload.ToArray());
        }

        /// <summary>
        /// Ensures that subscription callbacks fire when facilities enter and exit range.
        /// </summary>
        [Fact]
        public void Subscription_DispatchesCallbacks()
        {
            var api = new FakeFacilityApi();
            using var manager = new FacilityManager(new IntPtr(1), api);

            List<string> entered = new();
            List<string> exited = new();

            using var subscription = manager.SubscribeToMinimalFacilities(
                SimConnectFacilityListType.Airport,
                facilities => entered.AddRange(facilities.Select(f => f.Icao.Icao)),
                facilities => exited.AddRange(facilities.Select(f => f.Icao.Icao)));

            var entry = api.Subscriptions.Single();
            var facilities = new[]
            {
                new SimConnectFacilityMinimal
                {
                    Icao = SimConnectIcao.FromStrings("SUB1"),
                    LatLonAlt = default,
                },
            };

            using (var enterMessage = FacilityMessageBuilder.CreateMinimalListMessage(entry.EnterRequestId, facilities))
            {
                manager.ProcessFacilityMinimalList(enterMessage.Pointer);
            }

            using (var exitMessage = FacilityMessageBuilder.CreateMinimalListMessage(entry.ExitRequestId, facilities))
            {
                manager.ProcessFacilityMinimalList(exitMessage.Pointer);
            }

            Assert.Equal(new[] { "SUB1" }, entered);
            Assert.Equal(new[] { "SUB1" }, exited);
        }

        /// <summary>
        /// Provides helper methods to create simulated facility messages.
        /// </summary>
        private static class FacilityMessageBuilder
        {
            /// <summary>
            /// Creates a simulated facility minimal list message.
            /// </summary>
            /// <param name="requestId">The request identifier associated with the list.</param>
            /// <param name="facilities">The facilities included in the message.</param>
            /// <returns>A disposable message handle.</returns>
            public static FacilityMessageHandle CreateMinimalListMessage(uint requestId, IReadOnlyList<SimConnectFacilityMinimal> facilities)
            {
                var header = new MinimalListHeader
                {
                    Size = (uint)(MinimalListHeader.SizeInBytes + (Marshal.SizeOf<SimConnectFacilityMinimal>() * facilities.Count)),
                    Version = 1,
                    Id = (uint)SimConnectRecvId.FacilityMinimalList,
                    RequestId = requestId,
                    ArraySize = (uint)facilities.Count,
                    EntryNumber = 0,
                    OutOf = 1,
                };

                var headerBytes = StructureToBytes(header);
                var elementSize = Marshal.SizeOf<SimConnectFacilityMinimal>();
                var totalSize = headerBytes.Length + (elementSize * facilities.Count);
                var buffer = new byte[totalSize];
                Array.Copy(headerBytes, buffer, headerBytes.Length);

                if (facilities.Count > 0)
                {
                    var elementPtr = Marshal.AllocHGlobal(elementSize);
                    try
                    {
                        for (int i = 0; i < facilities.Count; i++)
                        {
                            Marshal.StructureToPtr(facilities[i], elementPtr, false);
                            Marshal.Copy(elementPtr, buffer, headerBytes.Length + (i * elementSize), elementSize);
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(elementPtr);
                    }
                }

                return FacilityMessageHandle.Factory.FromBuffer(buffer);
            }

            /// <summary>
            /// Creates a simulated facility data message for the specified payload.
            /// </summary>
            /// <param name="requestId">The request identifier.</param>
            /// <param name="type">The facility data type.</param>
            /// <param name="payload">The payload bytes to embed.</param>
            /// <returns>A disposable message handle.</returns>
            public static FacilityMessageHandle CreateFacilityDataMessage(uint requestId, SimConnectFacilityDataType type, byte[] payload)
            {
                var header = new FacilityDataHeader
                {
                    Size = (uint)(FacilityDataHeader.PayloadOffset + payload.Length),
                    Version = 1,
                    Id = (uint)SimConnectRecvId.FacilityData,
                    UserRequestId = requestId,
                    UniqueRequestId = 42,
                    ParentUniqueRequestId = 0,
                    Type = type,
                    IsListItem = 0,
                    ItemIndex = 0,
                    ListSize = 0,
                    Data = 0,
                };

                var headerBytes = StructureToBytes(header);
                var totalSize = Math.Max(headerBytes.Length, FacilityDataHeader.PayloadOffset + payload.Length);
                var buffer = new byte[totalSize];
                Array.Copy(headerBytes, buffer, headerBytes.Length);
                Array.Copy(payload, 0, buffer, FacilityDataHeader.PayloadOffset, payload.Length);

                return FacilityMessageHandle.Factory.FromBuffer(buffer, FacilityDataHeader.PayloadOffset + payload.Length);
            }

            /// <summary>
            /// Converts a structure into a byte array for native marshalling tests.
            /// </summary>
            /// <typeparam name="T">The structure type.</typeparam>
            /// <param name="value">The value to copy.</param>
            /// <returns>A byte array containing the structure.</returns>
            private static byte[] StructureToBytes<T>(T value)
                where T : struct
            {
                var size = Marshal.SizeOf<T>();
                var ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(value, ptr, false);
                    var buffer = new byte[size];
                    Marshal.Copy(ptr, buffer, 0, size);
                    return buffer;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct MinimalListHeader
            {
                public static readonly int SizeInBytes = Marshal.SizeOf<MinimalListHeader>();

                public uint Size;
                public uint Version;
                public uint Id;
                public uint RequestId;
                public uint ArraySize;
                public uint EntryNumber;
                public uint OutOf;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private struct FacilityDataHeader
            {
                public static readonly int PayloadOffset = Marshal.SizeOf<FacilityDataHeader>() - sizeof(uint);

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
        }

        /// <summary>
        /// Disposable wrapper around unmanaged facility message buffers.
        /// </summary>
        private sealed class FacilityMessageHandle : IDisposable
        {
            private FacilityMessageHandle(IntPtr pointer, int length)
            {
                this.Pointer = pointer;
                this.Length = length;
            }

            /// <summary>
            /// Gets the unmanaged pointer to the simulated message.
            /// </summary>
            public IntPtr Pointer { get; }

            /// <summary>
            /// Gets the length of the unmanaged buffer.
            /// </summary>
            public int Length { get; }

            /// <inheritdoc />
            public void Dispose()
            {
                if (this.Pointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.Pointer);
                }
            }

            /// <summary>
            /// Factory helpers for creating <see cref="FacilityMessageHandle"/> instances.
            /// </summary>
            public static class Factory
            {
                /// <summary>
                /// Creates a disposable facility message handle from the provided buffer.
                /// </summary>
                /// <param name="buffer">The buffer to copy into unmanaged memory.</param>
                /// <param name="sizeOverride">Optional size override when the buffer is larger than the payload.</param>
                /// <returns>A disposable facility message handle.</returns>
                public static FacilityMessageHandle FromBuffer(byte[] buffer, int? sizeOverride = null)
                {
                    var size = sizeOverride ?? buffer.Length;
                    var ptr = Marshal.AllocHGlobal(size);
                    Marshal.Copy(buffer, 0, ptr, size);
                    return new FacilityMessageHandle(ptr, size);
                }
            }
        }

        /// <summary>
        /// Fake facility API used to capture facility requests.
        /// </summary>
        private sealed class FakeFacilityApi : ISimConnectFacilityApi
        {
            public List<(uint DefinitionId, string Field)> AddedFields { get; } = new();

            public List<(uint Type, uint RequestId)> MinimalRequests { get; } = new();

            public List<(uint DefinitionId, uint RequestId, string Icao, string Region)> DataRequests { get; } = new();

            public List<(uint Type, uint EnterRequestId, uint ExitRequestId)> Subscriptions { get; } = new();

            public List<(uint Type, bool NewRange, bool OldRange)> Unsubscriptions { get; } = new();

            public int AddToFacilityDefinition(IntPtr handle, uint definitionId, string fieldName)
            {
                this.AddedFields.Add((definitionId, fieldName));
                return 0;
            }

            public int RequestFacilityData(IntPtr handle, uint definitionId, uint requestId, string icao, string region)
            {
                this.DataRequests.Add((definitionId, requestId, icao, region));
                return 0;
            }

            public int RequestFacilitiesList(IntPtr handle, uint type, uint requestId)
            {
                return 0;
            }

            public int RequestFacilitiesListEx1(IntPtr handle, uint type, uint requestId)
            {
                this.MinimalRequests.Add((type, requestId));
                return 0;
            }

            public int SubscribeToFacilitiesEx1(IntPtr handle, uint type, uint newInRangeRequestId, uint oldOutRangeRequestId)
            {
                this.Subscriptions.Add((type, newInRangeRequestId, oldOutRangeRequestId));
                return 0;
            }

            public int UnsubscribeFromFacilitiesEx1(IntPtr handle, uint type, bool unsubscribeNewInRange, bool unsubscribeOldOutRange)
            {
                this.Unsubscriptions.Add((type, unsubscribeNewInRange, unsubscribeOldOutRange));
                return 0;
            }
        }
    }
}
