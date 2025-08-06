// <copyright file="AirwayType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of airway.
    /// </summary>
    public enum AirwayType
    {
        /// <summary>No specific airway type.</summary>
        None = 0,

        /// <summary>Victor airway.</summary>
        Victor = 1,

        /// <summary>Jet airway.</summary>
        Jet = 2,

        /// <summary>Both Victor and Jet airway.</summary>
        Both = 3,
    }
}
