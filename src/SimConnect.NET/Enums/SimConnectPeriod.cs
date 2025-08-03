// <copyright file="SimConnectPeriod.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectPeriod enumeration specifies how often data is to be sent to the client.
    /// </summary>
    public enum SimConnectPeriod
    {
        /// <summary>
        /// Specifies that the data is not to be sent.
        /// </summary>
        Never,

        /// <summary>
        /// Specifies that the data should be sent once only. Note that this is not an efficient way of receiving data frequently; use one of the other periods if there is a regular frequency to the data request.
        /// </summary>
        Once,

        /// <summary>
        /// Specifies that the data should be sent every visual (rendered) frame.
        /// </summary>
        VisualFrame,

        /// <summary>
        /// Specifies that the data should be sent every simulated frame, whether that frame is rendered or not.
        /// </summary>
        SimFrame,

        /// <summary>
        /// Specifies that the data should be sent once every second.
        /// </summary>
        Second,
    }
}
