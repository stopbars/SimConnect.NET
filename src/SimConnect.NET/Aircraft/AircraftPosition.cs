// <copyright file="AircraftPosition.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET.Aircraft
{
    /// <summary>
    /// Represents the complete position and orientation of an aircraft.
    /// </summary>
    public sealed class AircraftPosition
    {
        /// <summary>
        /// Gets or sets the aircraft latitude in degrees.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft longitude in degrees.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft altitude above sea level in feet.
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft altitude above ground in feet.
        /// </summary>
        public double AltitudeAboveGround { get; set; }

        /// <summary>
        /// Gets or sets the aircraft true heading in degrees.
        /// </summary>
        public double TrueHeading { get; set; }

        /// <summary>
        /// Gets or sets the aircraft magnetic heading in degrees.
        /// </summary>
        public double MagneticHeading { get; set; }

        /// <summary>
        /// Gets or sets the aircraft pitch in degrees.
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Gets or sets the aircraft bank angle in degrees.
        /// </summary>
        public double Bank { get; set; }
    }
}
