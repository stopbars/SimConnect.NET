// <copyright file="StartType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of starting point.
    /// </summary>
    public enum StartType
    {
        /// <summary>Unknown start type.</summary>
        Unknown = 0,

        /// <summary>Runway start type.</summary>
        Runway = 1,

        /// <summary>Water start type.</summary>
        Water = 2,

        /// <summary>Helipad start type.</summary>
        Helipad = 3,

        /// <summary>Track start type.</summary>
        Track = 4,
    }
}
