// <copyright file="SimConnectDataWaypoint.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectDataWaypoint struct is used to define a single waypoint.
    /// </summary>
    public struct SimConnectDataWaypoint
    {
        /// <summary>
        /// Gets or sets the latitude in degrees.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude in degrees.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet.
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Gets or sets the flags set for this waypoint. These flags can be OR'ed together.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the required speed in knots. If a specific speed is required,
        /// then the SIMCONNECT_WAYPOINT_SPEED_REQUESTED flag must be set to True.
        /// </summary>
        public double KtsSpeed { get; set; }

        /// <summary>
        /// Gets or sets the required throttle as a percentage. If a specific throttle percentage is required,
        /// then the SIMCONNECT_WAYPOINT_THROTTLE_REQUESTED flag must be set to True.
        /// </summary>
        public double PercentThrottle { get; set; }
    }
}
