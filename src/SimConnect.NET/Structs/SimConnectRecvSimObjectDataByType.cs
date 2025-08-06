// <copyright file="SimConnectRecvSimObjectDataByType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvSimObjectDataByType structure is received by the client after a successful call to SimConnect_RequestDataOnSimObjectType.
    /// It is identical to the SimConnectRecvSimObjectData structure.
    /// </summary>
    public struct SimConnectRecvSimObjectDataByType
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
        /// Gets or sets the ID of the client-defined request.
        /// </summary>
        public uint RequestId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the client-defined object.
        /// </summary>
        public uint ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the client-defined data definition.
        /// </summary>
        public uint DefineId { get; set; }

        /// <summary>
        /// Gets or sets the flags that were set for this data request.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the index number of this object out of a total of OutOf.
        /// </summary>
        public uint EntryNumber { get; set; }

        /// <summary>
        /// Gets or sets the total number of objects being returned.
        /// </summary>
        public uint OutOf { get; set; }

        /// <summary>
        /// Gets or sets the number of 8-byte elements in the Data array.
        /// </summary>
        public uint DefineCount { get; set; }

        /// <summary>
        /// Gets or sets the data array containing information on a specified object in 8-byte elements.
        /// </summary>
        public ulong Data { get; set; }
    }
}
