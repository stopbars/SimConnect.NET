// <copyright file="SimConnectRecvFacilityMinimalList.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvFacilityMinimalList structure is used to provide minimal information on the number of elements in a list of facilities returned to the client, and the number of packets that were used to transmit the data.
    /// </summary>
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
        /// Gets or sets the array of SIMCONNECT_FACILITY_MINIMAL structures.
        /// </summary>
        public SimConnectFacilityMinimal[] RgData { get; set; }
    }
}
