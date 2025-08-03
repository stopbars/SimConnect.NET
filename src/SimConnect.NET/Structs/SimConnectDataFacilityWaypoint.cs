// <copyright file="SimConnectDataFacilityWaypoint.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents information on a single waypoint in the facilities cache.
    /// </summary>
    public struct SimConnectDataFacilityWaypoint
    {
        /// <summary>
        /// Gets or sets the magnetic variation of the waypoint in degrees.
        /// </summary>
        public float MagneticVariation { get; set; }
    }
}
