// <copyright file="AircraftEngine.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET.Aircraft
{
    /// <summary>
    /// Represents engine information for an aircraft.
    /// </summary>
    public sealed class AircraftEngine
    {
        /// <summary>
        /// Gets or sets the engine number (1-based).
        /// </summary>
        public int EngineNumber { get; set; }

        /// <summary>
        /// Gets or sets the throttle position as a percentage (0-100).
        /// </summary>
        public double ThrottlePosition { get; set; }

        /// <summary>
        /// Gets or sets the engine RPM.
        /// </summary>
        public double Rpm { get; set; }

        /// <summary>
        /// Gets or sets the N1 percentage for turbine engines.
        /// </summary>
        public double N1 { get; set; }

        /// <summary>
        /// Gets or sets the N2 percentage for turbine engines.
        /// </summary>
        public double N2 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the engine is running.
        /// </summary>
        public bool IsRunning { get; set; }
    }
}
