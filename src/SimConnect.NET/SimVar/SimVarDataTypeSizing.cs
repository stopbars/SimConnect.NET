// <copyright file="SimVarDataTypeSizing.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>
using SimConnect.NET;

namespace SimConnect.NET.SimVar
{
    /// <summary>
    /// Provides utilities for determining the size and offsets of SimConnect data types in unmanaged payloads.
    /// </summary>
    public static class SimVarDataTypeSizing
    {
        /// <summary>
        /// Raw bytes for one datum of the given SimConnect type in the untagged payload.
        /// </summary>
        /// <param name="type">The SimConnect data type to evaluate.</param>
        /// <returns>The size in bytes of a single datum of the specified type.</returns>
        public static int GetDatumSizeBytes(SimConnectDataType type) => type switch
        {
            SimConnectDataType.Invalid => 0,

            // Scalars (raw sizes)
            SimConnectDataType.Integer32 => 4,      // uint (SimConnect uses unsigned 32-bit for Integer32)
            SimConnectDataType.Integer64 => 8,      // long
            SimConnectDataType.FloatSingle => 4,    // float
            SimConnectDataType.FloatDouble => 8,    // double

            // Fixed-length strings (ANSI, fixed buffer including NUL)
            SimConnectDataType.String8 => 8,        // string
            SimConnectDataType.String32 => 32,      // string
            SimConnectDataType.String64 => 64,      // string
            SimConnectDataType.String128 => 128,    // string
            SimConnectDataType.String256 => 256,    // string
            SimConnectDataType.String260 => 260,    // string

            // Not supported in this marshaller
            SimConnectDataType.StringV => throw new NotSupportedException(
                "StringV is not supported. Use fixed-length String8..String260."),

            // Composite structs (per SDK)
            SimConnectDataType.LatLonAlt => 24, // 3 x double
            SimConnectDataType.Xyz => 24, // 3 x double
            SimConnectDataType.InitPosition => 56, // 6 x double + 2 x DWORD (pack=1)

            // These depend on your interop definition; use Marshal.SizeOf<T> in your code.
            SimConnectDataType.MarkerState => throw new NotSupportedException("Use Marshal.SizeOf<SIMCONNECT_DATA_MARKERSTATE>()."),
            SimConnectDataType.Waypoint => throw new NotSupportedException("Use Marshal.SizeOf<SIMCONNECT_DATA_WAYPOINT>()."),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        /// <summary>
        /// Total payload size (bytes) for a sequence of datums in untagged SIMOBJECT_DATA.
        /// </summary>
        /// <param name="types">The sequence of SimConnect data types to calculate the total payload size for.</param>
        /// <returns>The total size in bytes of the payload for the provided sequence of data types.</returns>
        public static int GetPayloadSizeBytes(IEnumerable<SimConnectDataType> types)
            => types.Sum(GetDatumSizeBytes);

        /// <summary>
        /// Compute byte offsets for each datum in order (untagged).
        /// </summary>
        /// <param name="types">The list of SimConnect data types to compute offsets for.</param>
        /// <returns>An array of byte offsets for each datum in the provided list.</returns>
        public static int[] ComputeOffsets(IEnumerable<SimConnectDataType> types)
        {
            var offsets = new List<int>();
            int cursor = 0;
            foreach (var type in types)
            {
                offsets.Add(cursor);
                cursor += GetDatumSizeBytes(type);
            }

            return offsets.ToArray();
        }
    }
}
