// <copyright file="SimConnectRecvListTemplate.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvListTemplate structure is used to provide information on the number of elements in a list returned to the client, and the number of packets that were used to transmit the data.
    /// </summary>
    public struct SimConnectRecvListTemplate
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
    }
}
