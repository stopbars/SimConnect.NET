// <copyright file="SimConnectRecvEventObjectAddRemove.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEventObjectAddRemove structure is used to return the type and ID of an AI object that has been added or removed from the simulation, by any client.
    /// </summary>
    public struct SimConnectRecvEventObjectAddRemove
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
        /// Gets or sets the type of object that was added or removed.
        /// </summary>
        public SimConnectSimObjectType EObjType { get; set; }
    }
}
