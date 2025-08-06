// <copyright file="SimConnectInputEventDescriptor.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents an item of data for a specific input event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SimConnectInputEventDescriptor
    {
        /// <summary>
        /// Gets or sets the name of the input event.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] NameBytes;

        /// <summary>
        /// Gets or sets the hash ID for the event.
        /// </summary>
        public uint Hash;

        /// <summary>
        /// Gets or sets the expected data type (from the SimConnectDataType enum).
        /// Usually a Float32 or String128.
        /// </summary>
        public SimConnectDataType Type;

        /// <summary>
        /// Gets or sets a list of the names of the nodes linked to this input event.
        /// Each node name is separated by a semicolon (;).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] NodeNamesBytes;

        /// <summary>
        /// Gets the name of the input event as a string.
        /// </summary>
        public readonly string? Name
        {
            get
            {
                if (this.NameBytes == null)
                {
                    return null;
                }

                var nullIndex = Array.IndexOf(this.NameBytes, (byte)0);
                var length = nullIndex >= 0 ? nullIndex : this.NameBytes.Length;
                return System.Text.Encoding.UTF8.GetString(this.NameBytes, 0, length);
            }
        }

        /// <summary>
        /// Gets the list of node names as a string.
        /// Each node name is separated by a semicolon (;).
        /// </summary>
        public readonly string? NodeNames
        {
            get
            {
                if (this.NodeNamesBytes == null)
                {
                    return null;
                }

                var nullIndex = Array.IndexOf(this.NodeNamesBytes, (byte)0);
                var length = nullIndex >= 0 ? nullIndex : this.NodeNamesBytes.Length;
                return System.Text.Encoding.UTF8.GetString(this.NodeNamesBytes, 0, length);
            }
        }
    }
}
