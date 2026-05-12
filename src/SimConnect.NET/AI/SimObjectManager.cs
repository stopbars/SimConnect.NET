// <copyright file="SimObjectManager.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Reflection;
using SimConnect.NET.SimVar;

namespace SimConnect.NET.AI
{
    /// <summary>
    /// Manages creation, tracking, and removal of AI simulation objects.
    /// Provides a high-level interface for spawning and managing objects in the simulation.
    /// </summary>
    public class SimObjectManager : IDisposable
    {
        private static readonly MethodInfo SetAsyncByNameMethod = typeof(SimVarManager)
            .GetMethods()
            .Single(method =>
                method.Name == nameof(SimVarManager.SetAsync) &&
                method.IsGenericMethodDefinition &&
                method.GetParameters() is { Length: 5 } parameters &&
                parameters[0].ParameterType == typeof(string));

        private readonly SimConnectClient client;
        private readonly ConcurrentDictionary<uint, SimObject> managedObjects = new();
        private readonly ConcurrentDictionary<uint, PendingObjectCreation> pendingCreations = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<uint, SimObject>> objectsByType = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<Type, MethodInfo> setAsyncMethodCache = new();
        private int activeObjectCount;
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
        public int ActiveObjectCount => Volatile.Read(ref this.activeObjectCount);

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
            var pendingCreation = new PendingObjectCreation(containerTitle, position);

            // Store the pending creation request
            this.pendingCreations[requestId] = pendingCreation;

            try
            {
                var result = await this.client.InvokeNativeAsync(
                    handle => SimConnectNative.SimConnect_AICreateSimulatedObject(
                        handle,
                        containerTitle,
                        position,
                        requestId),
                    cancellationToken).ConfigureAwait(false);

                if (result != (int)SimConnectError.None)
                {
                    this.pendingCreations.TryRemove(requestId, out _);
                    throw SimConnectErrorMapper.Wrap($"Create AI object '{containerTitle}'", result);
                }

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"SimObjectManager: Requested creation of '{containerTitle}' with requestId {requestId}");
                }

