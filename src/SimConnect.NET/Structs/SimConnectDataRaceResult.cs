// <copyright file="SimConnectDataRaceResult.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the multiplayer racing results.
    /// </summary>
    public struct SimConnectDataRaceResult
    {
        /// <summary>
        /// Gets or sets the total number of racers.
        /// </summary>
        public uint NumberOfRacers { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the mission selected by the host.
        /// </summary>
        public Guid MissionGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the player.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Gets or sets the type of the multiplayer session.
        /// </summary>
        public string SessionType { get; set; }

        /// <summary>
        /// Gets or sets the aircraft type.
        /// </summary>
        public string Aircraft { get; set; }

        /// <summary>
        /// Gets or sets the player's role or name in the mission.
        /// </summary>
        public string PlayerRole { get; set; }

        /// <summary>
        /// Gets or sets the final race time in seconds, or 0 for DNF (Did Not Finish).
        /// </summary>
        public double TotalTime { get; set; }

        /// <summary>
        /// Gets or sets the final penalty time in seconds.
        /// </summary>
        public double PenaltyTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player has been disqualified.
        /// </summary>
        public bool IsDisqualified { get; set; }
    }
}
