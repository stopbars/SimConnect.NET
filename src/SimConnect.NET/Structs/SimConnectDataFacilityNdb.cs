// <copyright file="SimConnectDataFacilityNdb.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents information about a single NDB station in the facilities cache.
    /// </summary>
    public struct SimConnectDataFacilityNdb
    {
        /// <summary>
        /// Gets or sets the frequency of the station in Hz.
        /// </summary>
        public uint Frequency { get; set; }
    }
}
