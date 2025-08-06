// <copyright file="SimConnectRecvSystemState.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the system state received from SimConnect.
    /// Used with the SimConnect_RequestSystemState function to retrieve specific Microsoft Flight Simulator system states and information.
    /// </summary>
    public struct SimConnectRecvSystemState
    {
        /// <summary>
        /// Gets or sets the total size of the returned structure in bytes.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets or sets the version number of the SimConnect server.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the ID of the returned structure.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets the client-defined request ID.
        /// </summary>
        public uint RequestId { get; set; }

        /// <summary>
        /// Gets or sets an integer or boolean value.
        /// </summary>
        public uint IntegerValue { get; set; }

        /// <summary>
        /// Gets or sets a float value.
        /// </summary>
        public float FloatValue { get; set; }

        /// <summary>
        /// Gets or sets a null-terminated string.
        /// </summary>
        public string StringValue { get; set; }
    }
}
