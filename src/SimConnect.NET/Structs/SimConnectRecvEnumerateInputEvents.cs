// <copyright file="SimConnectRecvEnumerateInputEvents.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEnumerateInputEvents structure is used to return a single page of data about an input event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SimConnectRecvEnumerateInputEvents
    {
        /// <summary>
        /// Gets or sets the total size of the returned structure in bytes.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Gets or sets the version number of the SimConnect server.
        /// </summary>
        public uint Version;

        /// <summary>
        /// Gets or sets the ID of the returned structure.
        /// </summary>
        public uint Id;

        /// <summary>
        /// Gets or sets the client-defined request ID.
        /// </summary>
        public uint RequestId;

        /// <summary>
        /// Gets or sets the number of elements in the list that are within this packet.
        /// </summary>
        public uint ArraySize;

        /// <summary>
        /// Gets or sets the index number of this list packet. This number will be from 0 to OutOf - 1.
        /// </summary>
        public uint EntryNumber;

        /// <summary>
        /// Gets or sets the total number of packets used to transmit the list.
        /// </summary>
        public uint OutOf;
    }
}
