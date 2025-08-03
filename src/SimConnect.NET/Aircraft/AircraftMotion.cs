// <copyright file="AircraftMotion.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET.Aircraft
{
    /// <summary>
    /// Represents the motion characteristics of an aircraft.
    /// </summary>
    public sealed class AircraftMotion
    {
        /// <summary>
        /// Gets or sets the indicated airspeed in knots.
        /// </summary>
        public double IndicatedAirspeed { get; set; }

        /// <summary>
        /// Gets or sets the true airspeed in knots.
        /// </summary>
        public double TrueAirspeed { get; set; }

        /// <summary>
        /// Gets or sets the ground speed in knots.
        /// </summary>
        public double GroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets the vertical speed in feet per minute.
        /// </summary>
        public double VerticalSpeed { get; set; }

        /// <summary>
        /// Gets or sets the GPS ground speed in meters per second.
        /// </summary>
        public double GpsGroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets the GPS track in degrees.
        /// </summary>
        public double GpsTrack { get; set; }
    }
}
