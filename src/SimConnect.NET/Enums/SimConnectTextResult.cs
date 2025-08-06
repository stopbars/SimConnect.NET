// <copyright file="SimConnectTextResult.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// Specifies the result of a call to SimConnect_Text.
    /// </summary>
    public enum SimConnectTextResult
    {
        /// <summary>
        /// Menu selection 1.
        /// </summary>
        MenuSelect1,

        /// <summary>
        /// Menu selection 2.
        /// </summary>
        MenuSelect2,

        /// <summary>
        /// Menu selection 3.
        /// </summary>
        MenuSelect3,

        /// <summary>
        /// Menu selection 4.
        /// </summary>
        MenuSelect4,

        /// <summary>
        /// Menu selection 5.
        /// </summary>
        MenuSelect5,

        /// <summary>
        /// Menu selection 6.
        /// </summary>
        MenuSelect6,

        /// <summary>
        /// Menu selection 7.
        /// </summary>
        MenuSelect7,

        /// <summary>
        /// Menu selection 8.
        /// </summary>
        MenuSelect8,

        /// <summary>
        /// Menu selection 9.
        /// </summary>
        MenuSelect9,

        /// <summary>
        /// Menu selection 10.
        /// </summary>
        MenuSelect10,

        /// <summary>
        /// The text has been queued.
        /// </summary>
        Queued,

        /// <summary>
        /// The text has been removed.
        /// </summary>
        Removed,

        /// <summary>
        /// The text has been replaced.
        /// </summary>
        Replaced,

        /// <summary>
        /// The text has timed out.
        /// </summary>
        Timeout,

        /// <summary>
        /// The text has been displayed.
        /// </summary>
        Displayed = 0x00010000,
    }
}
