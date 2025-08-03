// <copyright file="SimConnectInputEventDescriptor.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents an item of data for a specific input event.
    /// </summary>
    public struct SimConnectInputEventDescriptor
    {
        /// <summary>
        /// Gets or sets the name of the input event.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the hash ID for the event.
        /// </summary>
        public uint Hash { get; set; }

        /// <summary>
        /// Gets or sets the expected data type (from the SimConnectDataType enum).
        /// Usually a Float32 or String128.
        /// </summary>
        public SimConnectDataType Type { get; set; }

        /// <summary>
        /// Gets or sets a list of the names of the nodes linked to this input event.
        /// Each node name is separated by a semicolon (;).
        /// </summary>
        public string NodeNames { get; set; }
    }
}
