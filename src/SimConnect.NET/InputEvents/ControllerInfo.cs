// <copyright file="ControllerInfo.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Represents information about a connected controller/input device.
    /// </summary>
    public sealed class ControllerInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerInfo"/> class.
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        /// <param name="deviceId">The unique identifier of the device.</param>
        /// <param name="productId">The product ID of the device.</param>
        /// <param name="deviceType">The type of the device.</param>
        public ControllerInfo(string deviceName, uint deviceId, uint productId, ControllerType deviceType)
        {
            this.DeviceName = deviceName ?? throw new ArgumentNullException(nameof(deviceName));
            this.DeviceId = deviceId;
            this.ProductId = productId;
            this.DeviceType = deviceType;
        }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public string DeviceName { get; }

        /// <summary>
        /// Gets the unique identifier of the device.
        /// </summary>
        public uint DeviceId { get; }

        /// <summary>
        /// Gets the product ID of the device.
        /// </summary>
        public uint ProductId { get; }

        /// <summary>
        /// Gets the type of the device.
        /// </summary>
        public ControllerType DeviceType { get; }

        /// <summary>
        /// Returns a string representation of the controller information.
        /// </summary>
        /// <returns>A string representation of the controller.</returns>
        public override string ToString()
        {
            return $"{this.DeviceName} (ID: {this.DeviceId}, Type: {this.DeviceType}, Product: {this.ProductId})";
        }
    }
}
