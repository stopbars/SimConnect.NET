// <copyright file="SimConnectIcao.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the ICAO code of a facility.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct SimConnectIcao
    {
        /// <summary>
        /// The type of the ICAO code.
        /// </summary>
        public char Type;

        /// <summary>
        /// The identity string (fixed 6 characters).
        /// </summary>
        public char Ident;

        /// <summary>
        /// The region string (fixed 3 characters).
        /// </summary>
        public char Region;

        /// <summary>
        /// The airport string (fixed 5 characters).
        /// </summary>
        public char Airport;
    }
}
