// <copyright file="VorType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of VOR station.
    /// </summary>
    public enum VorType
    {
        /// <summary>Unknown VOR type.</summary>
        Unknown = 0,

        /// <summary>Terminal VOR.</summary>
        Terminal = 1,

        /// <summary>Low altitude VOR.</summary>
        LowAltitude = 2,

        /// <summary>Low altitude VOR (alternative).</summary>
        LowAlt = 3,

        /// <summary>High altitude VOR.</summary>
        HighAltitude = 4,

        /// <summary>High altitude VOR (alternative).</summary>
        HighAlt = 5,

        /// <summary>ILS VOR.</summary>
        Ils = 6,

        /// <summary>VOR test facility.</summary>
        Vot = 7,
    }
}
