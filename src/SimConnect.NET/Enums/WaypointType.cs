// <copyright file="WaypointType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of waypoint.
    /// </summary>
    public enum WaypointType
    {
        /// <summary>No specific waypoint type.</summary>
        None = 0,

        /// <summary>Named waypoint.</summary>
        Named = 1,

        /// <summary>Unnamed waypoint.</summary>
        Unnamed = 2,

        /// <summary>VOR waypoint.</summary>
        Vor = 3,

        /// <summary>NDB waypoint.</summary>
        Ndb = 4,

        /// <summary>Off-route waypoint.</summary>
        OffRoute = 5,

        /// <summary>Initial Approach Fix waypoint.</summary>
        Iaf = 6,

        /// <summary>Final Approach Fix waypoint.</summary>
        Faf = 7,

        /// <summary>RNAV waypoint.</summary>
        Rnav = 8,

        /// <summary>VFR waypoint.</summary>
        Vfr = 9,
    }
}
