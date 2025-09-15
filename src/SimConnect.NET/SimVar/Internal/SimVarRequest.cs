// <copyright file="SimVarRequest.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET;

namespace SimConnect.NET.SimVar.Internal
{
    /// <summary>
    /// Represents a pending SimVar request.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    internal sealed class SimVarRequest<T> : ISimVarRequest
    {
        private readonly TaskCompletionSource<T> taskCompletionSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimVarRequest{T}"/> class.
        /// </summary>
        /// <param name="requestId">The unique request identifier.</param>
        /// <param name="objectId">The object ID (usually SIMCONNECT_OBJECT_ID_USER).</param>
        /// <param name="period">The SimConnect period describing how often data should be sent.</param>
        /// <param name="onValue">Optional callback invoked for each received value when recurring.</param>
        /// <param name="definitionId">The data definition id used for this request.</param>
        public SimVarRequest(uint requestId, uint objectId, uint definitionId, SimConnectPeriod period, Action<T>? onValue)
        {
            this.RequestId = requestId;
            this.ObjectId = objectId;
            this.DefinitionId = definitionId;
            this.taskCompletionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously); // Prevent continuations from blocking SimConnect thread
            this.Period = period;
            this.OnValue = onValue;
        }

        /// <summary>
        /// Gets the unique request identifier.
        /// </summary>
        public uint RequestId { get; }

        /// <summary>
        /// Gets the object ID.
        /// </summary>
        public uint ObjectId { get; }

        /// <summary>
        /// Gets the data definition identifier.
        /// </summary>
        public uint DefinitionId { get; }

        /// <summary>
        /// Gets the <see cref="SimConnectPeriod"/> for this request.
        /// </summary>
        public SimConnectPeriod Period { get; }

        /// <summary>
        /// Gets a value indicating whether this is a recurring subscription.
        /// Recurring periods are any period other than <see cref="SimConnectPeriod.Never"/> or <see cref="SimConnectPeriod.Once"/>.
        /// </summary>
        public bool IsRecurring => this.Period != SimConnectPeriod.Never && this.Period != SimConnectPeriod.Once;

        /// <summary>
        /// Gets the callback invoked for each received value when recurring.
        /// </summary>
        public Action<T>? OnValue { get; }

        /// <summary>
        /// Gets the task that completes when the request is fulfilled.
        /// </summary>
        public Task<T> Task => this.taskCompletionSource.Task;

        /// <summary>
        /// Gets a non-generic completion task for interface consumption.
        /// </summary>
        Task ISimVarRequest.Completion => this.taskCompletionSource.Task;

        /// <summary>
        /// Completes the request with a successful result.
        /// </summary>
        /// <param name="result">The result value.</param>
        public void SetResult(T result)
        {
            if (this.IsRecurring)
            {
                var cb = this.OnValue;
                if (cb is not null)
                {
                    // Dispatch callback off-thread to avoid blocking the SimConnect message loop.
                    ThreadPool.UnsafeQueueUserWorkItem<(Action<T> Callback, T Value)>(
                        static state =>
                        {
                            try
                            {
                                // Invoke the user-provided callback with the received value.
                                state.Callback(state.Value);
                            }
                            catch
                            {
                                // Swallow exceptions from user callbacks to avoid breaking the message loop.
                                // User code errors should not affect SimConnect processing.
                            }
                        },
                        (cb, result),
                        preferLocal: true);
                }

                return;
            }

            // else, complete the task for one-time requests
            this.taskCompletionSource.TrySetResult(result);
        }

        /// <summary>
        /// Sets the result from a boxed value, converting to T as needed.
        /// </summary>
        /// <param name="value">The boxed value.</param>
        public void SetResultBoxed(object? value)
        {
            try
            {
                T converted;
                if (value is null)
                {
                    converted = default!;
                }
                else if (value is T tval)
                {
                    converted = tval;
                }
                else
                {
                    converted = (T)ConvertTo(typeof(T), value);
                }

                this.SetResult(converted!);
            }
            catch (Exception ex)
            {
                this.SetException(ex);
            }
        }

        /// <summary>
        /// Completes the request with an exception.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        public void SetException(Exception exception)
        {
            this.taskCompletionSource.TrySetException(exception);
        }

        /// <summary>
        /// Cancels the request.
        /// </summary>
        public void SetCanceled()
        {
            this.taskCompletionSource.TrySetCanceled();
        }

        /// <summary>
        /// Attempts to set a typed result without boxing. Returns true only when TValue matches T.
        /// </summary>
        /// <typeparam name="TValue">Provided value type.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <returns>True if accepted; otherwise false.</returns>
        public bool TrySetResult<TValue>(TValue value)
        {
            if (this is SimVarRequest<TValue> self)
            {
                self.SetResult(value);
                return true;
            }

            return false;
        }

        private static object ConvertTo(Type targetType, object value)
        {
            var sourceType = value.GetType();

            // Custom shims for common SimConnect conversions
            if (targetType == typeof(bool) && sourceType == typeof(int))
            {
                return (int)value != 0;
            }

            if (targetType == typeof(float) && sourceType == typeof(double))
            {
                return (float)(double)value;
            }

            // Enums from integers
            if (targetType.IsEnum && (sourceType == typeof(int) || sourceType == typeof(uint)))
            {
                return Enum.ToObject(targetType, value);
            }

            return Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
