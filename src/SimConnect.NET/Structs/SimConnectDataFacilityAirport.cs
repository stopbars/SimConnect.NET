// <copyright file="SimConnectDataFacilityAirport.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents information about a single airport in the facilities cache.
    /// </summary>
    public struct SimConnectDataFacilityAirport
    {
        /// <summary>
        /// Gets or sets the airport ICAO code.
        /// </summary>
        public string Ident { get; set; }

        /// <summary>
        /// Gets or sets the airport region code.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the airport facility.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the airport facility.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude of the facility in meters.
        /// </summary>
        public double Altitude { get; set; }
    }
}
