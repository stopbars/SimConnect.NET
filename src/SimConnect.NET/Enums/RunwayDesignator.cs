// <copyright file="RunwayDesignator.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents runway designator types.
    /// </summary>
    public enum RunwayDesignator
    {
        /// <summary>No specific designator.</summary>
        None = 0,

        /// <summary>Left runway designator.</summary>
        Left = 1,

        /// <summary>Right runway designator.</summary>
        Right = 2,

        /// <summary>Center runway designator.</summary>
        Center = 3,

        /// <summary>Water runway designator.</summary>
        Water = 4,

        /// <summary>A runway designator.</summary>
        A = 5,

        /// <summary>B runway designator.</summary>
        B = 6,

        /// <summary>Last runway designator.</summary>
        Last = 7,
    }
}
