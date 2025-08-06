// <copyright file="SimConnectRecvSubscribeInputEvent.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the value of a subscribed input event.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the value of the requested input event, which should be cast to the correct format (e.g., float or string).
        /// </summary>
        public IntPtr Value { get; set; }
    }
}
