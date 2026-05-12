// <copyright file="SimConnectNativeDispatcher.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimConnect.NET.Internal
{
    /// <summary>
    /// Serializes calls into the native SimConnect handle.
    /// </summary>
    internal sealed class SimConnectNativeDispatcher : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private bool disposed;

        /// <summary>
        /// Releases dispatcher resources.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.semaphore.Dispose();
        }

        /// <summary>
        /// Invokes a native operation synchronously.
        /// </summary>
        /// <typeparam name="T">The operation result type.</typeparam>
        /// <param name="operation">The operation to invoke.</param>
        /// <returns>The operation result.</returns>
        internal T Invoke<T>(Func<T> operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectNativeDispatcher));

            this.semaphore.Wait();
            try
            {
                ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectNativeDispatcher));
                return operation();
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        /// <summary>
        /// Invokes a native operation asynchronously.
        /// </summary>
        /// <param name="operation">The operation to invoke.</param>
        /// <param name="cancellationToken">Cancellation token for waiting to enter the dispatcher.</param>
        /// <returns>A task that represents the operation.</returns>
        internal async Task InvokeAsync(Action operation, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectNativeDispatcher));

            await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectNativeDispatcher));
                operation();
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        /// <summary>
        /// Invokes a native operation asynchronously.
        /// </summary>
        /// <typeparam name="T">The operation result type.</typeparam>
        /// <param name="operation">The operation to invoke.</param>
        /// <param name="cancellationToken">Cancellation token for waiting to enter the dispatcher.</param>
        /// <returns>A task containing the operation result.</returns>
        internal async Task<T> InvokeAsync<T>(Func<T> operation, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectNativeDispatcher));

            await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectNativeDispatcher));
                return operation();
            }
            finally
            {
                this.semaphore.Release();
            }
        }
    }
}
