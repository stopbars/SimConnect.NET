// <copyright file="SimVarFieldWriterFactory.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SimConnect.NET.SimVar.Internal
{
    internal static class SimVarFieldWriterFactory
    {
        /// <summary>
        /// Builds field writers for a struct T and optionally adds each field to a SimConnect definition.
        /// Mirrors the reader factory logic to ensure identical packing order and sizes.
        /// </summary>
        public static (List<IFieldWriter<T>> Writers, int TotalSize) Build<T>(
            Action<string /*name*/, string? /*unit*/, SimConnectDataType /*type*/>? addToDefinition = null)
            where T : struct
        {
            var t = typeof(T);
            var fields = GetOrderedSimVarFields(t);
            if (fields.Count == 0)
            {
                throw new InvalidOperationException($"Type {t.FullName} has no fields with [SimConnect].");
            }

            var writers = new List<IFieldWriter<T>>(fields.Count);
            int offset = 0;

            foreach (var (field, simVar) in fields)
            {
                if (field == null)
                {
                    throw new InvalidOperationException("FieldInfo is null in SimVarFieldWriterFactory.Build.");
                }

                if (simVar == null)
                {
                    throw new InvalidOperationException($"SimConnectAttribute is null for field '{field.Name}' in SimVarFieldWriterFactory.Build.");
                }

                // Determine effective data type matching the reader factory rules
                SimConnectDataType effectiveDataType;
                if (simVar.DataType.HasValue)
                {
                    effectiveDataType = simVar.DataType.Value;
                }
                else
                {
                    var ft = field.FieldType;
                    var nullableUnderlying = Nullable.GetUnderlyingType(ft);
                    if (nullableUnderlying != null)
                    {
                        ft = nullableUnderlying;
                    }

                    if (ft.IsEnum)
                    {
                        ft = Enum.GetUnderlyingType(ft);
                    }

                    effectiveDataType = ft switch
                    {
                        _ when ft == typeof(double) => SimConnectDataType.FloatDouble,
                        _ when ft == typeof(float) => SimConnectDataType.FloatSingle,
                        _ when ft == typeof(long) || ft == typeof(ulong) => SimConnectDataType.Integer64,
                        _ when ft == typeof(int) || ft == typeof(uint) ||
                               ft == typeof(short) || ft == typeof(ushort) ||
                               ft == typeof(byte) || ft == typeof(sbyte) ||
                               ft == typeof(bool) => SimConnectDataType.Integer32,
                        _ when ft == typeof(SimConnectDataInitPosition) => SimConnectDataType.InitPosition,
                        _ when ft == typeof(SimConnectDataMarkerState) => SimConnectDataType.MarkerState,
                        _ when ft == typeof(SimConnectDataWaypoint) => SimConnectDataType.Waypoint,
                        _ when ft == typeof(SimConnectDataLatLonAlt) => SimConnectDataType.LatLonAlt,
                        _ when ft == typeof(SimConnectDataXyz) => SimConnectDataType.Xyz,
                        _ when ft == typeof(string) => SimConnectDataType.String256,
                        _ => throw new NotSupportedException($"Cannot infer SimConnectDataType for field '{field.Name}' of type {ft.FullName}."),
                    };
                }

                addToDefinition?.Invoke(simVar.Name, simVar.Unit, effectiveDataType);

                var (dataType, rawType, size) = Classify(field, effectiveDataType);

                var writerType = typeof(SimVarFieldWriter<,>).MakeGenericType(t, field.FieldType);
                var writer = Activator.CreateInstance(writerType)!;

                dynamic d = writer;
                d.OffsetBytes = offset;
                d.DataType = dataType;
                d.Size = size;
                d.Extractor = BuildExtractor(t, field, rawType);

                writers.Add((IFieldWriter<T>)writer);
                offset += size;
            }

            return (writers, offset);
        }

        private static List<(FieldInfo Field, SimConnectAttribute? Attr)> GetOrderedSimVarFields(Type t)
        {
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(f => (Field: f, Attr: f.GetCustomAttribute<SimConnectAttribute>()))
                .Where(x => x.Attr != null)
                .OrderBy(x => x!.Attr!.Order)
                .ThenBy(x => x.Field.MetadataToken)
                .ToList();

            return fields;
        }

        private static (SimConnectDataType DataType, Type RawType, int SizeBytes) Classify(FieldInfo field, SimConnectDataType dt)
        {
            switch (dt)
            {
                case SimConnectDataType.FloatDouble:
                    return (SimConnectDataType.FloatDouble, typeof(double), 8);
                case SimConnectDataType.FloatSingle:
                    return (SimConnectDataType.FloatSingle, typeof(float), 4);
                case SimConnectDataType.Integer32:
                    return (SimConnectDataType.Integer32, typeof(int), 4);
                case SimConnectDataType.Integer64:
                    return (SimConnectDataType.Integer64, typeof(long), 8);
                case SimConnectDataType.String8:
                    return (SimConnectDataType.String8, typeof(string), 8);
                case SimConnectDataType.String32:
                    return (SimConnectDataType.String32, typeof(string), 32);
                case SimConnectDataType.String64:
                    return (SimConnectDataType.String64, typeof(string), 64);
                case SimConnectDataType.String128:
                    return (SimConnectDataType.String128, typeof(string), 128);
                case SimConnectDataType.String256:
                    return (SimConnectDataType.String256, typeof(string), 256);
                case SimConnectDataType.String260:
                    return (SimConnectDataType.String260, typeof(string), 260);
                case SimConnectDataType.InitPosition:
                    return (SimConnectDataType.InitPosition, typeof(SimConnectDataInitPosition), Marshal.SizeOf<SimConnectDataInitPosition>());
                case SimConnectDataType.MarkerState:
                    return (SimConnectDataType.MarkerState, typeof(SimConnectDataMarkerState), Marshal.SizeOf<SimConnectDataMarkerState>());
                case SimConnectDataType.Waypoint:
                    return (SimConnectDataType.Waypoint, typeof(SimConnectDataWaypoint), Marshal.SizeOf<SimConnectDataWaypoint>());
                case SimConnectDataType.LatLonAlt:
                    return (SimConnectDataType.LatLonAlt, typeof(SimConnectDataLatLonAlt), Marshal.SizeOf<SimConnectDataLatLonAlt>());
                case SimConnectDataType.Xyz:
                    return (SimConnectDataType.Xyz, typeof(SimConnectDataXyz), Marshal.SizeOf<SimConnectDataXyz>());
                default:
                    throw new NotSupportedException($"{field.DeclaringType!.FullName}.{field.Name}: unsupported SimConnectDataType {dt}");
            }
        }

        /// <summary>
        /// Builds an extractor that returns the field value converted to the requested raw type.
        /// </summary>
        private static Delegate BuildExtractor(Type structType, FieldInfo fi, Type rawType)
        {
            // param: T s
            var s = Expression.Parameter(structType, "s");

            // access field: s.Field
            var fieldExpr = Expression.Field(s, fi);

            // If the field type equals rawType -> identity
            if (fi.FieldType == rawType)
            {
                var lambdaTypeId = typeof(Func<,>).MakeGenericType(structType, rawType);
                return Expression.Lambda(lambdaTypeId, fieldExpr, s).Compile();
            }

            // If field is Nullable<U>, unwrap .Value or default
            Type destFieldType = fi.FieldType;
            var nullableUnderlying = Nullable.GetUnderlyingType(destFieldType);
            Expression valueExpr = fieldExpr;
            if (nullableUnderlying != null)
            {
                // coalesce: field.HasValue ? field.Value : default(U)
                var valueProp = Expression.Property(fieldExpr, "Value");
                var defaultValue = Expression.Default(nullableUnderlying);
                valueExpr = Expression.Condition(
                    Expression.Property(fieldExpr, "HasValue"),
                    valueProp,
                    defaultValue);
                destFieldType = nullableUnderlying;
            }

            // Enums -> convert to underlying integral type first
            if (destFieldType.IsEnum)
            {
                var underlying = Enum.GetUnderlyingType(destFieldType);
                if (valueExpr.Type != underlying)
                {
                    valueExpr = Expression.Convert(valueExpr, underlying);
                }

                destFieldType = underlying;
            }

            // Finally convert to rawType
            if (valueExpr.Type != rawType)
            {
                valueExpr = Expression.Convert(valueExpr, rawType);
            }

            var lambdaType = typeof(Func<,>).MakeGenericType(structType, rawType);
            return Expression.Lambda(lambdaType, valueExpr, s).Compile();
        }
    }
}
