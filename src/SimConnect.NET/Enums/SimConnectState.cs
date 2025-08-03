// <copyright file="SimConnectState.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SIMCONNECT_STATE enumeration type is used with the SimConnect_SetSystemEventState call to turn the reporting of events on and off.
    /// </summary>
    public enum SimConnectState
    {
        /// <summary>
        /// Specifies off.
        /// </summary>
        Off,

        /// <summary>
        /// Specifies on.
        /// </summary>
        On,
    }
}