                // Wait for the object creation to complete with shorter timeout
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(10)); // Reduced from 30 to 10 seconds

                var createdObject = await pendingCreation.Completion.Task.WaitAsync(timeoutCts.Token).ConfigureAwait(false);
                createdObject.UserData = userData;

                SimConnectLogger.Info($"SimObjectManager: Successfully created object {createdObject}");
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
                throw SimConnectErrorMapper.Wrap($"Create AI object '{containerTitle}'", SimConnectError.Error, ex);
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
        public async Task RemoveObjectAsync(SimObject simObject, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimObjectManager));
            ArgumentNullException.ThrowIfNull(simObject);

            if (!simObject.IsActive)
            {
                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"SimObjectManager: Object {simObject.ObjectId} is already inactive");
                }

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var requestId = Interlocked.Increment(ref this.nextRequestId);
            var result = await this.client.InvokeNativeAsync(
                handle => SimConnectNative.SimConnect_AIRemoveObject(
                    handle,
                    simObject.ObjectId,
                    requestId),
                cancellationToken).ConfigureAwait(false);

            if (result != (int)SimConnectError.None)
            {
                throw SimConnectErrorMapper.Wrap($"Remove AI object {simObject.ObjectId}", result);
            }

            this.UntrackObject(simObject);

            SimConnectLogger.Info($"SimObjectManager: Removed object {simObject}");
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

            SimConnectLogger.Info($"SimObjectManager: Removing {objects.Count} active objects");

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

            if (!this.objectsByType.TryGetValue(containerTitle, out var objects))
            {
                return Enumerable.Empty<SimObject>();
            }

            return objects.Values.Where(static obj => obj.IsActive);
        }

        /// <summary>
        /// Sets a SimVar value on a specific AI simulation object.
        /// This is a convenience wrapper over <see cref="SimVarManager.SetAsync{T}(string, string, T, uint, System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="simObject">The target simulation object.</param>
        /// <param name="simVarName">The SimVar name (e.g. "BARS_LIGHT_GREEN").</param>
        /// <param name="unit">The SimVar unit (use an empty string for unit-less custom vars).</param>
        /// <param name="value">The value to set.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the manager is disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simObject"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the object is inactive.</exception>
        public Task SetDataAsync<T>(SimObject simObject, string simVarName, string unit, T value, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimObjectManager));
            ArgumentNullException.ThrowIfNull(simObject);
            if (!simObject.IsActive)
            {
                throw new InvalidOperationException($"Cannot set data on inactive object {simObject.ObjectId}");
            }

            return this.client.SimVars.SetAsync(simVarName, unit, value, simObject.ObjectId, cancellationToken);
        }

        /// <summary>
        /// Sets multiple SimVar values on an AI object concurrently for efficiency.
        /// </summary>
        /// <param name="simObject">The target simulation object.</param>
        /// <param name="values">A collection of (Name, Unit, Value) tuples.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that completes when all values have been set.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the manager is disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simObject"/> or <paramref name="values"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the object is inactive.</exception>
        public Task SetDataBatchAsync(SimObject simObject, IEnumerable<(string Name, string Unit, object Value)> values, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimObjectManager));
            ArgumentNullException.ThrowIfNull(simObject);
            ArgumentNullException.ThrowIfNull(values);
            if (!simObject.IsActive)
            {
                throw new InvalidOperationException($"Cannot set data on inactive object {simObject.ObjectId}");
            }

            var tasks = values.Select(tuple =>
            {
                ArgumentNullException.ThrowIfNull(tuple.Value);

                var generic = this.GetSetAsyncMethod(tuple.Value.GetType());
                return (Task)generic.Invoke(
                    this.client.SimVars,
                    new object[] { tuple.Name, tuple.Unit, tuple.Value, simObject.ObjectId, cancellationToken })!;
            });

            return Task.WhenAll(tasks);
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
            if (this.pendingCreations.TryRemove(requestId, out var pendingCreation))
            {
                var simObject = new SimObject(
                    objectId,
                    pendingCreation.ContainerTitle,
                    requestId,
                    pendingCreation.Position);
                this.TrackObject(simObject);
                pendingCreation.Completion.SetResult(simObject);

                SimConnectLogger.Info($"SimObjectManager: Object creation completed - {simObject}");
            }
            else
            {
                SimConnectLogger.Warning($"SimObjectManager: Received unexpected object creation for requestId {requestId}");
            }
        }

        /// <summary>
        /// Processes object creation failure.
        /// </summary>
        /// <param name="requestId">The request ID that failed.</param>
        /// <param name="error">The error that occurred.</param>
        public void ProcessObjectCreationFailed(uint requestId, SimConnectError error)
        {
            if (this.pendingCreations.TryRemove(requestId, out var pendingCreation))
            {
                pendingCreation.Completion.SetException(SimConnectErrorMapper.Wrap("Object creation", error));
                SimConnectLogger.Error($"SimObjectManager: Object creation failed for requestId {requestId}: {SimConnectErrorMapper.Format(error)}");
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
                foreach (var pendingCreation in this.pendingCreations.Values)
                {
                    pendingCreation.Completion.TrySetCanceled();
                }

                this.pendingCreations.Clear();

                // Mark all objects as inactive (don't remove from sim since we're disposing)
                foreach (var obj in this.managedObjects.Values)
                {
                    obj.IsActive = false;
                }

                this.managedObjects.Clear();
                this.objectsByType.Clear();
                Volatile.Write(ref this.activeObjectCount, 0);

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug("SimObjectManager: Disposed");
                }
            }
            finally
            {
                this.disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private MethodInfo GetSetAsyncMethod(Type valueType)
        {
            return this.setAsyncMethodCache.GetOrAdd(
                valueType,
                static type => SetAsyncByNameMethod.MakeGenericMethod(type));
        }

        private void TrackObject(SimObject simObject)
        {
            while (true)
            {
                if (this.managedObjects.TryAdd(simObject.ObjectId, simObject))
                {
                    this.AddToTypeIndex(simObject);
                    Interlocked.Increment(ref this.activeObjectCount);
                    return;
                }

                if (!this.managedObjects.TryGetValue(simObject.ObjectId, out var existing))
                {
                    continue;
                }

                if (this.managedObjects.TryUpdate(simObject.ObjectId, simObject, existing))
                {
                    if (existing.IsActive)
                    {
                        existing.IsActive = false;
                        this.RemoveFromTypeIndex(existing);
                        Interlocked.Decrement(ref this.activeObjectCount);
                    }

                    this.AddToTypeIndex(simObject);
                    Interlocked.Increment(ref this.activeObjectCount);
                    return;
                }
            }
        }

        private void UntrackObject(SimObject simObject)
        {
            if (this.managedObjects.TryRemove(simObject.ObjectId, out var trackedObject))
            {
                var wasActive = trackedObject.IsActive;
                trackedObject.IsActive = false;
                this.RemoveFromTypeIndex(trackedObject);
                if (wasActive)
                {
                    Interlocked.Decrement(ref this.activeObjectCount);
                }

                if (!ReferenceEquals(simObject, trackedObject))
                {
                    simObject.IsActive = false;
                }

                return;
            }

            simObject.IsActive = false;
        }

        private void AddToTypeIndex(SimObject simObject)
        {
            var objects = this.objectsByType.GetOrAdd(
                simObject.ContainerTitle,
                static _ => new ConcurrentDictionary<uint, SimObject>());

            objects[simObject.ObjectId] = simObject;
        }

        private void RemoveFromTypeIndex(SimObject simObject)
        {
            if (!this.objectsByType.TryGetValue(simObject.ContainerTitle, out var objects))
            {
                return;
            }

            objects.TryRemove(simObject.ObjectId, out _);
            if (objects.IsEmpty)
            {
                this.objectsByType.TryRemove(new KeyValuePair<string, ConcurrentDictionary<uint, SimObject>>(simObject.ContainerTitle, objects));
            }
        }

        private sealed class PendingObjectCreation
        {
            public PendingObjectCreation(string containerTitle, SimConnectDataInitPosition position)
            {
                this.ContainerTitle = containerTitle;
                this.Position = position;
            }

            public string ContainerTitle { get; }

            public SimConnectDataInitPosition Position { get; }

            public TaskCompletionSource<SimObject> Completion { get; } = new();
        }
    }
}
