// <copyright file="FacilityDataResult.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace SimConnect.NET.Facilities
{
    /// <summary>
    /// Represents a single facility data packet returned by SimConnect.
    /// </summary>
    public readonly struct FacilityDataResult
    {
        private readonly byte[] payload;

        internal FacilityDataResult(
            uint uniqueRequestId,
            uint parentUniqueRequestId,
            SimConnectFacilityDataType dataType,
            bool isListItem,
            uint itemIndex,
            uint listSize,
            byte[] payload)
        {
            this.UniqueRequestId = uniqueRequestId;
            this.ParentUniqueRequestId = parentUniqueRequestId;
            this.DataType = dataType;
            this.IsListItem = isListItem;
            this.ItemIndex = itemIndex;
            this.ListSize = listSize;
            this.payload = payload ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Gets the unique request identifier assigned by SimConnect.
        /// </summary>
        public uint UniqueRequestId { get; }

        /// <summary>
        /// Gets the parent unique request identifier, if any.
        /// </summary>
        public uint ParentUniqueRequestId { get; }

        /// <summary>
        /// Gets the facility data type represented by this payload.
        /// </summary>
        public SimConnectFacilityDataType DataType { get; }

        /// <summary>
        /// Gets a value indicating whether this payload represents an element of a list.
        /// </summary>
        public bool IsListItem { get; }

        /// <summary>
        /// Gets the index of the item within a list, if <see cref="IsListItem"/> is true.
        /// </summary>
        public uint ItemIndex { get; }

        /// <summary>
        /// Gets the total number of items in the list, if <see cref="IsListItem"/> is true.
        /// </summary>
        public uint ListSize { get; }

        /// <summary>
        /// Gets the raw payload memory returned by SimConnect.
        /// </summary>
        public ReadOnlyMemory<byte> Payload => this.payload;

        /// <summary>
        /// Marshals the payload into a managed struct of the specified type.
        /// </summary>
        /// <typeparam name="T">The target struct type. Must be blittable.</typeparam>
        /// <returns>The marshalled struct.</returns>
        public T As<T>()
            where T : struct
        {
            if (this.payload.Length < Marshal.SizeOf<T>())
            {
                throw new InvalidOperationException("Payload is smaller than the requested struct type.");
            }

            return MemoryMarshal.Read<T>(this.payload.AsSpan());
        }
    }
}
