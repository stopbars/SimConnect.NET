// <copyright file="SimConnectRecvAssignedObjectId.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents a structure used to return an object ID that matches a request ID.
    /// </summary>
    public struct SimConnectRecvAssignedObjectId
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
        /// Gets or sets the server-defined object ID.
        /// </summary>
        public uint ObjectId { get; set; }
    }
}
