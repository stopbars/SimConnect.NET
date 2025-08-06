// <copyright file="SimConnectFacilityMinimal.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents minimal information about a facility.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SimConnectFacilityMinimal
    {
        /// <summary>
        /// Gets or sets the ICAO code of the facility.
        /// </summary>
        public SimConnectIcao Icao { get; set; }

        /// <summary>
        /// Gets or sets the latitude, longitude, and altitude of the facility.
        /// </summary>
        public SimConnectDataLatLonAlt LatLonAlt { get; set; }
    }
}
