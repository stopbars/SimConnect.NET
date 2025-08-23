// <copyright file="SimConnectRecvGetInputEvent.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvGetInputEventHeader structure is used to return the header of a specific input event including the type but without the value.
    /// </summary>
    public struct SimConnectRecvGetInputEventHeader
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
        /// Gets or sets the client-defined request ID.
        /// </summary>
        public uint RequestId { get; set; }

        /// <summary>
        /// Gets or sets the type of the input event. This is used to cast the Value to the correct type.
        /// </summary>
        public SimConnectInputEventType Type { get; set; }
    }
}
