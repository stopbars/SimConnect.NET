// <copyright file="SimSystemEventFilenameReceivedEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for filename-based system events such as flight load/save notifications.
    /// </summary>
    /// <param name="fileName">The filename reported by the simulator.</param>
    /// <param name="flags">Optional flags returned by the simulator.</param>
    public class SimSystemEventFilenameReceivedEventArgs(string fileName, uint flags) : EventArgs
    {
        /// <summary>
        /// Gets the filename reported by the simulator.
        /// </summary>
        public string FileName { get; } = fileName;

        /// <summary>
        /// Gets the flags returned alongside the filename.
        /// </summary>
        public uint Flags { get; } = flags;
    }
}
