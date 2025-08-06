// <copyright file="SimConnectRecvNdbList.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvNdbList structure is used to return a list of SimConnectDataFacilityNdb structures.
    /// </summary>
    public struct SimConnectRecvNdbList
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
        /// Gets or sets the array of SimConnectDataFacilityNdb structures.
        /// </summary>
        public SimConnectDataFacilityNdb[] RgData { get; set; }
    }
}
