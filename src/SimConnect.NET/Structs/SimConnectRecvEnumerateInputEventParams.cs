// <copyright file="SimConnectRecvEnumerateInputEventParams.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEnumerateInputEventParams structure is a response with the available parameters for an input event.
    /// </summary>
    public struct SimConnectRecvEnumerateInputEventParams
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
        /// Gets or sets the hash ID that identifies the input event.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// Gets or sets the string that contains the values, separated by ';'.
        /// Values can be:
        /// <list type="bullet">
        /// <item><description>char[260]</description></item>
        /// <item><description>FLOAT64</description></item>
        /// </list>
        /// </summary>
        public string Value { get; set; }
    }
}
