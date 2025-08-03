// <copyright file="SimConnectDataPbh.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectDataPbh structure is used to hold a world orientation.
    /// </summary>
    public struct SimConnectDataPbh
    {
        /// <summary>
        /// Gets or sets the pitch in degrees.
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// Gets or sets the bank in degrees.
        /// </summary>
        public float Bank { get; set; }

        /// <summary>
        /// Gets or sets the heading in degrees.
        /// </summary>
        public float Heading { get; set; }
    }
}
