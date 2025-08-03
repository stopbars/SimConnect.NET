// <copyright file="SimConnectRecvVorList.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvVorList structure is used to return a list of SimConnectDataFacilityVor structures.
    /// </summary>
    public struct SimConnectRecvVorList
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
        /// Gets or sets an integer or boolean value.
        /// </summary>
        public uint IntegerValue { get; set; }

        /// <summary>
        /// Gets or sets a float value.
        /// </summary>
        public float FloatValue { get; set; }

        /// <summary>
        /// Gets or sets a null-terminated string.
        /// </summary>
        public string StringValue { get; set; }
    }
}
