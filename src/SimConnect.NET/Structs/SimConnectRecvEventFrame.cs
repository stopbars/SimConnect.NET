// <copyright file="SimConnectRecvEventFrame.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEventFrame structure is used with the SimConnect_SubscribeToSystemEvent call to return the frame rate and simulation speed to the client.
    /// </summary>
    public struct SimConnectRecvEventFrame
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
        /// Gets or sets the visual frame rate in frames per second.
        /// </summary>
        public float FrameRate { get; set; }

        /// <summary>
        /// Gets or sets the simulation rate. For example, if the simulation is running at four times normal speed (4X), then 4.0 will be returned.
        /// </summary>
        public float SimSpeed { get; set; }
    }
}
