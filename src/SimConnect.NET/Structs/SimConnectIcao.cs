// <copyright file="SimConnectIcao.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the ICAO code of a facility.
    /// </summary>
    public struct SimConnectIcao
    {
        /// <summary>
        /// Gets or sets the type of the ICAO code.
        /// </summary>
        public char Type { get; set; }

        /// <summary>
        /// Gets or sets the identity string.
        /// </summary>
        public string Ident { get; set; }

        /// <summary>
        /// Gets or sets the region string.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the airport string.
        /// </summary>
        public string Airport { get; set; }
    }
}
