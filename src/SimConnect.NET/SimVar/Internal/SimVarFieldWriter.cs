// <copyright file="SimVarFieldWriter.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace SimConnect.NET.SimVar.Internal
{
    internal sealed class SimVarFieldWriter<T, TSrc> : IFieldWriter<T>
        where T : struct
    {
        public int OffsetBytes { get; set; }

        public int Size { get; set; }

        public SimConnectDataType DataType { get; set; }

        // Holds a typed extractor matching the field type, e.g. Func<T, double>
        public Delegate Extractor { get; set; } = default!;

        public void WriteFrom(in T source, IntPtr basePtr)
        {
            var addr = IntPtr.Add(basePtr, this.OffsetBytes);
            switch (this.DataType)
            {
                case SimConnectDataType.FloatDouble:
                    {
                        var getter = (Func<T, double>)this.Extractor;
                        double v = getter(source);
                        var bytes = BitConverter.GetBytes(v);
                        Marshal.Copy(bytes, 0, addr, 8);
                        break;
                    }

                case SimConnectDataType.FloatSingle:
                    {
                        var getter = (Func<T, float>)this.Extractor;
                        float v = getter(source);
                        var bytes = BitConverter.GetBytes(v);
                        Marshal.Copy(bytes, 0, addr, 4);
                        break;
                    }

                case SimConnectDataType.Integer64:
                    {
                        var getter = (Func<T, long>)this.Extractor;
                        long v = getter(source);
                        Marshal.WriteInt64(addr, v);
                        break;
                    }

                case SimConnectDataType.Integer32:
                    {
                        var getter = (Func<T, int>)this.Extractor;
                        int v = getter(source);
                        Marshal.WriteInt32(addr, v);
                        break;
                    }

                case SimConnectDataType.String8:
                case SimConnectDataType.String32:
                case SimConnectDataType.String64:
                case SimConnectDataType.String128:
                case SimConnectDataType.String256:
                case SimConnectDataType.String260:
                    {
                        var getter = (Func<T, string>)this.Extractor;
                        string s = getter(source) ?? string.Empty;

                        // zero-initialize then encode into span up to Size-1, ensure explicit null-termination
                        // SimConnect expects fixed-size, null-terminated ANSI strings.
                        // We reserve the last byte for '\0' when Size > 0 to avoid losing the terminator.
                        Span<byte> tmp = stackalloc byte[this.Size];
                        if (this.Size > 0)
                        {
                            var dest = tmp[..(this.Size - 1)];
                            _ = System.Text.Encoding.Latin1.GetBytes(s.AsSpan(), dest);

                            // Explicitly set terminator even though tmp is zeroed by default
                            tmp[this.Size - 1] = 0;
                        }

                        // Copy without allocating an intermediate array
                        for (int i = 0; i < this.Size; i++)
                        {
                            Marshal.WriteByte(addr, i, tmp[i]);
                        }

                        break;
                    }

                case SimConnectDataType.InitPosition:
                    {
                        var getter = (Func<T, SimConnectDataInitPosition>)this.Extractor;
                        var v = getter(source);
                        Marshal.StructureToPtr(v, addr, fDeleteOld: false);
                        break;
                    }

                case SimConnectDataType.MarkerState:
                    {
                        var getter = (Func<T, SimConnectDataMarkerState>)this.Extractor;
                        var v = getter(source);
                        Marshal.StructureToPtr(v, addr, fDeleteOld: false);
                        break;
                    }

                case SimConnectDataType.Waypoint:
                    {
                        var getter = (Func<T, SimConnectDataWaypoint>)this.Extractor;
                        var v = getter(source);
                        Marshal.StructureToPtr(v, addr, fDeleteOld: false);
                        break;
                    }

                case SimConnectDataType.LatLonAlt:
                    {
                        var getter = (Func<T, SimConnectDataLatLonAlt>)this.Extractor;
                        var v = getter(source);
                        Marshal.StructureToPtr(v, addr, fDeleteOld: false);
                        break;
                    }

                case SimConnectDataType.Xyz:
                    {
                        var getter = (Func<T, SimConnectDataXyz>)this.Extractor;
                        var v = getter(source);
                        Marshal.StructureToPtr(v, addr, fDeleteOld: false);
                        break;
                    }

                default:
                    throw new NotSupportedException($"Unsupported SimConnectDataType {this.DataType}");
            }
        }
    }
}
