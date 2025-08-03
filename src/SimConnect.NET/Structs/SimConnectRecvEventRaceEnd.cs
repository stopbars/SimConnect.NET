// <copyright file="SimConnectRecvEventRaceEnd.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the results for one player at the end of a multi-player race.
    /// </summary>
    public struct SimConnectRecvEventRaceEnd
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
        /// Gets or sets the index of the racer the results are for. Players are indexed from 0.
        /// </summary>
        public uint DwRacerNumber { get; set; }

        /// <summary>
        /// Gets or sets the race result data for the racer.
        /// </summary>
        public SimConnectDataRaceResult RacerData { get; set; }
    }
}
