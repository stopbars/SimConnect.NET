// <copyright file="SimConnectRecvFacilityMinimalList.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvFacilityMinimalList structure is used to provide minimal information on the number of elements in a list of facilities returned to the client, and the number of packets that were used to transmit the data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SimConnectRecvFacilityMinimalList
    {
        /// <summary>
        /// Gets or sets the total size of the returned structure in bytes.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets or sets the version number of the SimConnect server.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the ID of the returned structure.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets the client-defined request ID.
        /// </summary>
        public uint RequestId { get; set; }

        /// <summary>
        /// Gets or sets the number of elements in the list that are within this packet.
        /// </summary>
        public uint ArraySize { get; set; }

        /// <summary>
        /// Gets or sets the index number of this list packet. This number will be from 0 to OutOf - 1.
        /// </summary>
        public uint EntryNumber { get; set; }

        /// <summary>
        /// Gets or sets the total number of packets used to transmit the list.
        /// </summary>
        public uint OutOf { get; set; }

        // Note: The variable-length array rgData[dwArraySize] is handled manually in the client code
        // due to P/Invoke limitations with variable-length arrays
    }
}
