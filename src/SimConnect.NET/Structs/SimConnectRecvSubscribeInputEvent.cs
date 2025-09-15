// <copyright file="SimConnectRecvSubscribeInputEvent.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the header for a subscribed input event notification.
    /// The value payload immediately follows this header in memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SimConnectRecvSubscribeInputEvent
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
        /// Gets or sets the hash ID that identifies the subscribed input event.
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// Gets or sets the type of the input event, which is a member of the <see cref="SimConnectInputEventType"/> enumeration.
        /// </summary>
        public SimConnectInputEventType Type { get; set; }
    }
}
