// <copyright file="FacilityDefinitionBuilder.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace SimConnect.NET.Facilities
{
    /// <summary>
    /// Fluent builder for <see cref="FacilityDefinition"/> instances.
    /// </summary>
    public sealed class FacilityDefinitionBuilder
    {
        private readonly List<string> fields = new();

        /// <summary>
        /// Gets the configured field list.
        /// </summary>
        internal IReadOnlyList<string> Fields => this.fields;

        /// <summary>
        /// Adds a field path to the definition.
        /// </summary>
        /// <param name="fieldPath">The SimConnect facility field path (for example "Airport.Latitude").</param>
        /// <returns>The same builder instance to allow fluent chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldPath"/> is null or whitespace.</exception>
        public FacilityDefinitionBuilder AddField(string fieldPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldPath);
            this.fields.Add(fieldPath);
            return this;
        }
    }
}
