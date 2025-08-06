// <copyright file="InputEventChangedEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Event arguments for input event value changes.
    /// </summary>
    public class InputEventChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventChangedEventArgs"/> class.
        /// </summary>
        /// <param name="inputEventValue">The input event value that changed.</param>
        public InputEventChangedEventArgs(InputEventValue inputEventValue)
        {
            this.InputEventValue = inputEventValue ?? throw new ArgumentNullException(nameof(inputEventValue));
            this.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the input event value that changed.
        /// </summary>
        public InputEventValue InputEventValue { get; }

        /// <summary>
        /// Gets the timestamp when the event occurred.
        /// </summary>
        public DateTime Timestamp { get; }
    }
}
