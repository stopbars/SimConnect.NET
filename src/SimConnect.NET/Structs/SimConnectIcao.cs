// <copyright file="SimConnectIcao.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Text;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents the ICAO code of a facility.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct SimConnectIcao
    {
        private const int IcaoSize = 9;
        private const int IdentSize = 9;
        private const int RegionSize = 9;
        private const int AirportSize = 5;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IcaoSize)]
        private byte[]? icao;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IdentSize)]
        private byte[]? ident;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = RegionSize)]
        private byte[]? region;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = AirportSize)]
        private byte[]? airport;

        /// <summary>
        /// Gets the ICAO identifier (e.g. "KJFK").
        /// </summary>
        public string Icao => Decode(this.icao);

        /// <summary>
        /// Gets the facility ident (e.g. runway or navaid name).
        /// </summary>
        public string Ident => Decode(this.ident);

        /// <summary>
        /// Gets the region (State/area) code.
        /// </summary>
        public string Region => Decode(this.region);

        /// <summary>
        /// Gets the airport identifier if the facility belongs to an airport.
        /// </summary>
        public string Airport => Decode(this.airport);

        /// <summary>
        /// Creates a new <see cref="SimConnectIcao"/> instance from strings.
        /// </summary>
        /// <param name="icao">ICAO identifier.</param>
        /// <param name="ident">Facility ident.</param>
        /// <param name="region">Region code.</param>
        /// <param name="airport">Airport identifier.</param>
        /// <returns>A populated <see cref="SimConnectIcao"/>.</returns>
        public static SimConnectIcao FromStrings(string? icao, string? ident = null, string? region = null, string? airport = null)
        {
            return new SimConnectIcao
            {
                icao = Encode(IcaoSize, icao),
                ident = Encode(IdentSize, ident),
                region = Encode(RegionSize, region),
                airport = Encode(AirportSize, airport),
            };
        }

        private static string Decode(byte[]? buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return string.Empty;
            }

            return Encoding.ASCII.GetString(buffer).TrimEnd('\0', ' ');
        }

        private static byte[] Encode(int size, string? value)
        {
            var buffer = new byte[size];
            if (!string.IsNullOrEmpty(value))
            {
                var bytes = Encoding.ASCII.GetBytes(value);
                Array.Copy(bytes, buffer, Math.Min(bytes.Length, size - 1));
            }

            return buffer;
        }
    }
}
