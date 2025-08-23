// <copyright file="SimConnectInputEventType.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
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
        /// Specifies a double value.
        /// </summary>
        DoubleValue,

        /// <summary>
        /// Specifies a string value.
        /// </summary>
        StringValue,
    }
}
