// <copyright file="SimConnectRecvWaypointList.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvWaypointList structure is used to return a list of SimConnectDataFacilityWaypoint structures.
    /// </summary>
    public struct SimConnectRecvWaypointList
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
        /// Gets or sets the array of SimConnectDataFacilityWaypoint structures.
        /// </summary>
        public SimConnectDataFacilityWaypoint[] RgData { get; set; }
    }
}
