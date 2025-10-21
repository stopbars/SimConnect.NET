// <copyright file="SimVarFieldReaderFactory.cs" company="BARS">
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
    internal static class SimVarFieldReaderFactory
    {
        /// <summary>
        /// Build readers for T (reflect once). Caller can optionally register each field
        /// into a SimConnect definition by passing addToDefinition. If null, no registration occurs.
        /// </summary>
        public static List<IFieldReader<T>> Build<T>(
            Action<string /*name*/, string? /*unit*/, SimConnectDataType /*type*/>? addToDefinition = null)
            where T : struct
        {
            var t = typeof(T);

            // Collect and order fields with [SimConnect]
            var fields = GetOrderedSimVarFields(t);

            if (fields.Count == 0)
            {
                throw new InvalidOperationException($"Type {t.FullName} has no fields with [SimConnect].");
            }

            var readers = new List<IFieldReader<T>>(fields.Count);
            int offset = 0;

            foreach (var (field, simVar) in fields)
            {
                if (field == null)
                {
                    throw new InvalidOperationException("FieldInfo is null in SimVarFieldReaderFactory.Build.");
                }

                if (simVar == null)
                {
                    throw new InvalidOperationException($"SimConnectAttribute is null for field '{field.Name}' in SimVarFieldReaderFactory.Build.");
                }

                // Determine effective (non-nullable) data type for this field
                SimConnectDataType effectiveDataType;
                if (simVar.DataType.HasValue)
                {
                    effectiveDataType = simVar.DataType.Value;
                }
                else
                {
                    var ft = field.FieldType;

                    // Unwrap nullable
                    var nullableUnderlying = Nullable.GetUnderlyingType(ft);
                    if (nullableUnderlying != null)
                    {
                        ft = nullableUnderlying;
                    }

                    // Enums -> underlying integral type
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
                        _ when ft == typeof(string) => SimConnectDataType.String256, // default
                        _ => throw new NotSupportedException($"Cannot infer SimConnectDataType for field '{field.Name}' of type {ft.FullName}."),
                    };
                }

                // (Optional) add to SimConnect data definition in the same order
                addToDefinition?.Invoke(simVar.Name, simVar.Unit, effectiveDataType);

                // Decide read strategy and size for this datum using the effective type
                var (dataType, rawType, size) = Classify(field, effectiveDataType);

                // Create a closed SimVarFieldReader<T, TDest>
                var readerType = typeof(SimVarFieldReader<,>).MakeGenericType(t, field.FieldType);
                var reader = Activator.CreateInstance(readerType)!;

                // One-time dynamic assignment: clean and off the hot path (avoid dynamic for Setter to prevent binder issues)
                dynamic d = reader;
                d.OffsetBytes = offset;
                d.DataType = dataType;
                d.Size = size;
                d.Converter = BuildConverter(rawType, field.FieldType);

                // Build the concrete closed Setter<T,TDest> and assign via reflection to bypass dynamic binder conversion quirks.
                readerType.GetProperty("Setter")!.SetValue(reader, BuildSetter(t, field));

                readers.Add((IFieldReader<T>)reader);

                offset += size; // next datum starts here
            }

            return readers;
        }

        /// <summary>
        /// Builds a strongly-typed <c>Setter&lt;TStruct,TField&gt;</c> delegate instance and returns it as <see cref="Delegate"/>,
        /// allowing assignment to the strongly typed Setter property via reflection without dynamic binder conversion issues.
        /// </summary>
        /// <param name="structType">Struct type declaring the field.</param>
        /// <param name="fi">Field info.</param>
        /// <returns>The compiled field assignment delegate.</returns>
        public static Delegate BuildSetter(Type structType, FieldInfo fi)
        {
            var setterType = typeof(Setter<,>).MakeGenericType(structType, fi.FieldType);
            var tParam = Expression.Parameter(structType.MakeByRefType(), "t");
            var vParam = Expression.Parameter(fi.FieldType, "v");
            var assign = Expression.Assign(Expression.Field(tParam, fi), vParam);
            return Expression.Lambda(setterType, assign, tParam, vParam).Compile();
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

        /// <summary>
        /// Map a field + its SimVar type to: read kind, raw CLR type, and byte size.
        /// </summary>
        private static (SimConnectDataType DataType, Type RawType, int SizeBytes)
            Classify(FieldInfo field, SimConnectDataType dt)
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
                    throw new NotSupportedException(
                        $"{field.DeclaringType!.FullName}.{field.Name}: unsupported SimConnectDataType {dt}");
            }
        }

        /// <summary>
        /// Build a converter TRaw -> TDest (never null). Handles nullable, enums, numeric casts, identity.
        /// Always returns a compiled delegate so downstream code can invoke without null checks.
        /// </summary>
        private static Delegate BuildConverter(Type rawType, Type destType)
        {
            // Identity fast path
            if (rawType == destType)
            {
                var pId = Expression.Parameter(rawType, "r");
                var lambdaTypeId = typeof(Func<,>).MakeGenericType(rawType, destType);
                return Expression.Lambda(lambdaTypeId, pId, pId).Compile();
            }

            // Nullable<TInner>
            if (TryGetNullableUnderlying(destType, out var innerDest))
            {
                var innerConv = BuildConverter(rawType, innerDest); // recursive (non-null)
                var r = Expression.Parameter(rawType, "r");

                Expression innerValueExpr = rawType == innerDest
                    ? (Expression)r
                    : Expression.Invoke(Expression.Constant(innerConv), r);

                if (innerValueExpr.Type != innerDest)
                {
                    innerValueExpr = Expression.Convert(innerValueExpr, innerDest);
                }

                var ctor = destType.GetConstructor(new[] { innerDest })
                           ?? throw new InvalidOperationException($"Nullable<{innerDest.Name}> ctor missing.");

                var body = Expression.New(ctor, innerValueExpr);
                var lambdaType = typeof(Func<,>).MakeGenericType(rawType, destType);
                return Expression.Lambda(lambdaType, body, r).Compile();
            }

            // Enums
            if (destType.IsEnum)
            {
                var underlying = Enum.GetUnderlyingType(destType);
                var r = Expression.Parameter(rawType, "r");
                Expression toUnderlying = rawType == underlying ? (Expression)r : Expression.Convert(r, underlying);
                var toEnum = Expression.Convert(toUnderlying, destType);
                var lambdaType = typeof(Func<,>).MakeGenericType(rawType, destType);
                return Expression.Lambda(lambdaType, toEnum, r).Compile();
            }

            // General numeric / other convertible path (Expression.Convert handles checks)
            {
                var r = Expression.Parameter(rawType, "r");
                Expression body = Expression.Convert(r, destType);
                var lambdaType = typeof(Func<,>).MakeGenericType(rawType, destType);
                return Expression.Lambda(lambdaType, body, r).Compile();
            }
        }

        private static bool TryGetNullableUnderlying(Type t, out Type inner)
        {
            inner = Nullable.GetUnderlyingType(t) ?? typeof(void);
            return inner != typeof(void);
        }

        // Identity builder removed (BuildConverter now always returns a delegate)
    }
}
