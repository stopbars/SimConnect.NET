// <copyright file="SimSystemEventFrameEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for a frame-based system event that includes frame rate and simulation speed.
    /// </summary>
    /// <param name="frameRate">The reported frame rate in frames per second.</param>
    /// <param name="simulationSpeed">The reported simulation speed multiplier.</param>
    public class SimSystemEventFrameEventArgs(float frameRate, float simulationSpeed) : EventArgs
    {
        /// <summary>
        /// Gets the reported frame rate in frames per second.
        /// </summary>
        public float FrameRate { get; } = frameRate;

        /// <summary>
        /// Gets the reported simulation speed multiplier.
        /// </summary>
        public float SimulationSpeed { get; } = simulationSpeed;
    }
}
