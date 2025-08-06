// <copyright file="InputGroupPriority.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Defines the priority levels for input groups based on SimConnect constants.
    /// </summary>
    public enum InputGroupPriority : uint
    {
        /// <summary>
        /// The very highest priority (SIMCONNECT_GROUP_PRIORITY_HIGHEST).
        /// </summary>
        Highest = 1,

        /// <summary>
        /// The highest priority that allows events to be masked (SIMCONNECT_GROUP_PRIORITY_HIGHEST_MASKABLE).
        /// No longer used in Microsoft Flight Simulator.
        /// </summary>
        HighestMaskable = 10000000,

        /// <summary>
        /// The standard priority (SIMCONNECT_GROUP_PRIORITY_STANDARD).
        /// </summary>
        Standard = 1900000000,

        /// <summary>
        /// The default priority (SIMCONNECT_GROUP_PRIORITY_DEFAULT).
        /// </summary>
        Default = 2000000000,

        /// <summary>
        /// Priorities lower than this will be ignored (SIMCONNECT_GROUP_PRIORITY_LOWEST).
        /// </summary>
        Lowest = 4000000000,
    }
}
