// <copyright file="InputGroupManager.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Manages input groups for organizing and prioritizing input events.
    /// </summary>
    public sealed class InputGroupManager : IDisposable
    {
        private readonly IntPtr simConnectHandle;
        private readonly ConcurrentDictionary<uint, InputGroup> inputGroups;
        private readonly object lockObject = new object();
        private uint nextGroupId = 1;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputGroupManager"/> class.
        /// </summary>
        /// <param name="simConnectHandle">The SimConnect handle.</param>
        internal InputGroupManager(IntPtr simConnectHandle)
        {
            this.simConnectHandle = simConnectHandle;
            this.inputGroups = new ConcurrentDictionary<uint, InputGroup>();
        }

        /// <summary>
        /// Creates a new input group.
        /// </summary>
        /// <param name="name">The name of the input group.</param>
        /// <param name="priority">The priority of the input group.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous creation operation, returning the input group.</returns>
        /// <exception cref="SimConnectException">Thrown when the creation fails.</exception>
        public async Task<InputGroup> CreateInputGroupAsync(string name, InputGroupPriority priority = InputGroupPriority.Default, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputGroupManager));
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(name);

            var groupId = this.GetNextGroupId();
            var inputGroup = new InputGroup(groupId, name, priority);

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_SetInputGroupPriority(this.simConnectHandle, groupId, (uint)priority);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to set input group priority: {(SimConnectError)result}", (SimConnectError)result);
                }
            },
                cancellationToken).ConfigureAwait(false);

            this.inputGroups[groupId] = inputGroup;
            return inputGroup;
        }

        /// <summary>
        /// Sets the priority of an input group.
        /// </summary>
        /// <param name="groupId">The ID of the input group.</param>
        /// <param name="priority">The new priority.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the operation fails.</exception>
        public async Task SetInputGroupPriorityAsync(uint groupId, InputGroupPriority priority, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputGroupManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_SetInputGroupPriority(this.simConnectHandle, groupId, (uint)priority);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to set input group priority: {(SimConnectError)result}", (SimConnectError)result);
                }

                if (this.inputGroups.TryGetValue(groupId, out var group))
                {
                    group.Priority = priority;
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the state of an input group (enabled/disabled).
        /// </summary>
        /// <param name="groupId">The ID of the input group.</param>
        /// <param name="enabled">True to enable the group, false to disable.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the operation fails.</exception>
        public async Task SetInputGroupStateAsync(uint groupId, bool enabled, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputGroupManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_SetInputGroupState(this.simConnectHandle, groupId, enabled ? 1u : 0u);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to set input group state: {(SimConnectError)result}", (SimConnectError)result);
                }

                if (this.inputGroups.TryGetValue(groupId, out var group))
                {
                    group.IsEnabled = enabled;
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears all input events from an input group.
        /// </summary>
        /// <param name="groupId">The ID of the input group.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the operation fails.</exception>
        public async Task ClearInputGroupAsync(uint groupId, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputGroupManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_ClearInputGroup(this.simConnectHandle, groupId);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to clear input group: {(SimConnectError)result}", (SimConnectError)result);
                }

                if (this.inputGroups.TryGetValue(groupId, out var group))
                {
                    group.ClearEvents();
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes an input event from an input group.
        /// </summary>
        /// <param name="groupId">The ID of the input group.</param>
        /// <param name="inputDefinition">The input definition to remove.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the operation fails.</exception>
        public async Task RemoveInputEventAsync(uint groupId, string inputDefinition, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputGroupManager));
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(inputDefinition);

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_RemoveInputEvent(this.simConnectHandle, groupId, inputDefinition);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to remove input event: {(SimConnectError)result}", (SimConnectError)result);
                }

                if (this.inputGroups.TryGetValue(groupId, out var group))
                {
                    group.RemoveEvent(inputDefinition);
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an input group by its ID.
        /// </summary>
        /// <param name="groupId">The ID of the input group.</param>
        /// <returns>The input group, or null if not found.</returns>
        public InputGroup? GetInputGroup(uint groupId)
        {
            return this.inputGroups.TryGetValue(groupId, out var group) ? group : null;
        }

        /// <summary>
        /// Disposes the InputGroupManager and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.inputGroups.Clear();
                this.disposed = true;
            }
        }

        private uint GetNextGroupId()
        {
            lock (this.lockObject)
            {
                return this.nextGroupId++;
            }
        }
    }
}
