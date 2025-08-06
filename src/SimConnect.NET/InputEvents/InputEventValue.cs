// <copyright file="InputEventValue.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Globalization;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Represents the value of an input event.
    /// </summary>
    public sealed class InputEventValue
    {
        /// <summary>
        /// Gets or sets the hash ID of the input event.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// Gets or sets the type of the input event value.
        /// </summary>
        public SimConnectInputEventType Type { get; set; }

        /// <summary>
        /// Gets or sets the raw value of the input event.
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Gets the value as a double, if the type is DoubleValue.
        /// </summary>
        /// <returns>The double value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the type is not DoubleValue.</exception>
        public double GetDoubleValue()
        {
            if (this.Type != SimConnectInputEventType.DoubleValue)
            {
                throw new InvalidOperationException($"Input event type is {this.Type}, not DoubleValue");
            }

            return this.Value switch
            {
                double d => d,
                float f => f,
                int i => i,
                uint ui => ui,
                long l => l,
                ulong ul => ul,
                _ => Convert.ToDouble(this.Value, CultureInfo.InvariantCulture),
            };
        }

        /// <summary>
        /// Gets the value as a string, if the type is StringValue.
        /// </summary>
        /// <returns>The string value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the type is not StringValue.</exception>
        public string GetStringValue()
        {
            if (this.Type != SimConnectInputEventType.StringValue)
            {
                throw new InvalidOperationException($"Input event type is {this.Type}, not StringValue");
            }

            return this.Value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Tries to get the value as a double.
        /// </summary>
        /// <param name="value">The double value if successful.</param>
        /// <returns>True if the conversion was successful; otherwise, false.</returns>
        public bool TryGetDoubleValue(out double value)
        {
            value = 0;
            if (this.Type != SimConnectInputEventType.DoubleValue)
            {
                return false;
            }

            try
            {
                value = this.GetDoubleValue();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get the value as a string.
        /// </summary>
        /// <param name="value">The string value if successful.</param>
        /// <returns>True if the conversion was successful; otherwise, false.</returns>
        public bool TryGetStringValue(out string value)
        {
            value = string.Empty;
            if (this.Type != SimConnectInputEventType.StringValue)
            {
                return false;
            }

            try
            {
                value = this.GetStringValue();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a string representation of the input event value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return $"Hash: {this.Hash}, Type: {this.Type}, Value: {this.Value}";
        }
    }
}
