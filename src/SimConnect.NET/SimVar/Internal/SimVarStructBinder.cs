// <copyright file="SimVarStructBinder.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Reflection;

namespace SimConnect.NET.SimVar.Internal
{
    internal static class SimVarStructBinder
    {
        /// <summary>
        /// Returns the ordered [SimVar]-annotated fields for T, validating .NET types vs SimConnect types.
        /// </summary>
    internal static (System.Reflection.FieldInfo Field, SimConnectAttribute Attr)[] GetOrderedFields<T>()
    {
            var t = typeof(T);
            if (!t.IsLayoutSequential)
            {
                throw new InvalidOperationException($"{t.Name} must be annotated with [StructLayout(LayoutKind.Sequential)].");
            }

            var fields = t.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                  .Select(f => (Field: f, Attr: f.GetCustomAttribute<SimConnectAttribute>()))
                  .Where(x => x.Attr != null)
                  .OrderBy(x => x!.Attr!.Order)
                  .ThenBy(x => x.Field.MetadataToken)
                  .ToArray();

            if (fields.Length == 0)
            {
                throw new InvalidOperationException($"{t.Name} has no fields annotated with [SimVar].");
            }

            foreach (var (field, attr) in fields)
            {
                var ft = field.FieldType;
                switch (attr!.DataType)
                {
                    case SimConnectDataType.FloatDouble:
                        if (ft != typeof(double))
                        {
                            throw Fail(field, "double");
                        }

                        break;
                    case SimConnectDataType.FloatSingle:
                        if (ft != typeof(float))
                        {
                            throw Fail(field, "float");
                        }

                        break;
                    case SimConnectDataType.Integer32:
                        if (ft != typeof(int) && ft != typeof(uint))
                        {
                            throw Fail(field, "int/uint");
                        }

                        break;
                    case SimConnectDataType.Integer64:
                        if (ft != typeof(long) && ft != typeof(ulong))
                        {
                            throw Fail(field, "long/ulong");
                        }

                        break;
                    case SimConnectDataType.String8:
                    case SimConnectDataType.String32:
                    case SimConnectDataType.String64:
                    case SimConnectDataType.String128:
                    case SimConnectDataType.String256:
                    case SimConnectDataType.String260:
                        if (ft != typeof(string))
                        {
                            throw Fail(field, "string");
                        }

                        break;
                }
            }

            return fields!;

            static InvalidOperationException Fail(FieldInfo f, string expected)
                => new($"Field {f.DeclaringType!.Name}.{f.Name} must be {expected} to match its [SimVar] attribute.");
        }

    /// <summary>
    /// Builds a single SimConnect data definition for T (using [SimVar] attributes),
    /// registers T for marshalling, and returns the definition ID.
    /// </summary>
    /// <param name="handle">Native SimConnect handle.</param>
    internal static (uint DefId, (System.Reflection.FieldInfo Field, SimConnectAttribute Attr)[] Fields) BuildAndRegisterFromStruct<T>(IntPtr handle)
        {
            var t = typeof(T);
            if (!t.IsLayoutSequential)
            {
                throw new InvalidOperationException($"{t.Name} must be annotated with [StructLayout(LayoutKind.Sequential)].");
            }

            var fields = GetOrderedFields<T>();

            uint defId = unchecked((uint)Guid.NewGuid().GetHashCode());

            var result = SimConnectNative.SimConnect_ClearDataDefinition(handle, defId);
            if (result != 0)
            {
                throw new InvalidOperationException($"Failed to clear data definition for {t.Name}: {result}");
            }

            foreach (var (field, attr) in fields)
            {
                // Add each SimVar field to the SimConnect data definition using the native layer
                if (attr == null)
                {
                    throw new InvalidOperationException($"Field {field.Name} is missing [SimVar] attribute.");
                }

                result = SimConnectNative.SimConnect_AddToDataDefinition(
                    handle,
                    defId,
                    attr.Name,
                    attr.Unit ?? string.Empty,
                    (uint)attr.DataType);

                if (result != 0)
                {
                    throw new InvalidOperationException($"Failed to add data definition for {field.Name}: {result}");
                }
            }

            var size = SimVarDataTypeSizing.GetPayloadSizeBytes(fields.Select(f => f.Attr!.DataType));
            var offsets = SimVarDataTypeSizing.ComputeOffsets(fields.Select(f => f.Attr!.DataType));
            return (defId, fields);
        }
    }
}
