// <copyright file="SimConnectDataInitPosition.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectDataInitPosition struct is used to initialize the position of the user aircraft, AI-controlled aircraft, or other simulation object.
    /// </summary>
    public struct SimConnectDataInitPosition
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

        /// <summary>
        /// Gets or sets the pitch in degrees.
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Gets or sets the bank in degrees.
        /// </summary>
        public double Bank { get; set; }

        /// <summary>
        /// Gets or sets the heading in degrees.
        /// </summary>
        public double Heading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the object is on the ground (1) or airborne (0).
        /// </summary>
        public uint OnGround { get; set; }

        /// <summary>
        /// Gets or sets the airspeed in knots, or one of the following special values:
        /// - INITPOSITION_AIRSPEED_CRUISE (-1): The aircraft's design cruising speed.
        /// - INITPOSITION_AIRSPEED_KEEP (-2): Maintain the current airspeed.
        /// </summary>
        public uint Airspeed { get; set; }
    }
}
