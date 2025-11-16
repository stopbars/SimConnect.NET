// <copyright file="FacilityDefinition.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace SimConnect.NET.Facilities
{
    /// <summary>
    /// Represents a facility data definition registered with SimConnect.
    /// </summary>
    public sealed class FacilityDefinition
    {
        internal FacilityDefinition(uint definitionId, IReadOnlyList<string> fields, Type? structType)
        {
            this.DefinitionId = definitionId;
            this.Fields = fields;
            this.StructType = structType;
        }

        /// <summary>
        /// Gets the SimConnect definition identifier.
        /// </summary>
        public uint DefinitionId { get; }

        /// <summary>
        /// Gets the list of facility fields that are part of this definition.
        /// </summary>
        public IReadOnlyList<string> Fields { get; }

        /// <summary>
        /// Gets the struct type used to generate this definition, if any.
        /// </summary>
        public Type? StructType { get; }
    }
}
