// <copyright file="SimConnectDataLatLonAlt.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectDataLatLonAlt struct is used to hold a world position.
    /// </summary>
    public struct SimConnectDataLatLonAlt
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
    }
}
