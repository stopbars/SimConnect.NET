// <copyright file="SimConnectRecvOpen.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the data returned by SimConnect immediately after a successful call to SimConnect_Open (SIMCONNECT_RECV_OPEN).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SimConnectRecvOpen
    {
        /// <summary>
        /// Backing fixed-size ANSI byte buffer for the application name (256 bytes defined by the native SDK).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        private byte[] applicationNameBytes;

        /// <summary>
        /// Gets or sets the total size of the returned structure in bytes.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets or sets the version number of the SimConnect server (internal use).
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the ID of the returned structure (should be <see cref="SimConnectRecvId.Open"/>).
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Gets the null-terminated simulator application name string.
        /// </summary>
        public readonly string? ApplicationName
        {
            get
            {
                if (this.applicationNameBytes == null)
                {
                    return null;
                }

                var nullIndex = Array.IndexOf(this.applicationNameBytes, (byte)0);
                var length = nullIndex >= 0 ? nullIndex : this.applicationNameBytes.Length;
                return System.Text.Encoding.ASCII.GetString(this.applicationNameBytes, 0, length);
            }
        }

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
