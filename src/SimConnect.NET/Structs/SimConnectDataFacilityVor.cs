// <copyright file="SimConnectDataFacilityVor.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents information on a single VOR station in the facilities cache.
    /// </summary>
    public struct SimConnectDataFacilityVor
    {
        /// <summary>
        /// Gets or sets flags indicating whether the other fields are valid or not.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the ILS localizer angle in degrees.
        /// </summary>
        public float Localizer { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the glide slope transmitter in degrees.
        /// </summary>
        public double GlideLat { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the glide slope transmitter in degrees.
        /// </summary>
        public double GlideLon { get; set; }

        /// <summary>
        /// Gets or sets the altitude of the glide slope transmitter in degrees.
        /// </summary>
        public double GlideAlt { get; set; }

        /// <summary>
        /// Gets or sets the ILS approach angle in degrees.
        /// </summary>
        public float GlideSlopeAngle { get; set; }
    }
}
