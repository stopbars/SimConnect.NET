// <copyright file="SimConnectEventOptions.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET
{
    /// <summary>
    /// Options controlling behavior of transmitted SimConnect client events.
    /// Mirrors the native SIMCONNECT_EVENT_FLAG values.
    /// </summary>
    [Flags]
    public enum SimConnectEventOptions : uint
    {
        /// <summary>
        /// No special behavior.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Reset the repeat timer to simulate a fast repeat (e.g. key held down quickly).
        /// Native constant: SIMCONNECT_EVENT_FLAG_FAST_REPEAT_TIMER (0x1).
        /// </summary>
        FastRepeatTimer = 0x00000001,

        /// <summary>
        /// Reset the repeat timer to simulate a slow repeat.
        /// Native constant: SIMCONNECT_EVENT_FLAG_SLOW_REPEAT_TIMER (0x2).
        /// </summary>
        SlowRepeatTimer = 0x00000002,

        /// <summary>
        /// Treat the provided groupId as a priority when transmitting to other clients.
        /// Native constant: SIMCONNECT_EVENT_FLAG_GROUPID_IS_PRIORITY (0x10).
        /// </summary>
        GroupIdIsPriority = 0x00000010,
    }
}
