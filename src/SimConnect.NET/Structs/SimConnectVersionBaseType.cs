// <copyright file="SimConnectVersionBaseType.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents version data for the hardware.
    /// </summary>
    public struct SimConnectVersionBaseType
    {
        /// <summary>
        /// Gets or sets the major version number.
        /// </summary>
        public ushort Major { get; set; }

        /// <summary>
        /// Gets or sets the minor version number.
        /// </summary>
        public ushort Minor { get; set; }

        /// <summary>
        /// Gets or sets the revision number.
        /// </summary>
        public ushort Revision { get; set; }

        /// <summary>
        /// Gets or sets the build ID.
        /// </summary>
        public ushort Build { get; set; }
    }
}
