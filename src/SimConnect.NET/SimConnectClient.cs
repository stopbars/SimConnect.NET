// <copyright file="SimConnectClient.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.AI;
using SimConnect.NET.Aircraft;
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
        /// Gets a value indicating whether the client is connected to SimConnect.
        /// </summary>
        public bool IsConnected => this.isConnected;

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
        /// Gets the SimConnect handle for advanced operations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when not connected to SimConnect.</exception>
        internal IntPtr Handle
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

            // Initialize managers
            this.simVarManager = new SimVarManager(this.simConnectHandle);
            this.aircraftDataManager = new AircraftDataManager(this.simVarManager);
            this.simObjectManager = new SimObjectManager(this);

            // Start background message processing
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

            // Dispose managers first
            this.simObjectManager?.Dispose();
            this.simVarManager?.Dispose();
            this.simObjectManager = null;
            this.simVarManager = null;
            this.aircraftDataManager = null;

            // Stop message processing
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
                        // Expected when cancelling
                    }
                }

                this.messageLoopCancellation.Dispose();
                this.messageLoopCancellation = null;
                this.messageProcessingTask = null;
            }

            if (this.isConnected && this.simConnectHandle != IntPtr.Zero)
            {
                var result = SimConnectNative.SimConnect_Close(this.simConnectHandle);
                this.simConnectHandle = IntPtr.Zero;
                this.isConnected = false;

                // Note: We don't throw on close errors to allow graceful cleanup
                if (result != (int)SimConnectError.None)
                {
                    // Log the error if needed, but continue cleanup
                    System.Diagnostics.Debug.WriteLine($"Warning: SimConnect_Close returned error: {(SimConnectError)result}");
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

                    // Don't throw on normal "no messages" scenarios
                    if (result != (int)SimConnectError.None)
                    {
                        // Filter out the common "no messages available" error (-2147467259) to reduce log spam
                        if (result != -2147467259)
                        {
                            System.Diagnostics.Debug.WriteLine($"SimConnect_GetNextDispatch returned: {(SimConnectError)result}");
                        }

                        return false; // No message processed
                    }

                    // Process the message - determine type and route accordingly
                    if (ppData != IntPtr.Zero && pcbData > 0)
                    {
                        var recv = Marshal.PtrToStructure<SimConnectRecv>(ppData);

                        System.Diagnostics.Debug.WriteLine($"Received SimConnect message: Id={recv.Id}, Size={recv.Size}");

                        switch ((SimConnectRecvId)recv.Id)
                        {
                            case SimConnectRecvId.AssignedObjectId:
                                this.ProcessAssignedObjectId(ppData);
                                break;
                            case SimConnectRecvId.Exception:
                                this.ProcessError(ppData);
                                break;
                            default:
                                // Forward all other messages to the SimVar manager
                                this.simVarManager?.ProcessReceivedData(ppData, pcbData);
                                break;
                        }

                        return true; // Message was processed
                    }

                    return false; // No valid message
                },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Disposes the SimConnect client and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                // Use synchronous version for disposal
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

                // Forward to the SimObject manager if available
                this.simObjectManager?.ProcessObjectCreated(
                    recvAssignedObjectId.RequestId,
                    recvAssignedObjectId.ObjectId,
                    string.Empty, // Container title not available in this message
                    default(SimConnectDataInitPosition)); // Position not available in this message

                System.Diagnostics.Debug.WriteLine($"Processed assigned object ID: RequestId={recvAssignedObjectId.RequestId}, ObjectId={recvAssignedObjectId.ObjectId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing assigned object ID: {ex.Message}");
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

                System.Diagnostics.Debug.WriteLine($"SimConnect error received: {error} (SendId={recvError.SendId}, Index={recvError.Index})");

                // Forward error to SimObject manager for handling failed object operations
                if (this.simObjectManager != null)
                {
                    this.simObjectManager.ProcessObjectCreationFailed(recvError.SendId, error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing SimConnect error message: {ex.Message}");
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
                    catch (SimConnectException)
                    {
                        // Continue processing even if individual messages fail
                        // In a real implementation, you might want to log these errors
                    }

                    if (messageProcessed)
                    {
                        consecutiveEmptyPolls = 0;

                        // Process more messages immediately if available
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
                // Expected when cancelling
            }
        }
    }
}
