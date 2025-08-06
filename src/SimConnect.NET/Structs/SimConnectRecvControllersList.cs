// <copyright file="SimConnectRecvControllersList.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvControllersList structure is used to return a list of SimConnectControllerItem structures.
    /// </summary>
    public struct SimConnectRecvControllersList
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
        /// Gets or sets the array of SimConnectControllerItem structures.
        /// </summary>
        public SimConnectControllerItem[] RgData { get; set; }
    }
}
