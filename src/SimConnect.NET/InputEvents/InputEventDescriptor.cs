// <copyright file="InputEventDescriptor.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Represents a descriptor for an input event with its metadata.
    /// </summary>
    public sealed class InputEventDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventDescriptor"/> class.
        /// </summary>
        /// <param name="name">The name of the input event.</param>
        /// <param name="hash">The hash ID of the input event.</param>
        /// <param name="type">The data type of the input event.</param>
        /// <param name="nodeNames">The node names associated with the input event.</param>
        public InputEventDescriptor(string name, ulong hash, SimConnectDataType type, string nodeNames)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Hash = hash;
            this.Type = type;
            this.NodeNames = nodeNames ?? string.Empty;
        }

        /// <summary>
        /// Gets the name of the input event.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the hash ID for the event.
        /// </summary>
        public ulong Hash { get; }

        /// <summary>
        /// Gets the expected data type (from the SimConnectDataType enum).
        /// Usually a Float32 or String128.
        /// </summary>
        public SimConnectDataType Type { get; }

        /// <summary>
        /// Gets a list of the names of the nodes linked to this input event.
        /// Each node name is separated by a semicolon (;).
        /// </summary>
        public string NodeNames { get; }

        /// <summary>
        /// Returns a string representation of the input event descriptor.
        /// </summary>
        /// <returns>A string representation of the descriptor.</returns>
        public override string ToString()
        {
            return $"{this.Name} (Hash: {this.Hash}, Type: {this.Type})";
        }
    }
}
