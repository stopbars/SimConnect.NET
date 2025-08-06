// <copyright file="FrequencyType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the type of radio frequency.
    /// </summary>
    public enum FrequencyType
    {
        /// <summary>No frequency type.</summary>
        None = 0,

        /// <summary>ATIS frequency.</summary>
        Atis = 1,

        /// <summary>Multicom frequency.</summary>
        Multicom = 2,

        /// <summary>Unicom frequency.</summary>
        Unicom = 3,

        /// <summary>CTAF frequency.</summary>
        Ctaf = 4,

        /// <summary>Ground frequency.</summary>
        Ground = 5,

        /// <summary>Tower frequency.</summary>
        Tower = 6,

        /// <summary>Clearance Delivery frequency.</summary>
        Clearance = 7,

        /// <summary>Approach frequency.</summary>
        Approach = 8,

        /// <summary>Departure frequency.</summary>
        Departure = 9,

        /// <summary>Center frequency.</summary>
        Center = 10,

        /// <summary>FSS frequency.</summary>
        Fss = 11,

        /// <summary>AWOS frequency.</summary>
        Awos = 12,

        /// <summary>ASOS frequency.</summary>
        Asos = 13,

        /// <summary>Clearance Pre-Taxi frequency.</summary>
        Cpt = 14,

        /// <summary>Remote Clearance Delivery frequency.</summary>
        Gco = 15,
    }
}
