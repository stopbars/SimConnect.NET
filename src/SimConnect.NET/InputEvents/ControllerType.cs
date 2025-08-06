// <copyright file="ControllerType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Defines the types of input controllers/devices.
    /// </summary>
    public enum ControllerType
    {
        /// <summary>
        /// Unknown or unspecified controller type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Keyboard device.
        /// </summary>
        Keyboard = 1,

        /// <summary>
        /// Mouse device.
        /// </summary>
        Mouse = 2,

        /// <summary>
        /// Joystick or gamepad device.
        /// </summary>
        Joystick = 3,

        /// <summary>
        /// Flight yoke device.
        /// </summary>
        Yoke = 4,

        /// <summary>
        /// Rudder pedals device.
        /// </summary>
        RudderPedals = 5,

        /// <summary>
        /// Throttle quadrant device.
        /// </summary>
        ThrottleQuadrant = 6,

        /// <summary>
        /// Other/custom input device.
        /// </summary>
        Other = 99,
    }
}
