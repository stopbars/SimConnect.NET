// <copyright file="SimConnectClient.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.AI;
using SimConnect.NET.Aircraft;
using SimConnect.NET.Events;
using SimConnect.NET.InputEvents;
using SimConnect.NET.SimVar;

namespace SimConnect.NET
{
    /// <summary>
    /// Represents a client for interacting with the SimConnect API.
    /// </summary>
    public sealed class SimConnectClient : IDisposable
    {
        private readonly string applicationName;
        private IntPtr simConnectHandle = IntPtr.Zero;
        private bool isConnected;
        private bool disposed;
        private CancellationTokenSource? messageLoopCancellation;
        private Task? messageProcessingTask;
        private SimVarManager? simVarManager;
        private AircraftDataManager? aircraftDataManager;
        private SimObjectManager? simObjectManager;
        private InputEventManager? inputEventManager;
        private InputGroupManager? inputGroupManager;
        private int reconnectAttempts;
        private Task? reconnectTask;
        private CancellationTokenSource? reconnectCancellation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimConnectClient"/> class.
        /// </summary>
        /// <param name="applicationName">The name of the application connecting to SimConnect.</param>
        public SimConnectClient(string applicationName = "SimConnect.NET Client")
        {
            this.applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SimConnectClient"/> class.
        /// </summary>
        ~SimConnectClient()
        {
            this.Dispose();
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
        /// Gets a value indicating whether the client is connected to SimConnect.
        /// </summary>
        public bool IsConnected => this.isConnected;

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

            await Task.Run(
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
                        throw new SimConnectException($"Failed to connect to SimConnect: {(SimConnectError)result}", (SimConnectError)result);
                    }
                },
                cancellationToken).ConfigureAwait(false);

            this.isConnected = true;
            this.OnConnectionStatusChanged(false, true);

            this.simVarManager = new SimVarManager(this.simConnectHandle);
            this.aircraftDataManager = new AircraftDataManager(this.simVarManager);
            this.simObjectManager = new SimObjectManager(this);
            this.inputEventManager = new InputEventManager(this.simConnectHandle);
            this.inputGroupManager = new InputGroupManager(this.simConnectHandle);

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

            this.reconnectCancellation?.Cancel();

            this.simObjectManager?.Dispose();
            this.simVarManager?.Dispose();
            this.inputEventManager?.Dispose();
            this.inputGroupManager?.Dispose();
            this.simObjectManager = null;
            this.simVarManager = null;
            this.aircraftDataManager = null;
            this.inputEventManager = null;
            this.inputGroupManager = null;

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
                }
            }

            this.reconnectCancellation?.Dispose();
            this.reconnectCancellation = null;
            this.reconnectTask = null;

            if (this.isConnected && this.simConnectHandle != IntPtr.Zero)
            {
                var wasConnected = this.isConnected;
                var result = SimConnectNative.SimConnect_Close(this.simConnectHandle);
                this.simConnectHandle = IntPtr.Zero;
                this.isConnected = false;

                if (wasConnected)
                {
                    this.OnConnectionStatusChanged(true, false);
                }

                if (result != (int)SimConnectError.None)
                {
                    // Log warning for non-zero close result, but don't throw
                    SimConnectLogger.Warning($"SimConnect_Close returned error: {(SimConnectError)result}");
                }
            }
        }

        /// <summary>
        /// Processes the next SimConnect message.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous message processing operation, returning true if a message was processed.</returns>
        /// <exception cref="SimConnectException">Thrown when message processing fails.</exception>
        public async Task<bool> ProcessNextMessageAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(SimConnectClient));

            if (!this.isConnected)
            {
                throw new InvalidOperationException("Not connected to SimConnect.");
            }

            return await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = SimConnectNative.SimConnect_GetNextDispatch(this.simConnectHandle, out var ppData, out var pcbData);

                    if (result != (int)SimConnectError.None)
                    {
                        // Filter out the common "no messages available" error to reduce log spam
                        if (result != -2147467259)
                        {
                            SimConnectLogger.Debug($"SimConnect_GetNextDispatch returned: {(SimConnectError)result}");
                        }

                        return false;
                    }

                    if (ppData != IntPtr.Zero && pcbData > 0)
                    {
                        var recv = Marshal.PtrToStructure<SimConnectRecv>(ppData);
                        var recvId = (SimConnectRecvId)recv.Id;

                        SimConnectLogger.Debug($"Received SimConnect message: Id={recv.Id}, Size={recv.Size}");

                        try
                        {
                            this.RawMessageReceived?.Invoke(this, new RawSimConnectMessageEventArgs(ppData, pcbData, recvId));
                        }
                        catch (Exception hookEx)
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
                            default:
                                this.simVarManager?.ProcessReceivedData(ppData, pcbData);
                                break;
                        }

                        return true;
                    }

                    return false;
                },
                cancellationToken).ConfigureAwait(false);
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
            catch (Exception ex)
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
            if (!this.disposed)
            {
                Task.Run(async () => await this.DisconnectAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
                this.disposed = true;
            }

            GC.SuppressFinalize(this);
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
            catch (Exception ex)
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

                SimConnectLogger.Warning($"SimConnect error received: {error} (SendId={recvError.SendId}, Index={recvError.Index})");

                this.OnErrorOccurred(error, null, $"SimConnect error (SendId={recvError.SendId}, Index={recvError.Index})");

                if (this.simObjectManager != null)
                {
                    this.simObjectManager.ProcessObjectCreationFailed(recvError.SendId, error);
                }
            }
            catch (Exception ex)
            {
                SimConnectLogger.Error("Error processing SimConnect error message", ex);
                this.OnErrorOccurred(SimConnectError.Error, ex, "Error processing SimConnect error message");
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
                        if (this.AutoReconnectEnabled && !ex.ErrorCode.ToString().Contains("UnrecognizedId"))
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

            if (this.AutoReconnectEnabled && previousStatus && !currentStatus && this.reconnectAttempts < this.MaxReconnectAttempts)
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
        private void OnErrorOccurred(SimConnectError error, Exception? exception = null, string? context = null)
        {
            var eventArgs = new SimConnectErrorEventArgs(error, exception, context);
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
                catch (Exception ex)
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
