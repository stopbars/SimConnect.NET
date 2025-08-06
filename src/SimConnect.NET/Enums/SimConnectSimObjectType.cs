// <copyright file="SimConnectSimObjectType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectSimObjectType enumeration type is used with the SimConnect_RequestDataOnSimObjectType call
    /// to request information on specific or nearby objects.
    /// </summary>
    public enum SimConnectSimObjectType
    {
        /// <summary>
        /// Specifies the user's aircraft.
        /// </summary>
        User,

        /// <summary>
        /// Specifies all AI controlled objects.
        /// </summary>
        All,

        /// <summary>
        /// Specifies all aircraft.
        /// </summary>
        Aircraft,

        /// <summary>
        /// Specifies all helicopters.
        /// </summary>
        Helicopter,

        /// <summary>
        /// Specifies all AI controlled boats.
        /// </summary>
        Boat,

        /// <summary>
        /// Specifies all AI controlled ground vehicles.
        /// </summary>
        Ground,
    }
}
