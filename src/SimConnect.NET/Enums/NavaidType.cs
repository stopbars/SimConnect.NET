// <copyright file="NavaidType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of navigation aid.
    /// </summary>
    public enum NavaidType
    {
        /// <summary>Airport navaid.</summary>
        Airport = 'A',

        /// <summary>VOR navaid.</summary>
        Vor = 'V',

        /// <summary>NDB navaid.</summary>
        Ndb = 'N',

        /// <summary>Waypoint navaid.</summary>
        Waypoint = 'W',
    }
}
