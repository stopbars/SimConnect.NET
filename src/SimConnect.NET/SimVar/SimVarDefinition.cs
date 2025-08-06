// <copyright file="SimVarDefinition.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.SimVar
{
    /// <summary>
    /// Represents a SimVar definition with metadata about its properties.
    /// </summary>
    public sealed class SimVarDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimVarDefinition"/> class.
        /// </summary>
        /// <param name="name">The SimVar name (e.g., "PLANE LATITUDE").</param>
        /// <param name="unit">The unit of measurement (e.g., "degrees").</param>
        /// <param name="dataType">The data type for SimConnect.</param>
        /// <param name="isSettable">Whether this SimVar can be set.</param>
        /// <param name="description">Optional description of the SimVar.</param>
        public SimVarDefinition(string name, string unit, SimConnectDataType dataType, bool isSettable, string? description = null)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            this.DataType = dataType;
            this.IsSettable = isSettable;
            this.Description = description;
        }

        /// <summary>
        /// Gets the SimVar name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unit of measurement.
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// Gets the SimConnect data type.
        /// </summary>
        public SimConnectDataType DataType { get; }

        /// <summary>
        /// Gets a value indicating whether this SimVar can be set.
        /// </summary>
        public bool IsSettable { get; }

        /// <summary>
        /// Gets the optional description.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Gets the .NET type that corresponds to the SimConnect data type.
        /// </summary>
        public Type NetType => this.DataType switch
        {
            SimConnectDataType.Integer32 => typeof(int),
            SimConnectDataType.Integer64 => typeof(long),
            SimConnectDataType.FloatSingle => typeof(float),
            SimConnectDataType.FloatDouble => typeof(double),
            SimConnectDataType.String8 or
            SimConnectDataType.String32 or
            SimConnectDataType.String64 or
            SimConnectDataType.String128 or
            SimConnectDataType.String256 or
            SimConnectDataType.String260 or
            SimConnectDataType.StringV => typeof(string),
            SimConnectDataType.LatLonAlt => typeof(SimConnectDataLatLonAlt),
            SimConnectDataType.Xyz => typeof(SimConnectDataXyz),
            _ => typeof(object),
        };
    }
}
