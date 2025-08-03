// <copyright file="SimConnectMissionEnd.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectMissionEnd enumeration type is used to specify the three possible outcomes of a mission.
    /// </summary>
    public enum SimConnectMissionEnd
    {
        /// <summary>
        /// The mission failed for some reason other than a crash.
        /// </summary>
        SimconnectMissionFailed,

        /// <summary>
        /// The mission failed because of a crash.
        /// </summary>
        SimconnectMissionCrashed,

        /// <summary>
        /// The mission was completed successfully.
        /// </summary>
        SimconnectMissionSucceeded,
    }
}
