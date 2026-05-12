// <copyright file="SimVarMemoryReader.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>
using System;
using System.Runtime.InteropServices;

namespace SimConnect.NET.SimVar.Internal
{
    internal static class SimVarMemoryReader
    {
        // Concrete readers (no boxing): call these from sites that know the expected type.
        public static double ReadDouble(IntPtr addr)
        {
            // Zero-allocation: read as Int64 and reinterpret as double
            return BitConverter.Int64BitsToDouble(Marshal.ReadInt64(addr));
        }

        public static float ReadFloat(IntPtr addr)
        {
            // Zero-allocation: read as Int32 and reinterpret as float
            return BitConverter.Int32BitsToSingle(Marshal.ReadInt32(addr));
        }

        public static long ReadInt64(IntPtr addr) => Marshal.ReadInt64(addr);

        public static int ReadInt32(IntPtr addr) => Marshal.ReadInt32(addr);

        public static string ReadFixedString(IntPtr addr, int size)
        {
            int length = 0;
            while (length < size && Marshal.ReadByte(addr, length) != 0)
            {
                length++;
            }

            return length == 0
                ? string.Empty
                : Marshal.PtrToStringAnsi(addr, length) ?? string.Empty;
        }
    }
}
