// <copyright file="RawSimConnectMessageEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for the <see cref="SimConnectClient.RawMessageReceived"/> event, exposing the raw
    /// SimConnect dispatch pointer and size for advanced consumers that need low-level access.
    /// The data memory referenced by <see cref="DataPointer"/> is only valid for the duration of the event callback.
    /// </summary>
    public sealed class RawSimConnectMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawSimConnectMessageEventArgs"/> class.
        /// </summary>
        /// <param name="dataPointer">Pointer to the raw SimConnect message memory.</param>
        /// <param name="dataSize">Size (in bytes) of the message pointed to by <paramref name="dataPointer"/>.</param>
        /// <param name="messageId">The SimConnect receive message identifier.</param>
        public RawSimConnectMessageEventArgs(IntPtr dataPointer, uint dataSize, SimConnectRecvId messageId)
        {
            this.DataPointer = dataPointer;
            this.DataSize = dataSize;
            this.MessageId = messageId;
        }

        /// <summary>
        /// Gets the pointer to the raw SimConnect message memory. Valid only within the event scope.
        /// </summary>
        public IntPtr DataPointer { get; }

        /// <summary>
        /// Gets the size (in bytes) of the raw message.
        /// </summary>
        public uint DataSize { get; }

        /// <summary>
        /// Gets the SimConnect receive message identifier for this message.
        /// </summary>
        public SimConnectRecvId MessageId { get; }
    }
}
