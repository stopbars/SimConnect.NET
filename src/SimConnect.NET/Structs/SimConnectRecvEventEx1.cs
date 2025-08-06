// <copyright file="SimConnectRecvEventEx1.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEventEx1 structure is used to return an event ID to the client, along with up to 5 parameters.
    /// </summary>
    public struct SimConnectRecvEventEx1
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
        /// Gets or sets the first parameter passed along with the event.
        /// </summary>
        public uint Data0 { get; set; }

        /// <summary>
        /// Gets or sets the second parameter passed along with the event.
        /// </summary>
        public uint Data1 { get; set; }

        /// <summary>
        /// Gets or sets the third parameter passed along with the event.
        /// </summary>
        public uint Data2 { get; set; }

        /// <summary>
        /// Gets or sets the fourth parameter passed along with the event.
        /// </summary>
        public uint Data3 { get; set; }

        /// <summary>
        /// Gets or sets the fifth parameter passed along with the event.
        /// </summary>
        public uint Data4 { get; set; }
    }
}
