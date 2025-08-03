// <copyright file="SimConnectDataXyz.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectDataXyz struct is used to hold a 3D coordinate.
    /// </summary>
    public struct SimConnectDataXyz
    {
        /// <summary>
        /// Gets or sets the position along the x-axis.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the position along the y-axis.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the position along the z-axis.
        /// </summary>
        public double Z { get; set; }
    }
}
