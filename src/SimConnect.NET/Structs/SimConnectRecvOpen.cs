// <copyright file="SimConnectRecvOpen.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvOpen structure is used to return information to the client after a successful call to SimConnect_Open.
    /// </summary>
    public struct SimConnectRecvOpen
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
        /// Gets or sets the null-terminated string containing the application name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the application version major number.
        /// </summary>
        public uint ApplicationVersionMajor { get; set; }

        /// <summary>
        /// Gets or sets the application version minor number.
        /// </summary>
        public uint ApplicationVersionMinor { get; set; }

        /// <summary>
        /// Gets or sets the application build major number.
        /// </summary>
        public uint ApplicationBuildMajor { get; set; }

        /// <summary>
        /// Gets or sets the application build minor number.
        /// </summary>
        public uint ApplicationBuildMinor { get; set; }

        /// <summary>
        /// Gets or sets the SimConnect version major number.
        /// </summary>
        public uint SimConnectVersionMajor { get; set; }

        /// <summary>
        /// Gets or sets the SimConnect version minor number.
        /// </summary>
        public uint SimConnectVersionMinor { get; set; }

        /// <summary>
        /// Gets or sets the SimConnect build major number.
        /// </summary>
        public uint SimConnectBuildMajor { get; set; }

        /// <summary>
        /// Gets or sets the SimConnect build minor number.
        /// </summary>
        public uint SimConnectBuildMinor { get; set; }

        /// <summary>
        /// Gets or sets the reserved field 1.
        /// </summary>
        public uint Reserved1 { get; set; }

        /// <summary>
        /// Gets or sets the reserved field 2.
        /// </summary>
        public uint Reserved2 { get; set; }
    }
}
