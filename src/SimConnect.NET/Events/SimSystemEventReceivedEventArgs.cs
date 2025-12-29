// <copyright file="SimSystemEventReceivedEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for an event that is raised when a Simconnect system event is raised.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SimSystemEventReceivedEventArgs"/> class with the specified event identifier and.
    /// associated data.
    /// </remarks>
    /// <param name="eventId">The unique identifier for the system event.</param>
    /// <param name="data">The data associated with the system event.</param>
    public class SimSystemEventReceivedEventArgs(uint eventId, uint data) : EventArgs
    {
        /// <summary>
        /// Gets the unique identifier for the event.
        /// </summary>
        public uint EventId { get; } = eventId;

        /// <summary>
        /// Gets the data associated with the event.
        /// </summary>
        public uint Data { get; } = data;
    }
}
