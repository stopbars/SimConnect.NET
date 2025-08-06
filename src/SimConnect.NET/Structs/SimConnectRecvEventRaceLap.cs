// <copyright file="SimConnectRecvEventRaceLap.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectRecvEventRaceLap structure is used in multi-player racing to hold the results for one player at the end of a lap.
    /// </summary>
    public struct SimConnectRecvEventRaceLap
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
        /// Gets or sets the index of the lap the results are for. Laps are indexed from 0.
        /// </summary>
        public uint LapIndex { get; set; }

        /// <summary>
        /// Gets or sets the race result data for the racer.
        /// </summary>
        public SimConnectDataRaceResult RacerData { get; set; }
    }
}
