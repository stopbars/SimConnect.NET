// <copyright file="SimConnectFacilityMinimal.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents minimal information about a facility.
    /// </summary>
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
