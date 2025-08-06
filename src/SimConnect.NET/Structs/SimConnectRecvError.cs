// <copyright file="SimConnectRecvError.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents an exception received from SimConnect.
    /// </summary>
    public struct SimConnectRecvError
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
        /// Gets or sets the exception code, indicating which error has occurred.
        /// </summary>
        public uint ExceptionCode { get; set; }

        /// <summary>
        /// Gets or sets the ID of the packet that contained the error.
        /// </summary>
        public uint SendId { get; set; }

        /// <summary>
        /// Gets or sets the index number of the first parameter that caused an error.
        /// Special case: UNKNOWN_INDEX = 0.
        /// </summary>
        public uint Index { get; set; }
    }
}
