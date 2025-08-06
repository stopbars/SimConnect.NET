// <copyright file="InputGroup.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Represents an input group for organizing and managing input events.
    /// </summary>
    public sealed class InputGroup
    {
        private readonly ConcurrentDictionary<string, InputEventMapping> eventMappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGroup"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the input group.</param>
        /// <param name="name">The name of the input group.</param>
        /// <param name="priority">The priority of the input group.</param>
        internal InputGroup(uint id, string name, InputGroupPriority priority)
        {
            this.Id = id;
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Priority = priority;
            this.IsEnabled = true;
            this.eventMappings = new ConcurrentDictionary<string, InputEventMapping>();
        }

        /// <summary>
        /// Gets the unique identifier for the input group.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Gets the name of the input group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the priority of the input group.
        /// </summary>
        public InputGroupPriority Priority { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the input group is enabled.
        /// </summary>
        public bool IsEnabled { get; internal set; }

        /// <summary>
        /// Gets the event mappings in this input group.
        /// </summary>
        public IReadOnlyDictionary<string, InputEventMapping> EventMappings => this.eventMappings;

        /// <summary>
        /// Returns a string representation of the input group.
        /// </summary>
        /// <returns>A string representation of the input group.</returns>
        public override string ToString()
        {
            return $"{this.Name} (ID: {this.Id}, Priority: {this.Priority}, Enabled: {this.IsEnabled}, Events: {this.eventMappings.Count})";
        }

        /// <summary>
        /// Adds an input event mapping to this group.
        /// </summary>
        /// <param name="inputDefinition">The input definition.</param>
        /// <param name="mapping">The event mapping.</param>
        internal void AddEventMapping(string inputDefinition, InputEventMapping mapping)
        {
            this.eventMappings[inputDefinition] = mapping;
        }

        /// <summary>
        /// Removes an input event from this group.
        /// </summary>
        /// <param name="inputDefinition">The input definition to remove.</param>
        internal void RemoveEvent(string inputDefinition)
        {
            this.eventMappings.TryRemove(inputDefinition, out _);
        }

        /// <summary>
        /// Clears all event mappings from this group.
        /// </summary>
        internal void ClearEvents()
        {
            this.eventMappings.Clear();
        }
    }
}
