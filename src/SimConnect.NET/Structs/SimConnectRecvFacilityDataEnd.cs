// <copyright file="SimConnectRecvFacilityDataEnd.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the end of a data stream from the server after a call to SimConnect_RequestFacilityData.
    /// </summary>
    public struct SimConnectRecvFacilityDataEnd
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
    }
}
