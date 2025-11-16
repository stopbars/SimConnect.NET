// <copyright file="FacilityFieldAttribute.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Facilities
{
    /// <summary>
    /// Annotates a struct field with a SimConnect facility field path for automatic definition creation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class FacilityFieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacilityFieldAttribute"/> class.
        /// </summary>
        /// <param name="path">The SimConnect facility field path.</param>
        /// <param name="order">Optional explicit ordering (lower numbers are processed first).</param>
        public FacilityFieldAttribute(string path, int order = 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            this.Path = path;
            this.Order = order;
        }

        /// <summary>
        /// Gets the SimConnect facility field path represented by this attribute.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the optional ordering index. Lower values are processed first.
        /// </summary>
        public int Order { get; }
    }
}
