// <copyright file="SimVarMemoryReader.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>
using System;
using System.Text;

namespace SimConnect.NET.SimVar.Internal
{
    internal static class SimVarMemoryReader
    {
        // Concrete readers (no boxing): call these from sites that know the expected type.
        public static unsafe double ReadDouble(IntPtr addr) => *(double*)addr;

        public static unsafe float ReadFloat(IntPtr addr) => *(float*)addr;

        public static unsafe long ReadInt64(IntPtr addr) => *(long*)addr;

        public static unsafe int ReadInt32(IntPtr addr) => *(int*)addr;

        public static unsafe string ReadFixedString(IntPtr addr, int size)
        {
            if (size <= 0)
            {
                return string.Empty;
            }

            var buffer = new ReadOnlySpan<byte>(addr.ToPointer(), size);
            var length = buffer.IndexOf((byte)0);
            if (length < 0)
            {
                length = size;
            }

            return length == 0
                ? string.Empty
                : Encoding.ASCII.GetString(buffer[..length]);
        }
    }
}
