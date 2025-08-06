// <copyright file="ConnectionStatusChangedEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for the ConnectionStatusChanged event.
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStatusChangedEventArgs"/> class.
        /// </summary>
        /// <param name="previousStatus">The previous connection status.</param>
        /// <param name="currentStatus">The current connection status.</param>
        /// <param name="timestamp">The timestamp when the status change occurred.</param>
        public ConnectionStatusChangedEventArgs(bool previousStatus, bool currentStatus, DateTime timestamp)
        {
            this.PreviousStatus = previousStatus;
            this.CurrentStatus = currentStatus;
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// Gets a value indicating whether the previous connection status was connected.
        /// </summary>
        public bool PreviousStatus { get; }

        /// <summary>
        /// Gets a value indicating whether the current connection status is connected.
        /// </summary>
        public bool CurrentStatus { get; }

        /// <summary>
        /// Gets the timestamp when the status change occurred.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets a value indicating whether the connection was established.
        /// </summary>
        public bool IsConnected => this.CurrentStatus;

        /// <summary>
        /// Gets a value indicating whether the connection was lost.
        /// </summary>
        public bool IsDisconnected => !this.CurrentStatus;
    }
}
