// <copyright file="SimObject.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET.AI
{
    /// <summary>
    /// Represents a simulation object managed by SimConnect.
    /// </summary>
    public class SimObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimObject"/> class.
        /// </summary>
        /// <param name="objectId">The unique identifier assigned by SimConnect.</param>
        /// <param name="containerTitle">The container title used to create the object.</param>
        /// <param name="requestId">The request ID used when creating the object.</param>
        /// <param name="position">The initial position of the object.</param>
        public SimObject(uint objectId, string containerTitle, uint requestId, SimConnectDataInitPosition position)
        {
            this.ObjectId = objectId;
            this.ContainerTitle = containerTitle;
            this.RequestId = requestId;
            this.InitialPosition = position;
            this.CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the unique identifier assigned by SimConnect when the object was created.
        /// </summary>
        public uint ObjectId { get; }

        /// <summary>
        /// Gets the container title used to create this object.
        /// </summary>
        public string ContainerTitle { get; }

        /// <summary>
        /// Gets the request ID used when creating this object.
        /// </summary>
        public uint RequestId { get; }

        /// <summary>
        /// Gets the initial position where the object was created.
        /// </summary>
        public SimConnectDataInitPosition InitialPosition { get; }

        /// <summary>
        /// Gets the UTC timestamp when this object was created.
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Gets a value indicating whether this object is still active in the simulation.
        /// </summary>
        public bool IsActive { get; internal set; } = true;

        /// <summary>
        /// Gets or sets optional user data associated with this object.
        /// </summary>
        public object? UserData { get; set; }

        /// <summary>
        /// Returns a string representation of this simulation object.
        /// </summary>
        /// <returns>A string containing the object ID and container title.</returns>
        public override string ToString()
        {
            return $"SimObject(ID: {this.ObjectId}, Container: {this.ContainerTitle}, Active: {this.IsActive})";
        }
    }
}
