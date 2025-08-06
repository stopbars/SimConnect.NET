// <copyright file="SimObjectManager.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Diagnostics;

namespace SimConnect.NET.AI
{
    /// <summary>
    /// Manages creation, tracking, and removal of AI simulation objects.
    /// Provides a high-level interface for spawning and managing objects in the simulation.
    /// </summary>
    public class SimObjectManager : IDisposable
    {
        private readonly SimConnectClient client;
        private readonly ConcurrentDictionary<uint, SimObject> managedObjects = new();
        private readonly ConcurrentDictionary<uint, TaskCompletionSource<SimObject>> pendingCreations = new();
        private uint nextRequestId = 50000; // Start at 50000 to avoid conflicts with SimVarManager
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimObjectManager"/> class.
        /// </summary>
        /// <param name="client">The SimConnect client instance.</param>
        public SimObjectManager(SimConnectClient client)
        {
            ArgumentNullException.ThrowIfNull(client);
            this.client = client;
        }

        /// <summary>
        /// Gets all currently managed objects.
        /// </summary>
        public IReadOnlyDictionary<uint, SimObject> ManagedObjects => this.managedObjects;

        /// <summary>
        /// Gets the count of active objects being managed.
        /// </summary>
        public int ActiveObjectCount => this.managedObjects.Count(kvp => kvp.Value.IsActive);

        /// <summary>
        /// Creates a new AI simulation object asynchronously.
        /// </summary>
        /// <param name="containerTitle">The container title (case-sensitive) from the sim.cfg file.</param>
        /// <param name="position">The initial position and orientation of the object.</param>
        /// <param name="userData">Optional user data to associate with the object.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created SimObject.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
        /// <exception cref="SimConnectException">Thrown when object creation fails.</exception>
        /// <exception cref="ArgumentException">Thrown when containerTitle is null or empty.</exception>
        public async Task<SimObject> CreateObjectAsync(
            string containerTitle,
            SimConnectDataInitPosition position,
            object? userData = null,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimObjectManager));
            ArgumentException.ThrowIfNullOrEmpty(containerTitle);

            cancellationToken.ThrowIfCancellationRequested();

            var requestId = Interlocked.Increment(ref this.nextRequestId);
            var tcs = new TaskCompletionSource<SimObject>();

            // Store the pending creation request
            this.pendingCreations[requestId] = tcs;

