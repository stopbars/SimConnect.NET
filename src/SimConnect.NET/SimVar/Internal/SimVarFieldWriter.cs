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
                        var bytes = System.Text.Encoding.ASCII.GetBytes(s);

                        // zero-initialize then copy up to Size
                        Span<byte> tmp = stackalloc byte[this.Size];
                        var copyLen = Math.Min(bytes.Length, this.Size);
                        bytes.AsSpan(0, copyLen).CopyTo(tmp);
                        Marshal.Copy(tmp.ToArray(), 0, addr, this.Size);
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
