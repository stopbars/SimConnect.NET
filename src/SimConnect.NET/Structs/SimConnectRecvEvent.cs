// <copyright file="SimConnectRecvEvent.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEvent structure is used to return an event ID to the client.
    /// </summary>
    public struct SimConnectRecvEvent
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
        /// Gets or sets the ID of the client-defined group, or the special case value: UNKNOWN_GROUP (which equals DWORD_MAX).
        /// </summary>
        public uint GroupId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the client-defined event that has been requested (such as EVENT_1 or EVENT_BRAKES).
        /// </summary>
        public uint EventId { get; set; }

        /// <summary>
        /// Gets or sets the value that is usually zero, but some events require further qualification.
        /// For example, joystick movement events require a movement value in addition to the notification that the joystick has been moved.
        /// </summary>
        public uint Data { get; set; }
    }
}
