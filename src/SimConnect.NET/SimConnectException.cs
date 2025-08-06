// <copyright file="SimConnectException.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents an exception that is thrown when a SimConnect operation fails.
    /// </summary>
    public class SimConnectException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public SimConnectException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorCode">The SimConnect error code.</param>
        public SimConnectException(string message, SimConnectError errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SimConnectException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorCode">The SimConnect error code.</param>
        /// <param name="innerException">The inner exception.</param>
        public SimConnectException(string message, SimConnectError errorCode, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets the SimConnect error code associated with this exception.
        /// </summary>
        public SimConnectError ErrorCode { get; }
    }
}
