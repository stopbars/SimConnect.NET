// <copyright file="NdbType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of NDB transmitter.
    /// </summary>
    public enum NdbType
    {
        /// <summary>Compass Locator.</summary>
        CompassLocator = 0,

        /// <summary>Medium Homing beacon.</summary>
        MediumHoming = 1,

        /// <summary>Homing beacon.</summary>
        Homing = 2,

        /// <summary>High Homing beacon.</summary>
        HighHoming = 3,
    }
}
