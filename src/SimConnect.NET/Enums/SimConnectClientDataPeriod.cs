// <copyright file="SimConnectClientDataPeriod.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The <see cref="SimConnectClientDataPeriod"/> enumeration type is used with the SimConnect_RequestClientData call to specify how often data is to be sent to the client.
    /// </summary>
    public enum SimConnectClientDataPeriod
    {
        /// <summary>
        /// Specifies that the data is not to be sent.
        /// </summary>
        SimconnectPeriodNever,

        /// <summary>
        /// Specifies that the data should be sent once only. Note that this is not an efficient way of receiving data frequently; use one of the other periods if there is a regular frequency to the data request.
        /// </summary>
        SimconnectPeriodOnce,

        /// <summary>
        /// Specifies that the data should be sent every visual (rendered) frame.
        /// </summary>
        SimconnectPeriodVisualFrame,

        /// <summary>
        /// Specifies that the data should be sent whenever it is set.
        /// </summary>
        SimconnectPeriodOnSet,

        /// <summary>
        /// Specifies that the data should be sent once every second.
        /// </summary>
        SimconnectPeriodSecond,
    }
}
