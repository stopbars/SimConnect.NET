// <copyright file="SimConnectRecvGetInputEvent.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvGetInputEvent structure is used to return the value of a specific input event.
    /// </summary>
    public struct SimConnectRecvGetInputEvent
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

        /// <summary>
        /// Gets or sets the value of the requested input event, which should be cast to the correct format (float / string).
        /// </summary>
        public IntPtr Value { get; set; }
    }
}
