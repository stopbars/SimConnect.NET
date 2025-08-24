// <copyright file="SimConnectAttribute.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using SimConnect.NET.SimVar;

namespace SimConnect.NET
{
    /// <summary>Annotates a struct field with the SimVar you want marshalled into it.</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SimConnectAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectAttribute"/> class with name and unit.
        /// The data type is inferred from the SimVar registry if available.
        /// </summary>
        /// <param name="name">The SimVar name to marshal.</param>
        /// <param name="unit">The unit of the SimVar.</param>
        public SimConnectAttribute(string name, string unit)
        {
            this.Name = name;
            this.Unit = unit;
            var simVar = SimVarRegistry.Get(name);
            if (simVar != null)
            {
                this.DataType = simVar.DataType;
            }
            else
            {
                throw new ArgumentException($"SimVar '{name}' not found in registry. Please specify unit and dataType explicitly.", nameof(name));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectAttribute"/> class using the SimVar name.
        /// The unit and data type are inferred from the SimVar registry if available.
        /// </summary>
        /// <param name="name">The SimVar name to marshal.</param>
        public SimConnectAttribute(string name)
        {
            this.Name = name;
            var simVar = SimVarRegistry.Get(name);
            if (simVar != null)
            {
                this.Unit = simVar.Unit;
                this.DataType = simVar.DataType;
            }
            else
            {
                throw new ArgumentException($"SimVar '{name}' not found in registry. Please specify unit and dataType explicitly.", nameof(name));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectAttribute"/> class.
        /// </summary>
        /// <param name="name">The SimVar name to marshal.</param>
        /// <param name="unit">The unit of the SimVar.</param>
        /// <param name="dataType">The SimConnect data type for marshaling.</param>
        public SimConnectAttribute(string name, string unit, SimConnectDataType dataType)
        {
            this.Name = name;
            this.Unit = unit;
            this.DataType = dataType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectAttribute"/> class.
        /// </summary>
        /// <param name="name">The SimVar name to marshal.</param>
        /// <param name="unit">The unit of the SimVar.</param>
        /// <param name="dataType">The SimConnect data type for marshaling.</param>
        /// <param name="order">The order in which the SimVar should be marshaled.</param>
        public SimConnectAttribute(string name, string? unit, SimConnectDataType dataType, int order)
        {
            this.Name = name;
            this.Unit = unit;
            this.DataType = dataType;
            this.Order = order;
        }

        /// <summary>
        /// Gets the SimVar name to marshal.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unit of the SimVar.
        /// </summary>
        public string? Unit { get; }

        /// <summary>
        /// Gets the SimConnect data type for marshaling.
        /// </summary>
        public SimConnectDataType DataType { get; }

        /// <summary>
        /// Gets the order in which the SimVar should be marshaled.
        /// </summary>
        public int Order { get; }
    }
}
