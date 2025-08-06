// <copyright file="SimConnectRecv.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecv structure is used with the SimConnectRecvId enumeration to indicate which type of structure has been returned.
    /// </summary>
    public struct SimConnectRecv
    {
        /// <summary>
        /// Gets or sets the total size of the returned structure in bytes (that is, not usually the size of the SimConnectRecv structure, but of the structure that inherits it).
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets or sets the version number of the SimConnect server.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the ID of the returned structure. One member of SimConnectRecvId.
        /// </summary>
        public uint Id { get; set; }
    }
}
