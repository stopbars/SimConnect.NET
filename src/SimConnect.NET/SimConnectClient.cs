// <copyright file="SimConnectClient.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.AI;
using SimConnect.NET.Aircraft;
using SimConnect.NET.Events;
using SimConnect.NET.InputEvents;
using SimConnect.NET.Internal;
using SimConnect.NET.SimVar;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents a client for interacting with the SimConnect API.
    /// </summary>
    public sealed class SimConnectClient : IDisposable, IAsyncDisposable
    {
        private readonly string applicationName;
        private readonly SimConnectNativeDispatcher nativeDispatcher = new();
        private IntPtr simConnectHandle = IntPtr.Zero;
        private bool isConnected;
        private bool disposed;
        private CancellationTokenSource? messageLoopCancellation;
        private Task? messageProcessingTask;
        private TaskCompletionSource<bool>? simulatorIdentification;
        private SimVarManager? simVarManager;
        private AircraftDataManager? aircraftDataManager;
        private SimObjectManager? simObjectManager;
        private InputEventManager? inputEventManager;
        private InputGroupManager? inputGroupManager;
        private int reconnectAttempts;
        private Task? reconnectTask;
        private CancellationTokenSource? reconnectCancellation;
        private bool isMSFS2024;
        private bool isDisconnecting;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectClient"/> class.
        /// </summary>
        /// <param name="applicationName">The name of the application connecting to SimConnect.</param>
        public SimConnectClient(string applicationName = "SimConnect.NET Client")
        {
            this.applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        }

        /// <summary>
        /// Occurs when the connection status changes.
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        /// <summary>
        /// Occurs when a SimConnect error is encountered.
        /// </summary>
        public event EventHandler<SimConnectErrorEventArgs>? ErrorOccurred;

        /// <summary>
        /// Occurs when any raw SimConnect message is received before it is dispatched to managers.
        /// Allows advanced consumers to inspect or override low-level processing.
        /// The underlying memory pointed to by <see cref="RawSimConnectMessageEventArgs.DataPointer"/> is only valid for the duration of the event callback.
        /// </summary>
        public event EventHandler<RawSimConnectMessageEventArgs>? RawMessageReceived;

        /// <summary>
        /// Occurs when a typed frame system event is received (frame rate and sim speed).
        /// </summary>
        public event EventHandler<SimSystemEventFrameEventArgs>? FrameEventReceived;

        /// <summary>
        /// Occurs when a typed filename-based system event is received (for example FlightLoaded or FlightSaved).
        /// </summary>
        public event EventHandler<SimSystemEventFilenameReceivedEventArgs>? FilenameEventReceived;

        /// <summary>
        /// Occurs when an object add/remove system event is received.
        /// </summary>
        public event EventHandler<SimSystemEventObjectAddRemoveEventArgs>? ObjectAddRemoveEventReceived;

        /// <summary>
        /// Occurs when an extended EX1 system event is received with additional data payload.
        /// </summary>
        public event EventHandler<SimSystemEventEx1ReceivedEventArgs>? SystemEventEx1Received;

        /// <summary>
        /// Occurs when a subscribed event is fired.
        /// </summary>
        public event EventHandler<SimSystemEventReceivedEventArgs>? SystemEventReceived;

        /// <summary>
        /// Gets a value indicating whether the client is connected to SimConnect.
        /// </summary>
        public bool IsConnected => this.isConnected;

        /// <summary>
        /// Gets a value indicating whether the connected simulator instance is Microsoft Flight Simulator 2024.
        /// Determined from the <c>ApplicationVersionMajor</c> field of the initial SimConnect OPEN message (value 12 currently indicates MSFS 2024 per SDK forum guidance).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        public bool IsMSFS2024
        {
            get
            {
                if (!this.isConnected)
                {
                    throw new InvalidOperationException("Not connected to SimConnect. Call ConnectAsync first.");
                }

                return this.isMSFS2024;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto-reconnection is enabled.
        /// </summary>
        public bool AutoReconnectEnabled { get; set; }

        /// <summary>
        /// Gets or sets the delay between reconnection attempts.
        /// </summary>
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the maximum number of reconnection attempts.
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 3;

        /// <summary>
        /// Gets the SimVar manager for dynamic SimVar access.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        public SimVarManager SimVars
        {
            get
            {
                if (!this.isConnected || this.simVarManager == null)
                {
                    throw new InvalidOperationException("Not connected to SimConnect. Call ConnectAsync first.");
                }

                return this.simVarManager;
            }
        }

        /// <summary>
        /// Gets the aircraft data manager for convenient access to common aircraft data.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        public AircraftDataManager Aircraft
        {
            get
            {
                if (!this.isConnected || this.aircraftDataManager == null)
                {
                    throw new InvalidOperationException("Not connected to SimConnect. Call ConnectAsync first.");
                }

                return this.aircraftDataManager;
            }
        }

        /// <summary>
        /// Gets the AI object manager for creating and managing simulation objects.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        public SimObjectManager AIObjects
        {
            get
            {
                if (!this.isConnected || this.simObjectManager == null)
                {
                    throw new InvalidOperationException("Not connected to SimConnect. Call ConnectAsync first.");
                }

                return this.simObjectManager;
            }
        }

        /// <summary>
        /// Gets the input event manager for handling input events and key bindings.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        public InputEventManager InputEvents
        {
            get
            {
                if (!this.isConnected || this.inputEventManager == null)
                {
                    throw new InvalidOperationException("Not connected to SimConnect. Call ConnectAsync first.");
                }

                return this.inputEventManager;
            }
        }

        /// <summary>
        /// Gets the input group manager for organizing and prioritizing input events.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        public InputGroupManager InputGroups
        {
            get
            {
                if (!this.isConnected || this.inputGroupManager == null)
                {
                    throw new InvalidOperationException("Not connected to SimConnect. Call ConnectAsync first.");
                }

                return this.inputGroupManager;
            }
        }

        /// <summary>
        /// Gets the SimConnect handle for advanced operations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        public IntPtr Handle
        {
            get
            {
                if (!this.isConnected || this.simConnectHandle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Not connected to SimConnect. Call ConnectAsync first.");
                }

                return this.simConnectHandle;
            }
        }

        /// <summary>
        /// Connects to the SimConnect server.
        /// </summary>
        /// <param name="windowHandle">Handle to a window (can be IntPtr.Zero for console apps).</param>
        /// <param name="userEventWin32">User-defined win32 event (0 for default).</param>
        /// <param name="configIndex">Configuration index (0 for default).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous connect operation.</returns>
        /// <exception cref="SimConnectException">Thrown when connection fails.</exception>
        public async Task ConnectAsync(IntPtr windowHandle = default, uint userEventWin32 = 0, uint configIndex = 0, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectClient));

            if (this.isConnected)
            {
                throw new InvalidOperationException("Already connected to SimConnect.");
            }

            this.simulatorIdentification = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await this.nativeDispatcher.InvokeAsync(
                () =>
                {
                    var result = SimConnectNative.SimConnect_Open(
                        out this.simConnectHandle,
                        this.applicationName,
                        windowHandle,
                        userEventWin32,
                        IntPtr.Zero,
                        configIndex);

                    if (result != (int)SimConnectError.None)
                    {
                        throw SimConnectErrorMapper.Wrap("Connect to SimConnect", result);
                    }
                },
                cancellationToken).ConfigureAwait(false);

            this.isConnected = true;
            this.OnConnectionStatusChanged(false, true);

            this.simVarManager = new SimVarManager(this.simConnectHandle, this.nativeDispatcher);
            this.aircraftDataManager = new AircraftDataManager(this.simVarManager);
            this.simObjectManager = new SimObjectManager(this);
            this.inputEventManager = new InputEventManager(this.simConnectHandle, this.nativeDispatcher);
            this.inputGroupManager = new InputGroupManager(this.simConnectHandle, this.nativeDispatcher);

            this.messageLoopCancellation = new CancellationTokenSource();
            this.messageProcessingTask = this.StartMessageProcessingLoopAsync(this.messageLoopCancellation.Token);
        }

        /// <summary>
        /// Disconnects from the SimConnect server.
        /// </summary>
        /// <returns>A task that represents the asynchronous disconnect operation.</returns>
        public async Task DisconnectAsync()
        {
            if (this.disposed)
            {
                return;
            }

            this.isDisconnecting = true;
            try
            {
                this.reconnectCancellation?.Cancel();

                if (this.messageLoopCancellation != null)
                {
                    this.messageLoopCancellation.Cancel();
                    if (this.messageProcessingTask != null)
                    {
                        try
                        {
                            await this.messageProcessingTask.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                            {
                                SimConnectLogger.Debug("Message processing task canceled during disconnect.");
                            }
                        }
                    }

                    this.messageLoopCancellation.Dispose();
                    this.messageLoopCancellation = null;
                    this.messageProcessingTask = null;
                }

                if (this.reconnectTask != null && !this.reconnectTask.IsCompleted)
                {
                    try
                    {
                        await this.reconnectTask.ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                        {
                            SimConnectLogger.Debug("Reconnect task canceled during disconnect.");
                        }
                    }
                }

                this.reconnectCancellation?.Dispose();
                this.reconnectCancellation = null;
                this.reconnectTask = null;

                this.simObjectManager?.Dispose();
                this.simVarManager?.Dispose();
                this.inputEventManager?.Dispose();
                this.inputGroupManager?.Dispose();
                this.simObjectManager = null;
                this.simulatorIdentification?.TrySetCanceled();
                this.simulatorIdentification = null;
                this.isMSFS2024 = false;
                this.simVarManager = null;
                this.aircraftDataManager = null;
                this.inputEventManager = null;
                this.inputGroupManager = null;

                if (this.isConnected && this.simConnectHandle != IntPtr.Zero)
                {
                    var wasConnected = this.isConnected;
                    var result = this.nativeDispatcher.Invoke(() => SimConnectNative.SimConnect_Close(this.simConnectHandle));
                    this.simConnectHandle = IntPtr.Zero;
                    this.isConnected = false;

                    if (wasConnected)
                    {
                        this.OnConnectionStatusChanged(true, false);
                    }

                    if (result != (int)SimConnectError.None)
                    {
                        SimConnectLogger.Warning($"SimConnect_Close returned error: {SimConnectErrorMapper.Format(result)}");
                    }
                }
            }
            finally
            {
                this.isDisconnecting = false;
            }
        }

        /// <summary>
        /// Subscribes to a specific simulator system event.
        /// </summary>
        /// <param name="systemEventName">The name of the system event (e.g., "SimStart", "4Sec", "Crashed").</param>
        /// <param name="systemEventId">A user-defined ID to identify this subscription.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task representing the subscription operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a sim connection wasn't found.</exception>
        /// <exception cref="SimConnectException">Thrown when the event wasn't subscribed.</exception>
        public async Task SubscribeToEventAsync(string systemEventName, uint systemEventId, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectClient));

            if (!this.isConnected)
            {
                throw new InvalidOperationException("Not connected to SimConnect.");
            }

            await this.nativeDispatcher.InvokeAsync(
                () =>
                {
                    var result = SimConnectNative.SimConnect_SubscribeToSystemEvent(
                        this.simConnectHandle,
                        systemEventId,
                        systemEventName);

                    if (result != (int)SimConnectError.None)
                    {
                        throw SimConnectErrorMapper.Wrap($"Subscribe to event {systemEventName}", result);
                    }
                },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the reporting state for a previously subscribed system event.
        /// </summary>
        /// <param name="systemEventId">The user-defined ID of the system event.</param>
        /// <param name="state">The desired reporting state.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task representing the state change operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a sim connection was not found.</exception>
        /// <exception cref="SimConnectException">Thrown when the state change fails.</exception>
        public async Task SetSystemEventStateAsync(uint systemEventId, SimConnectState state, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectClient));

            if (!this.isConnected)
            {
                throw new InvalidOperationException("Not connected to SimConnect.");
            }

            await this.nativeDispatcher.InvokeAsync(
                () =>
                {
                    var result = SimConnectNative.SimConnect_SetSystemEventState(
                        this.simConnectHandle,
                        systemEventId,
                        (uint)state);

                    if (result != (int)SimConnectError.None)
                    {
                        throw SimConnectErrorMapper.Wrap($"Set system event state for {systemEventId}", result);
                    }
                },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribes from a previously subscribed system event.
        /// </summary>
        /// <param name="systemEventId">The user-defined ID of the system event.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task representing the unsubscribe operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a sim connection was not found.</exception>
        /// <exception cref="SimConnectException">Thrown when the unsubscribe fails.</exception>
        public async Task UnsubscribeFromEventAsync(uint systemEventId, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectClient));

            if (!this.isConnected)
            {
                throw new InvalidOperationException("Not connected to SimConnect.");
            }

            await this.nativeDispatcher.InvokeAsync(
                () =>
                {
                    var result = SimConnectNative.SimConnect_UnsubscribeFromSystemEvent(
                        this.simConnectHandle,
                        systemEventId);

                    if (result != (int)SimConnectError.None)
                    {
                        throw SimConnectErrorMapper.Wrap($"Unsubscribe from system event {systemEventId}", result);
                    }
                },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes the next SimConnect message.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous message processing operation, returning true if a message was processed.</returns>
        /// <exception cref="SimConnectException">Thrown when message processing fails.</exception>
        public Task<bool> ProcessNextMessageAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectClient));

            if (!this.isConnected)
            {
                throw new InvalidOperationException("Not connected to SimConnect.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var dispatch = this.nativeDispatcher.Invoke(
                () =>
                {
                    var result = SimConnectNative.SimConnect_GetNextDispatch(this.simConnectHandle, out var ppData, out var pcbData);
                    return (Result: result, Data: ppData, DataSize: pcbData);
                });
            var result = dispatch.Result;
            var ppData = dispatch.Data;
            var pcbData = dispatch.DataSize;

            if (result != (int)SimConnectError.None)
            {
                // Filter out the common "no messages available" error to reduce log spam
                if (result != SimConnectNative.DispatchNoMessageAvailableHResult && SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"SimConnect_GetNextDispatch returned: {SimConnectErrorMapper.Format(result)}");
                }

                return Task.FromResult(false);
            }

            if (ppData != IntPtr.Zero && pcbData > 0)
            {
                var recv = Marshal.PtrToStructure<SimConnectRecv>(ppData);
                var recvId = (SimConnectRecvId)recv.Id;

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"Received SimConnect message: Id={recv.Id}, Size={recv.Size}");
                }

                try
                {
                    this.RawMessageReceived?.Invoke(this, new RawSimConnectMessageEventArgs(ppData, pcbData, recvId));
                }
                catch (Exception hookEx) when (!ExceptionHelper.IsCritical(hookEx))
                {
                    SimConnectLogger.Warning($"RawMessageReceived hook threw: {hookEx.Message}");
                }

                switch (recvId)
                {
                    case SimConnectRecvId.AssignedObjectId:
                        this.ProcessAssignedObjectId(ppData);
                        break;
                    case SimConnectRecvId.Exception:
                        this.ProcessError(ppData);
                        break;
                    case SimConnectRecvId.Open:
                        this.ProcessOpen(ppData);
                        break;
                    case SimConnectRecvId.ControllersList:
                    case SimConnectRecvId.ActionCallback:
                    case SimConnectRecvId.EnumerateInputEvents:
                    case SimConnectRecvId.EnumerateInputEventParams:
                    case SimConnectRecvId.GetInputEvent:
                    case SimConnectRecvId.SubscribeInputEvent:
                        this.inputEventManager?.ProcessReceivedData(ppData, pcbData);
                        break;
                    case SimConnectRecvId.AirportList:
                    case SimConnectRecvId.VorList:
                    case SimConnectRecvId.NdbList:
                        break;
                    case SimConnectRecvId.Event:
                        this.ProcessSystemEvent(ppData);
                        break;
                    case SimConnectRecvId.EventFrame:
                        this.ProcessSystemEventFrame(ppData);
                        break;
                    case SimConnectRecvId.EventFilename:
                        this.ProcessSystemEventFilename(ppData);
                        break;
                    case SimConnectRecvId.EventObjectAddRemove:
                        this.ProcessSystemEventObjectAddRemove(ppData);
                        break;
                    case SimConnectRecvId.EventEx1:
                        this.ProcessSystemEventEx1(ppData);
                        break;
                    default:
                        this.simVarManager?.ProcessReceivedData(ppData, pcbData);
                        break;
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Tests the connection to SimConnect by performing a simple operation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous test operation, returning true if the connection is healthy.</returns>
        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectClient));

            if (!this.isConnected)
            {
                return false;
            }

            try
            {
                await this.SimVars.GetAsync<double>("SIMULATION RATE", "number", cancellationToken: cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                this.OnErrorOccurred(SimConnectError.Error, ex, "Connection health check failed");
                return false;
            }
        }

        /// <summary>
        /// Disposes the SimConnect client and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                GC.SuppressFinalize(this);
                return;
            }

            this.DisconnectAsync().GetAwaiter().GetResult();
            this.disposed = true;
            this.nativeDispatcher.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the SimConnect client and releases resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            if (this.disposed)
            {
                GC.SuppressFinalize(this);
                return;
            }

            await this.DisconnectAsync().ConfigureAwait(false);
            this.disposed = true;
            this.nativeDispatcher.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Invokes a native SimConnect operation with serialized access to the underlying handle.
        /// </summary>
        /// <typeparam name="T">The operation result type.</typeparam>
        /// <param name="operation">The operation to invoke.</param>
        /// <param name="cancellationToken">Cancellation token for waiting to enter the dispatcher.</param>
        /// <returns>A task containing the operation result.</returns>
        internal Task<T> InvokeNativeAsync<T>(Func<IntPtr, T> operation, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            return this.nativeDispatcher.InvokeAsync(() => operation(this.Handle), cancellationToken);
        }

        internal async Task<bool> GetIsMSFS2024Async(CancellationToken cancellationToken)
        {
            var identification = this.simulatorIdentification;
            if (identification == null)
            {
                throw new InvalidOperationException("Simulator identification is unavailable because SimConnect is not connected.");
            }

            return await identification.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes an assigned object ID message from SimConnect.
        /// </summary>
        /// <param name="ppData">Pointer to the received data.</param>
        private void ProcessAssignedObjectId(IntPtr ppData)
        {
            try
            {
                var recvAssignedObjectId = Marshal.PtrToStructure<SimConnectRecvAssignedObjectId>(ppData);

                this.simObjectManager?.ProcessObjectCreated(
                    recvAssignedObjectId.RequestId,
                    recvAssignedObjectId.ObjectId,
                    string.Empty,
                    default(SimConnectDataInitPosition));

                SimConnectLogger.Info($"Processed assigned object ID: RequestId={recvAssignedObjectId.RequestId}, ObjectId={recvAssignedObjectId.ObjectId}");
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing assigned object ID", ex);
            }
        }

        /// <summary>
        /// Processes an error/exception message from SimConnect.
        /// </summary>
        /// <param name="ppData">Pointer to the received data.</param>
        private void ProcessError(IntPtr ppData)
        {
            try
            {
                var recvError = Marshal.PtrToStructure<SimConnectRecvError>(ppData);
                var error = (SimConnectError)recvError.ExceptionCode;

                SimConnectLogger.Warning($"SimConnect error received: {SimConnectErrorMapper.Format(error)} (SendId={recvError.SendId}, Index={recvError.Index})");

                SimConnectException? creationException = null;
                if (this.simObjectManager != null &&
                    this.simObjectManager.TryResolveRequestId(recvError.SendId, out var requestId))
                {
                    creationException = this.simObjectManager.ProcessObjectCreationFailed(
                        requestId,
                        error,
                        recvError.SendId,
                        recvError.Index);
                }

                this.OnErrorOccurred(
                    error,
                    creationException ?? SimConnectErrorMapper.Wrap("SimConnect server request", error),
                    $"SimConnect error (SendId={recvError.SendId}, Index={recvError.Index})",
                    recvError.SendId,
                    recvError.Index);
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing SimConnect error message", ex);
                this.OnErrorOccurred(SimConnectError.Error, ex, "Error processing SimConnect error message");
            }
        }

        /// <summary>
        /// Processes the OPEN message from SimConnect to capture simulator version information.
        /// </summary>
        /// <param name="ppData">Pointer to the received OPEN data.</param>
        private void ProcessOpen(IntPtr ppData)
        {
            try
            {
                var recvOpen = Marshal.PtrToStructure<SimConnectRecvOpen>(ppData);

                // According to community reports (and current beta docs), ApplicationVersionMajor == 12 indicates MSFS 2024.
                this.isMSFS2024 = recvOpen.ApplicationVersionMajor == 12;
                this.simulatorIdentification?.TrySetResult(this.isMSFS2024);
                SimConnectLogger.Info($"SimConnect OPEN received: AppVersion={recvOpen.ApplicationVersionMajor}.{recvOpen.ApplicationVersionMinor} Build={recvOpen.ApplicationBuildMajor}.{recvOpen.ApplicationBuildMinor} (IsMSFS2024={this.isMSFS2024})");
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing SimConnect OPEN message", ex);
            }
        }

        /// <summary>
        /// Processes a system event message from SimConnect.
        /// </summary>
        /// <param name="ppData">Pointer to the received Event data.</param>
        private void ProcessSystemEvent(IntPtr ppData)
        {
            try
            {
                var recvEvent = Marshal.PtrToStructure<SimConnectRecvEvent>(ppData);

                this.SystemEventReceived?.Invoke(this, new SimSystemEventReceivedEventArgs(recvEvent.EventId, recvEvent.Data));

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"System Event Received: ID={recvEvent.EventId} Data={recvEvent.Data}");
                }
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing system event", ex);
            }
        }

        /// <summary>
        /// Processes a system frame event message from SimConnect.
        /// </summary>
        /// <param name="ppData">Pointer to the received EventFrame data.</param>
        private void ProcessSystemEventFrame(IntPtr ppData)
        {
            try
            {
                var recvEventFrame = Marshal.PtrToStructure<SimConnectRecvEventFrame>(ppData);

                this.FrameEventReceived?.Invoke(this, new SimSystemEventFrameEventArgs(recvEventFrame.FrameRate, recvEventFrame.SimSpeed));

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"Frame Event Received: FrameRate={recvEventFrame.FrameRate} SimSpeed={recvEventFrame.SimSpeed}");
                }
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing frame system event", ex);
            }
        }

        /// <summary>
        /// Processes a system filename event message from SimConnect.
        /// </summary>
        /// <param name="ppData">Pointer to the received EventFilename data.</param>
        private void ProcessSystemEventFilename(IntPtr ppData)
        {
            try
            {
                var recvEventFilename = Marshal.PtrToStructure<SimConnectRecvEventFilename>(ppData);

                this.FilenameEventReceived?.Invoke(this, new SimSystemEventFilenameReceivedEventArgs(recvEventFilename.FileName, recvEventFilename.Flags));

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"Filename Event Received: FileName={recvEventFilename.FileName} Flags={recvEventFilename.Flags}");
                }
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing filename system event", ex);
            }
        }

        /// <summary>
        /// Processes an object add/remove system event message from SimConnect.
        /// </summary>
        /// <param name="ppData">Pointer to the received EventObjectAddRemove data.</param>
        private void ProcessSystemEventObjectAddRemove(IntPtr ppData)
        {
            try
            {
                var recvEventObject = Marshal.PtrToStructure<SimConnectRecvEventObjectAddRemove>(ppData);

                this.ObjectAddRemoveEventReceived?.Invoke(this, new SimSystemEventObjectAddRemoveEventArgs(recvEventObject.EObjType));

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"Object Add/Remove Event Received: Type={recvEventObject.EObjType}");
                }
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing object add/remove system event", ex);
            }
        }

        /// <summary>
        /// Processes an extended EX1 system event message from SimConnect.
        /// </summary>
        /// <param name="ppData">Pointer to the received EventEx1 data.</param>
        private void ProcessSystemEventEx1(IntPtr ppData)
        {
            try
            {
                var recvEventEx1 = Marshal.PtrToStructure<SimConnectRecvEventEx1>(ppData);

                this.SystemEventEx1Received?.Invoke(
                    this,
                    new SimSystemEventEx1ReceivedEventArgs(
                        recvEventEx1.EventId,
                        recvEventEx1.Data0,
                        recvEventEx1.Data1,
                        recvEventEx1.Data2,
                        recvEventEx1.Data3,
                        recvEventEx1.Data4));

                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug($"EX1 System Event Received: EventId={recvEventEx1.EventId} Data=[{recvEventEx1.Data0},{recvEventEx1.Data1},{recvEventEx1.Data2},{recvEventEx1.Data3},{recvEventEx1.Data4}]");
                }
            }
            catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
            {
                SimConnectLogger.Error("Error processing EX1 system event", ex);
            }
        }

        /// <summary>
        /// Starts the background message processing loop.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop the loop.</param>
        /// <returns>A task that represents the message processing loop.</returns>
        private async Task StartMessageProcessingLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                int consecutiveEmptyPolls = 0;

                while (!cancellationToken.IsCancellationRequested && this.isConnected)
                {
                    bool messageProcessed = false;

                    try
                    {
                        messageProcessed = await this.ProcessNextMessageAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (SimConnectException ex)
                    {
                        this.OnErrorOccurred(ex.ErrorCode, ex, "Message processing failed");

                        // If this is a connection-related error, trigger auto-reconnection
                        if (this.AutoReconnectEnabled && ex.ErrorCode != SimConnectError.UnrecognizedId)
                        {
                            var wasConnected = this.isConnected;
                            this.isConnected = false;
                            if (wasConnected)
                            {
                                this.OnConnectionStatusChanged(true, false);
                            }

                            break;
                        }
                    }

                    if (messageProcessed)
                    {
                        consecutiveEmptyPolls = 0;

                        continue;
                    }
                    else
                    {
                        consecutiveEmptyPolls++;

                        // Adaptive delay: start with minimal delay, gradually increase
                        int delay = consecutiveEmptyPolls switch
                        {
                            <= 2 => 1,     // 1ms for first few empty polls
                            <= 5 => 5,     // 5ms for moderate polling
                            _ => 10,        // 10ms for sustained empty polling
                        };

                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (SimConnectLogger.IsLevelEnabled(SimConnectLogger.LogLevel.Debug))
                {
                    SimConnectLogger.Debug("Message processing loop canceled.");
                }
            }
        }

        /// <summary>
        /// Raises the ConnectionStatusChanged event.
        /// </summary>
        /// <param name="previousStatus">The previous connection status.</param>
        /// <param name="currentStatus">The current connection status.</param>
        private void OnConnectionStatusChanged(bool previousStatus, bool currentStatus)
        {
            var eventArgs = new ConnectionStatusChangedEventArgs(previousStatus, currentStatus, DateTime.UtcNow);
            this.ConnectionStatusChanged?.Invoke(this, eventArgs);

            if (!this.isDisconnecting && this.AutoReconnectEnabled && previousStatus && !currentStatus && this.reconnectAttempts < this.MaxReconnectAttempts)
            {
                this.StartAutoReconnectAsync();
            }
        }

        /// <summary>
        /// Raises the ErrorOccurred event.
        /// </summary>
        /// <param name="error">The SimConnect error that occurred.</param>
        /// <param name="exception">The exception that was thrown, if any.</param>
        /// <param name="context">Additional context about when/where the error occurred.</param>
        /// <param name="sendId">The native packet ID associated with the error.</param>
        /// <param name="index">The one-based parameter index associated with the error.</param>
        private void OnErrorOccurred(
            SimConnectError error,
            Exception? exception = null,
            string? context = null,
            uint? sendId = null,
            uint? index = null)
        {
            var eventArgs = new SimConnectErrorEventArgs(error, exception, context, sendId: sendId, index: index);
            this.ErrorOccurred?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Starts the auto-reconnection process.
        /// </summary>
        private void StartAutoReconnectAsync()
        {
            if (this.reconnectTask != null && !this.reconnectTask.IsCompleted)
            {
                return;
            }

            this.reconnectCancellation?.Cancel();
            this.reconnectCancellation = new CancellationTokenSource();
            this.reconnectTask = this.PerformAutoReconnectAsync(this.reconnectCancellation.Token);
        }

        /// <summary>
        /// Performs the auto-reconnection attempts.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop reconnection attempts.</param>
        /// <returns>A task representing the reconnection process.</returns>
        private async Task PerformAutoReconnectAsync(CancellationToken cancellationToken)
        {
            while (this.reconnectAttempts < this.MaxReconnectAttempts && !cancellationToken.IsCancellationRequested && !this.isConnected)
            {
                this.reconnectAttempts++;

                try
                {
                    await Task.Delay(this.ReconnectDelay, cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested || this.isConnected)
                    {
                        break;
                    }

                    await this.ConnectAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (this.isConnected)
                    {
                        this.reconnectAttempts = 0;
                        SimConnectLogger.Info("Auto-reconnection successful");
                        break;
                    }
                }
                catch (Exception ex) when (!ExceptionHelper.IsCritical(ex))
                {
                    SimConnectLogger.Warning($"Auto-reconnection attempt {this.reconnectAttempts} failed: {ex.Message}");
                    this.OnErrorOccurred(SimConnectError.Error, ex, $"Auto-reconnection attempt {this.reconnectAttempts}");
                }
            }

            if (this.reconnectAttempts >= this.MaxReconnectAttempts && !this.isConnected)
            {
                SimConnectLogger.Error("Auto-reconnection failed: Maximum attempts reached");
                this.OnErrorOccurred(SimConnectError.Error, null, "Auto-reconnection failed: Maximum attempts reached");
            }
        }
    }
}
