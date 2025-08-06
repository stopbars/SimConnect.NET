// <copyright file="SimConnectDataMarkerState.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the marker state data used to graphically link flight model data with the graphics model.
    /// </summary>
    public struct SimConnectDataMarkerState
    {
        /// <summary>
        /// Gets or sets the marker name. One from the following list:
        /// Cg, ModelCenter, Wheel, Skid, Ski, Float, Scrape, Engine, Prop, Eyepoint, LongScale, LatScale, VertScale,
        /// AeroCenter, WingApex, RefChord, Datum, WingTip, FuelTank, Forces.
        /// </summary>
        public string MarkerName { get; set; }

        /// <summary>
        /// Gets or sets the marker state. Set to 1 for on and 0 for off.
        /// </summary>
        public uint MarkerState { get; set; }
    }
}
