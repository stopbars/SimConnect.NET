// <copyright file="SimSystemEventObjectAddRemoveEventArgs.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using SimConnect.NET;

namespace SimConnect.NET.Events
{
    /// <summary>
    /// Provides data for system events that report AI object creation or removal.
    /// </summary>
    /// <param name="objectType">The type of the object that was added or removed.</param>
    public class SimSystemEventObjectAddRemoveEventArgs(SimConnectSimObjectType objectType) : EventArgs
    {
        /// <summary>
        /// Gets the type of the object that was added or removed.
        /// </summary>
        public SimConnectSimObjectType ObjectType { get; } = objectType;
    }
}
