// <copyright file="InputEventMapping.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Represents a mapping between an input definition and client events.
    /// </summary>
    public sealed class InputEventMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventMapping"/> class.
        /// </summary>
        /// <param name="inputDefinition">The input definition string.</param>
        /// <param name="downEventId">The event ID triggered on key/button down.</param>
        /// <param name="downValue">The value returned when down event occurs.</param>
        /// <param name="upEventId">The event ID triggered on key/button up (optional).</param>
        /// <param name="upValue">The value returned when up event occurs.</param>
        /// <param name="maskable">Whether the event can be masked from lower priority clients.</param>
        public InputEventMapping(
            string inputDefinition,
            uint downEventId,
            uint downValue = 0,
            uint? upEventId = null,
            uint upValue = 0,
            bool maskable = false)
        {
            this.InputDefinition = inputDefinition ?? throw new ArgumentNullException(nameof(inputDefinition));
            this.DownEventId = downEventId;
            this.DownValue = downValue;
            this.UpEventId = upEventId;
            this.UpValue = upValue;
            this.Maskable = maskable;
        }

        /// <summary>
        /// Gets the input definition string (e.g., "VK_LCONTROL+A", "joystick:0:button:0").
        /// </summary>
        public string InputDefinition { get; }

        /// <summary>
        /// Gets the event ID triggered on key/button down.
        /// </summary>
        public uint DownEventId { get; }

        /// <summary>
        /// Gets the value returned when down event occurs.
        /// </summary>
        public uint DownValue { get; }

        /// <summary>
        /// Gets the event ID triggered on key/button up (if specified).
        /// </summary>
        public uint? UpEventId { get; }

        /// <summary>
        /// Gets the value returned when up event occurs.
        /// </summary>
        public uint UpValue { get; }

        /// <summary>
        /// Gets a value indicating whether the event can be masked from lower priority clients.
        /// </summary>
        public bool Maskable { get; }

        /// <summary>
        /// Returns a string representation of the input event mapping.
        /// </summary>
        /// <returns>A string representation of the mapping.</returns>
        public override string ToString()
        {
            var upInfo = this.UpEventId.HasValue ? $", Up: {this.UpEventId}({this.UpValue})" : string.Empty;
            return $"{this.InputDefinition} -> Down: {this.DownEventId}({this.DownValue}){upInfo}";
        }
    }
}
