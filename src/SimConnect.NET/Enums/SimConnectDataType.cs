// <copyright file="SimConnectDataType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SIMCONNECT_DATATYPE enumeration type is used with the SimConnect_AddToDataDefinition call
    /// to specify the data type that the server should use to return the specified data to the client.
    /// </summary>
    public enum SimConnectDataType
    {
        /// <summary>
        /// Invalid data type.
        /// </summary>
        Invalid,

        /// <summary>
        /// Specifies a 32-bit signed integer value.
        /// </summary>
        Integer32,

        /// <summary>
        /// Specifies a 64-bit signed integer value.
        /// </summary>
        Integer64,

        /// <summary>
        /// Specifies a 32-bit floating point number.
        /// </summary>
        FloatSingle,

        /// <summary>
        /// Specifies a 64-bit floating point number.
        /// </summary>
        FloatDouble,

        /// <summary>
        /// Specifies a string of 8 characters.
        /// </summary>
        String8,

        /// <summary>
        /// Specifies a string of 32 characters.
        /// </summary>
        String32,

        /// <summary>
        /// Specifies a string of 64 characters.
        /// </summary>
        String64,

        /// <summary>
        /// Specifies a string of 128 characters.
        /// </summary>
        String128,

        /// <summary>
        /// Specifies a string of 256 characters.
        /// </summary>
        String256,

        /// <summary>
        /// Specifies a string of 260 characters.
        /// </summary>
        String260,

        /// <summary>
        /// Specifies a variable length string.
        /// </summary>
        StringV,

        /// <summary>
        /// Specifies the SIMCONNECT_DATA_INITPOSITION structure.
        /// </summary>
        InitPosition,

        /// <summary>
        /// Specifies the SIMCONNECT_DATA_MARKERSTATE structure.
        /// </summary>
        MarkerState,

        /// <summary>
        /// Specifies the SIMCONNECT_DATA_WAYPOINT structure.
        /// </summary>
        Waypoint,

        /// <summary>
        /// Specifies the SIMCONNECT_DATA_LATLONALT structure.
        /// </summary>
        LatLonAlt,

        /// <summary>
        /// Specifies the SIMCONNECT_DATA_XYZ structure.
        /// </summary>
        Xyz,
    }
}
