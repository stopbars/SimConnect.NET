// <copyright file="SimConnectControllerItem.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectControllerItem struct contains data related to a single controller currently connected to the simulation.
    /// </summary>
    public struct SimConnectControllerItem
    {
        /// <summary>
        /// Gets or sets a string that gives the descriptive name for the device.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the device ID.
        /// </summary>
        public uint DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        public uint ProductId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the USB composite device (for when devices have the same ProductId, but there are multiple recognized parts on the same device).
        /// </summary>
        public uint CompositeId { get; set; }

        /// <summary>
        /// Gets or sets the version data for the hardware.
        /// </summary>
        public SimConnectVersionBaseType HardwareVersion { get; set; }
    }
}
