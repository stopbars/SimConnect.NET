// <copyright file="SimConnectWaypoint.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectWaypoint enumeration type is used with the SIMCONNECT_DATA_WAYPOINT structure to define waypoints.
    /// </summary>
    public enum SimConnectWaypoint
    {
        /// <summary>
        /// Specifies requested speed is valid.
        /// </summary>
        SpeedRequested = 0x04,

        /// <summary>
        /// Specifies requested throttle percentage is valid.
        /// </summary>
        ThrottleRequested = 0x08,

        /// <summary>
        /// Specifies that the vertical speed should be calculated to reach the required speed when crossing the waypoint.
        /// </summary>
        ComputeVerticalSpeed = 0x10,

        /// <summary>
        /// Specifies the altitude specified is AGL.
        /// </summary>
        AltitudeIsAgl = 0x20,

        /// <summary>
        /// Specifies the waypoint should be on the ground. Make sure this flag is set if the aircraft is to taxi to this point.
        /// </summary>
        OnGround = 0x00100000,

        /// <summary>
        /// Specifies that the aircraft should back up to this waypoint. This is only valid on the first waypoint.
        /// </summary>
        Reverse = 0x00200000,

        /// <summary>
        /// Specifies that the next waypoint is the first waypoint. This is only valid on the last waypoint.
        /// </summary>
        WrapToFirst = 0x00400000,
    }
}
