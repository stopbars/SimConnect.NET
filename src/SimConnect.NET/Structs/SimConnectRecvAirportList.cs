// <copyright file="SimConnectRecvAirportList.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvAirportList structure is used to return a list of SimConnectDataFacilityAirport structures.
    /// </summary>
    public struct SimConnectRecvAirportList
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
        /// Gets or sets the array of SimConnectDataFacilityAirport structures.
        /// </summary>
        public SimConnectDataFacilityAirport[] RgData { get; set; }
    }
}
