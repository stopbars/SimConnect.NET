// <copyright file="SimConnectRecvFacilityData.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvFacilityData structure is used to provide information that has been requested from the server using the SimConnect_RequestFacilityData function.
    /// This struct may be received multiple times before receiving SimConnectRecvFacilityDataEnd.
    /// </summary>
    public struct SimConnectRecvFacilityData
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
        public uint UserRequestId { get; set; }

        /// <summary>
        /// Gets or sets the unique request ID, so the client can identify it.
        /// </summary>
        public uint UniqueRequestId { get; set; }

        /// <summary>
        /// Gets or sets the parent's unique request ID if the current message is about a child object; otherwise, 0.
        /// </summary>
        public uint ParentUniqueRequestId { get; set; }

        /// <summary>
        /// Gets or sets the type of the object, which will be a value from the SimConnectFacilityDataType enum.
        /// </summary>
        public SimConnectFacilityDataType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current message is about a child object and specifies if it is an orphan object or not.
        /// </summary>
        public bool IsListItem { get; set; }

        /// <summary>
        /// Gets or sets the index in the list if IsListItem is true.
        /// </summary>
        public uint ItemIndex { get; set; }

        /// <summary>
        /// Gets or sets the list size if IsListItem is true.
        /// </summary>
        public uint ListSize { get; set; }

        /// <summary>
        /// Gets or sets the buffer of data. This must be cast to a struct that matches the definition.
        /// </summary>
        public uint Data { get; set; }
    }
}
