// <copyright file="SimConnectInputEventType.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

namespace SimConnect.NET
{
    /// <summary>
    /// The SimConnectInputEventType enumeration type is used with the SimConnect_GetInputEvent call
    /// to specify the data type used and help you cast the return value correctly.
    /// </summary>
    public enum SimConnectInputEventType
    {
        /// <summary>
        /// No data type specification required (C++ only).
        /// </summary>
        None,

        /// <summary>
        /// Specifies a double value.
        /// </summary>
        DoubleValue,

        /// <summary>
        /// Specifies a string value.
        /// </summary>
        StringValue,
    }
}
