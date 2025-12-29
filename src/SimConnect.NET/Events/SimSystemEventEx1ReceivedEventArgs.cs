// <copyright file="SimSystemEventEx1ReceivedEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for extended EX1 system events that carry multiple data parameters.
    /// </summary>
    /// <param name="eventId">The identifier of the event.</param>
    /// <param name="data0">First data parameter.</param>
    /// <param name="data1">Second data parameter.</param>
    /// <param name="data2">Third data parameter.</param>
    /// <param name="data3">Fourth data parameter.</param>
    /// <param name="data4">Fifth data parameter.</param>
    public class SimSystemEventEx1ReceivedEventArgs(uint eventId, uint data0, uint data1, uint data2, uint data3, uint data4) : EventArgs
    {
        /// <summary>
        /// Gets the event identifier.
        /// </summary>
        public uint EventId { get; } = eventId;

        /// <summary>
        /// Gets the first data parameter.
        /// </summary>
        public uint Data0 { get; } = data0;

        /// <summary>
        /// Gets the second data parameter.
        /// </summary>
        public uint Data1 { get; } = data1;

        /// <summary>
        /// Gets the third data parameter.
        /// </summary>
        public uint Data2 { get; } = data2;

        /// <summary>
        /// Gets the fourth data parameter.
        /// </summary>
        public uint Data3 { get; } = data3;

        /// <summary>
        /// Gets the fifth data parameter.
        /// </summary>
        public uint Data4 { get; } = data4;
    }
}
