// <copyright file="SimConnectErrorEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for SimConnect error events.
    /// </summary>
    public class SimConnectErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectErrorEventArgs"/> class.
        /// </summary>
        /// <param name="error">The SimConnect error that occurred.</param>
        /// <param name="exception">The exception that was thrown, if any.</param>
        /// <param name="context">Additional context about when/where the error occurred.</param>
        /// <param name="timestamp">The timestamp when the error occurred.</param>
        public SimConnectErrorEventArgs(SimConnectError error, Exception? exception = null, string? context = null, DateTime? timestamp = null)
        {
            this.Error = error;
            this.Exception = exception;
            this.Context = context ?? string.Empty;
            this.Timestamp = timestamp ?? DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the SimConnect error that occurred.
        /// </summary>
        public SimConnectError Error { get; }

        /// <summary>
        /// Gets the exception that was thrown, if any.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets additional context about when/where the error occurred.
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// Gets the timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets a value indicating whether this error is recoverable.
        /// </summary>
        public bool IsRecoverable => this.Error switch
        {
            SimConnectError.None => true,
            SimConnectError.UnrecognizedId => true,
            SimConnectError.InvalidDataType => true,
            SimConnectError.InvalidDataSize => true,
            SimConnectError.InvalidArray => true,
            SimConnectError.CreateObjectFailed => true,
            SimConnectError.LoadFlightplanFailed => true,
            SimConnectError.AlreadySubscribed => true,
            SimConnectError.DuplicateId => true,
            SimConnectError.OutOfBounds => true,
            SimConnectError.VersionMismatch => false,
            SimConnectError.DataError => false,
            SimConnectError.Error => false,
            _ => false,
        };
    }
}