            try
            {
                var result = SimConnectNative.SimConnect_AICreateSimulatedObject(
                    this.client.Handle,
                    containerTitle,
                    position,
                    requestId);

                if (result != (int)SimConnectError.None)
                {
                    this.pendingCreations.TryRemove(requestId, out _);
                    throw new SimConnectException(
                        $"Failed to create AI object '{containerTitle}': {(SimConnectError)result}",
                        (SimConnectError)result);
                }

                Debug.WriteLine($"SimObjectManager: Requested creation of '{containerTitle}' with requestId {requestId}");

                // Wait for the object creation to complete with shorter timeout
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10)); // Reduced from 30 to 10 seconds

                var createdObject = await tcs.Task.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
                createdObject.UserData = userData;

                Debug.WriteLine($"SimObjectManager: Successfully created object {createdObject}");
                return createdObject;
            }
            catch (OperationCanceledException)
            {
                this.pendingCreations.TryRemove(requestId, out _);
                throw;
            }
            catch (Exception ex) when (ex is not SimConnectException)
            {
                this.pendingCreations.TryRemove(requestId, out _);
                throw new SimConnectException($"Unexpected error creating AI object '{containerTitle}': {ex.Message}", SimConnectError.Error, ex);
            }
        }

        /// <summary>
        /// Removes an AI simulation object asynchronously.
        /// </summary>
        /// <param name="simObject">The simulation object to remove.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the manager has been disposed.</exception>
        /// <exception cref="SimConnectException">Thrown when object removal fails.</exception>
        /// <exception cref="ArgumentNullException">Thrown when simObject is null.</exception>
        public Task RemoveObjectAsync(SimObject simObject, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimObjectManager));
            ArgumentNullException.ThrowIfNull(simObject);

            if (!simObject.IsActive)
            {
                Debug.WriteLine($"SimObjectManager: Object {simObject.ObjectId} is already inactive");
                return Task.CompletedTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var requestId = Interlocked.Increment(ref this.nextRequestId);
            var result = SimConnectNative.SimConnect_AIRemoveObject(
                this.client.Handle,
                simObject.ObjectId,
                requestId);

            if (result != (int)SimConnectError.None)
            {
                throw new SimConnectException(
                    $"Failed to remove AI object {simObject.ObjectId}: {(SimConnectError)result}",
                    (SimConnectError)result);
            }

            // Mark object as inactive and remove from tracking
            simObject.IsActive = false;
            this.managedObjects.TryRemove(simObject.ObjectId, out _);

            Debug.WriteLine($"SimObjectManager: Removed object {simObject}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes an AI simulation object by its object ID.
        /// </summary>
        /// <param name="objectId">The object ID to remove.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation. Returns true if the object was found and removed.</returns>
        public async Task<bool> RemoveObjectAsync(uint objectId, CancellationToken cancellationToken = default)
        {
            if (this.managedObjects.TryGetValue(objectId, out var simObject))
            {
                await this.RemoveObjectAsync(simObject, cancellationToken).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all managed AI simulation objects.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RemoveAllObjectsAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimObjectManager));

            var objects = this.managedObjects.Values.Where(obj => obj.IsActive).ToList();

            Debug.WriteLine($"SimObjectManager: Removing {objects.Count} active objects");

            var tasks = objects.Select(obj => this.RemoveObjectAsync(obj, cancellationToken));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a managed object by its object ID.
        /// </summary>
        /// <param name="objectId">The object ID to search for.</param>
        /// <returns>The SimObject if found; otherwise null.</returns>
        public SimObject? GetObject(uint objectId)
        {
            this.managedObjects.TryGetValue(objectId, out var simObject);
            return simObject;
        }

        /// <summary>
        /// Gets all managed objects of a specific container title.
        /// </summary>
        /// <param name="containerTitle">The container title to filter by.</param>
        /// <returns>An enumerable of matching SimObjects.</returns>
        public IEnumerable<SimObject> GetObjectsByType(string containerTitle)
        {
            ArgumentException.ThrowIfNullOrEmpty(containerTitle);

            return this.managedObjects.Values.Where(obj =>
                obj.IsActive &&
                string.Equals(obj.ContainerTitle, containerTitle, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Processes a received assigned object ID message from SimConnect.
        /// This should be called by the SimConnect client when it receives object creation confirmations.
        /// </summary>
        /// <param name="requestId">The request ID from the original creation call.</param>
        /// <param name="objectId">The assigned object ID from SimConnect.</param>
        /// <param name="containerTitle">The container title that was used.</param>
        /// <param name="position">The position where the object was created.</param>
        public void ProcessObjectCreated(uint requestId, uint objectId, string containerTitle, SimConnectDataInitPosition position)
        {
            if (this.pendingCreations.TryRemove(requestId, out var tcs))
            {
                var simObject = new SimObject(objectId, containerTitle, requestId, position);
                this.managedObjects[objectId] = simObject;
                tcs.SetResult(simObject);

                Debug.WriteLine($"SimObjectManager: Object creation completed - {simObject}");
            }
            else
            {
                Debug.WriteLine($"SimObjectManager: Received unexpected object creation for requestId {requestId}");
            }
        }

        /// <summary>
        /// Processes object creation failure.
        /// </summary>
        /// <param name="requestId">The request ID that failed.</param>
        /// <param name="error">The error that occurred.</param>
        public void ProcessObjectCreationFailed(uint requestId, SimConnectError error)
        {
            if (this.pendingCreations.TryRemove(requestId, out var tcs))
            {
                tcs.SetException(new SimConnectException($"Object creation failed: {error}", error));
                Debug.WriteLine($"SimObjectManager: Object creation failed for requestId {requestId}: {error}");
            }
        }

        /// <summary>
        /// Releases all managed objects and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            try
            {
                // Cancel all pending operations
                foreach (var tcs in this.pendingCreations.Values)
                {
                    tcs.TrySetCanceled();
                }

                this.pendingCreations.Clear();

                // Mark all objects as inactive (don't remove from sim since we're disposing)
                foreach (var obj in this.managedObjects.Values)
                {
                    obj.IsActive = false;
                }

                this.managedObjects.Clear();

                Debug.WriteLine("SimObjectManager: Disposed");
            }
            finally
            {
                this.disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
