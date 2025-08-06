// <copyright file="SimConnectRecvEventFilename.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEventFilename structure is used to return a filename and an event ID to the client.
    /// </summary>
    public struct SimConnectRecvEventFilename
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
        /// Gets or sets the returned filename.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets reserved flags, should be 0.
        /// </summary>
        public uint Flags { get; set; }
    }
}
