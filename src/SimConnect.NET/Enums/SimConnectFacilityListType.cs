// <copyright file="SimConnectFacilityListType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectFacilityListType enumeration type is used to determine which type of facilities data is being requested or returned.
    /// </summary>
    public enum SimConnectFacilityListType
    {
        /// <summary>
        /// Specifies that the type of information is for an airport, see SIMCONNECT_DATA_FACILITY_AIRPORT.
        /// </summary>
        Airport,

        /// <summary>
        /// Specifies that the type of information is for a waypoint, see SIMCONNECT_DATA_FACILITY_WAYPOINT.
        /// </summary>
        Waypoint,

        /// <summary>
        /// Specifies that the type of information is for an NDB, see SIMCONNECT_DATA_FACILITY_NDB.
        /// </summary>
        Ndb,

        /// <summary>
        /// Specifies that the type of information is for a VOR, see SIMCONNECT_DATA_FACILITY_VOR.
        /// </summary>
        Vor,

        /// <summary>
        /// Not valid as a list type, but simply the number of list types.
        /// </summary>
        Count,
    }
}
