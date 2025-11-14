// <copyright file="ExceptionHelper.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;

namespace SimConnect.NET.Internal
{
    /// <summary>
    /// Provides helpers for determining whether an exception is critical enough to rethrow.
    /// </summary>
    internal static class ExceptionHelper
    {
        /// <summary>
        /// Determines whether the supplied exception is considered critical and should not be swallowed.
        /// </summary>
        /// <param name="exception">The exception to inspect.</param>
        /// <returns><c>true</c> if the exception is critical; otherwise, <c>false</c>.</returns>
        public static bool IsCritical(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception is OutOfMemoryException
                or StackOverflowException
                or ThreadAbortException
                or AccessViolationException
                or AppDomainUnloadedException
                or CannotUnloadAppDomainException
                or BadImageFormatException;
        }
    }
}
