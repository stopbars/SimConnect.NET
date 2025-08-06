// <copyright file="HelipadType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of helipad.
    /// </summary>
    public enum HelipadType
    {
        /// <summary>No specific helipad type.</summary>
        None = 0,

        /// <summary>H-marked helipad.</summary>
        H = 1,

        /// <summary>Square helipad.</summary>
        Square = 2,

        /// <summary>Circle helipad.</summary>
        Circle = 3,

        /// <summary>Medical helipad.</summary>
        Medical = 4,
    }
}
