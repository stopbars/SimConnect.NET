// <copyright file="InputEventManager.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.Events;

namespace SimConnect.NET.InputEvents
{
    /// <summary>
    /// Manages input events, controllers, and key bindings for SimConnect.
    /// </summary>
    public sealed class InputEventManager : IDisposable
    {
        private readonly IntPtr simConnectHandle;
        private readonly ConcurrentDictionary<uint, TaskCompletionSource<InputEventDescriptor[]>> pendingEnumerationRequests;
        private readonly ConcurrentDictionary<uint, TaskCompletionSource<string>> pendingParameterRequests;
        private readonly ConcurrentDictionary<uint, TaskCompletionSource<InputEventValue>> pendingGetRequests;
        private readonly ConcurrentDictionary<ulong, Action<InputEventValue>> inputEventSubscriptions;
        private readonly object lockObject = new object();
        private uint nextRequestId = 1;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventManager"/> class.
        /// </summary>
        /// <param name="simConnectHandle">The SimConnect handle.</param>
        internal InputEventManager(IntPtr simConnectHandle)
        {
            this.simConnectHandle = simConnectHandle;
            this.pendingEnumerationRequests = new ConcurrentDictionary<uint, TaskCompletionSource<InputEventDescriptor[]>>();
            this.pendingParameterRequests = new ConcurrentDictionary<uint, TaskCompletionSource<string>>();
            this.pendingGetRequests = new ConcurrentDictionary<uint, TaskCompletionSource<InputEventValue>>();
            this.inputEventSubscriptions = new ConcurrentDictionary<ulong, Action<InputEventValue>>();
        }

        /// <summary>
        /// Occurs when an input event value changes (for subscribed events).
        /// </summary>
        public event EventHandler<InputEventChangedEventArgs>? InputEventChanged;

        /// <summary>
        /// Enumerates all controllers currently connected to the simulation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous enumeration operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the enumeration fails.</exception>
        public async Task EnumerateControllersAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_EnumerateControllers(this.simConnectHandle);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to enumerate controllers: {(SimConnectError)result}", (SimConnectError)result);
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Enumerates all available input events for the current aircraft.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous enumeration operation, returning an array of input event descriptors.</returns>
        /// <exception cref="SimConnectException">Thrown when the enumeration fails.</exception>
        public async Task<InputEventDescriptor[]> EnumerateInputEventsAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            var requestId = this.GetNextRequestId();
            var tcs = new TaskCompletionSource<InputEventDescriptor[]>();
            this.pendingEnumerationRequests[requestId] = tcs;

            try
            {
                await Task.Run(
                    () =>
                {
                    var result = SimConnectNative.SimConnect_EnumerateInputEvents(this.simConnectHandle, requestId);
                    if (result != (int)SimConnectError.None)
                    {
                        throw new SimConnectException($"Failed to enumerate input events: {(SimConnectError)result}", (SimConnectError)result);
                    }
                },
                    cancellationToken).ConfigureAwait(false);

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                this.pendingEnumerationRequests.TryRemove(requestId, out _);
            }
        }

        /// <summary>
        /// Enumerates the parameters for a specific input event.
        /// </summary>
        /// <param name="hash">The hash ID of the input event.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous enumeration operation, returning a string describing the parameters.</returns>
        /// <exception cref="SimConnectException">Thrown when the enumeration fails.</exception>
        public async Task<string> EnumerateInputEventParametersAsync(ulong hash, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            var requestId = this.GetNextRequestId();
            var tcs = new TaskCompletionSource<string>();
            this.pendingParameterRequests[requestId] = tcs;

            try
            {
                await Task.Run(
                    () =>
                {
                    var result = SimConnectNative.SimConnect_EnumerateInputEventParams(this.simConnectHandle, requestId);
                    if (result != (int)SimConnectError.None)
                    {
                        throw new SimConnectException($"Failed to enumerate input event parameters: {(SimConnectError)result}", (SimConnectError)result);
                    }
                },
                    cancellationToken).ConfigureAwait(false);

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                this.pendingParameterRequests.TryRemove(requestId, out _);
            }
        }

        /// <summary>
        /// Gets the current value of a specific input event.
        /// </summary>
        /// <param name="hash">The hash ID of the input event.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous get operation, returning the input event value.</returns>
        /// <exception cref="SimConnectException">Thrown when the get operation fails.</exception>
        public async Task<InputEventValue> GetInputEventAsync(ulong hash, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            var requestId = this.GetNextRequestId();
            var tcs = new TaskCompletionSource<InputEventValue>();
            this.pendingGetRequests[requestId] = tcs;

            try
            {
                await Task.Run(
                    () =>
                {
                    var result = SimConnectNative.SimConnect_GetInputEvent(this.simConnectHandle, requestId, hash);
                    if (result != (int)SimConnectError.None)
                    {
                        throw new SimConnectException($"Failed to get input event: {(SimConnectError)result}", (SimConnectError)result);
                    }
                },
                    cancellationToken).ConfigureAwait(false);

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    var value = await tcs.Task.ConfigureAwait(false);
                    value.Hash = hash; // Set the hash for reference
                    return value;
                }
            }
            finally
            {
                this.pendingGetRequests.TryRemove(requestId, out _);
            }
        }

        /// <summary>
        /// Sets the value of a specific input event.
        /// </summary>
        /// <param name="hash">The hash ID of the input event.</param>
        /// <param name="value">The value to set (double).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the set operation fails.</exception>
        public async Task SetInputEventAsync(ulong hash, double value, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var valueBytes = BitConverter.GetBytes(value);
                var valuePtr = Marshal.AllocHGlobal(valueBytes.Length);
                try
                {
                    Marshal.Copy(valueBytes, 0, valuePtr, valueBytes.Length);
                    var result = SimConnectNative.SimConnect_SetInputEvent(this.simConnectHandle, (ulong)hash, (uint)valueBytes.Length, valuePtr);
                    if (result != (int)SimConnectError.None)
                    {
                        throw new SimConnectException($"Failed to set input event: {(SimConnectError)result}", (SimConnectError)result);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(valuePtr);
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the value of a specific input event.
        /// </summary>
        /// <param name="hash">The hash ID of the input event.</param>
        /// <param name="value">The value to set (string).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the set operation fails.</exception>
        public async Task SetInputEventAsync(ulong hash, string value, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(value);

            await Task.Run(
                () =>
            {
                var valueBytes = System.Text.Encoding.UTF8.GetBytes(value + '\0'); // Null terminate
                var valuePtr = Marshal.AllocHGlobal(valueBytes.Length);
                try
                {
                    Marshal.Copy(valueBytes, 0, valuePtr, valueBytes.Length);
                    var result = SimConnectNative.SimConnect_SetInputEvent(this.simConnectHandle, (ulong)hash, (uint)valueBytes.Length, valuePtr);
                    if (result != (int)SimConnectError.None)
                    {
                        throw new SimConnectException($"Failed to set input event: {(SimConnectError)result}", (SimConnectError)result);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(valuePtr);
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribes to value changes for a specific input event.
        /// </summary>
        /// <param name="hash">The hash ID of the input event (use 0 to subscribe to all events).</param>
        /// <param name="callback">Callback function to invoke when the value changes.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous subscription operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the subscription fails.</exception>
        public async Task SubscribeToInputEventAsync(ulong hash, Action<InputEventValue> callback, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(callback);

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_SubscribeInputEvent(this.simConnectHandle, hash);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to subscribe to input event: {(SimConnectError)result}", (SimConnectError)result);
                }

                this.inputEventSubscriptions[hash] = callback;
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribes from value changes for a specific input event.
        /// </summary>
        /// <param name="hash">The hash ID of the input event (use 0 to unsubscribe from all events).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous unsubscription operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the unsubscription fails.</exception>
        public async Task UnsubscribeFromInputEventAsync(ulong hash, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_UnsubscribeInputEvent(this.simConnectHandle, hash);
                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to unsubscribe from input event: {(SimConnectError)result}", (SimConnectError)result);
                }

                this.inputEventSubscriptions.TryRemove(hash, out _);
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Maps an input definition (key combination, joystick button, etc.) to a client event.
        /// </summary>
        /// <param name="inputGroupId">The input group ID.</param>
        /// <param name="inputDefinition">The input definition string (e.g., "VK_LCONTROL+A", "joystick:0:button:0").</param>
        /// <param name="downEventId">The event ID triggered on key/button down.</param>
        /// <param name="downValue">Optional value returned when down event occurs.</param>
        /// <param name="upEventId">Optional event ID triggered on key/button up.</param>
        /// <param name="upValue">Optional value returned when up event occurs.</param>
        /// <param name="maskable">Whether the event can be masked from lower priority clients.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous mapping operation.</returns>
        /// <exception cref="SimConnectException">Thrown when the mapping fails.</exception>
        public async Task MapInputEventToClientEventAsync(
            uint inputGroupId,
            string inputDefinition,
            uint downEventId,
            uint downValue = 0,
            uint? upEventId = null,
            uint upValue = 0,
            bool maskable = false,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(inputDefinition);

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_MapInputEventToClientEvent_EX1(
                    this.simConnectHandle,
                    inputGroupId,
                    inputDefinition,
                    downEventId,
                    downValue,
                    upEventId ?? uint.MaxValue, // SIMCONNECT_UNUSED
                    upValue,
                    maskable);

                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to map input event to client event: {(SimConnectError)result}", (SimConnectError)result);
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Transmits a client event with an optional 32-bit data value to the simulation or other SimConnect clients.
        /// </summary>
        /// <param name="objectId">Target object ID (use 0 for user aircraft or broadcast semantics).</param>
        /// <param name="eventId">The mapped client event ID.</param>
        /// <param name="data">Optional data value (DWORD) required by some events.</param>
        /// <param name="groupId">Notification group ID or priority if <paramref name="options"/> includes <see cref="SimConnectEventOptions.GroupIdIsPriority"/>.</param>
        /// <param name="options">Event transmission options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimConnectException">Thrown if native call fails.</exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task TransmitClientEventAsync(
            uint objectId,
            uint eventId,
            uint data = 0,
            uint groupId = 0,
            SimConnectEventOptions options = SimConnectEventOptions.None,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_TransmitClientEvent(
                    this.simConnectHandle,
                    objectId,
                    eventId,
                    data,
                    groupId,
                    (uint)options);

                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to transmit client event: {(SimConnectError)result}", (SimConnectError)result);
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Transmits a client event (EX1 variant) with up to five 32-bit data parameters.
        /// </summary>
        /// <param name="objectId">Target object ID.</param>
        /// <param name="eventId">The mapped client event ID.</param>
        /// <param name="groupId">Notification group ID or priority when using <see cref="SimConnectEventOptions.GroupIdIsPriority"/>.</param>
        /// <param name="options">Event transmission options.</param>
        /// <param name="data0">First data parameter.</param>
        /// <param name="data1">Second data parameter.</param>
        /// <param name="data2">Third data parameter.</param>
        /// <param name="data3">Fourth data parameter.</param>
        /// <param name="data4">Fifth data parameter.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimConnectException">Thrown if native call fails.</exception>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task TransmitClientEventEx1Async(
            uint objectId,
            uint eventId,
            uint groupId,
            SimConnectEventOptions options = SimConnectEventOptions.None,
            uint data0 = 0,
            uint data1 = 0,
            uint data2 = 0,
            uint data3 = 0,
            uint data4 = 0,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this.disposed, nameof(InputEventManager));
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(
                () =>
            {
                var result = SimConnectNative.SimConnect_TransmitClientEvent_EX1(
                    this.simConnectHandle,
                    objectId,
                    eventId,
                    groupId,
                    (uint)options,
                    data0,
                    data1,
                    data2,
                    data3,
                    data4);

                if (result != (int)SimConnectError.None)
                {
                    throw new SimConnectException($"Failed to transmit client event (EX1): {(SimConnectError)result}", (SimConnectError)result);
                }
            },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Disposes the InputEventManager and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                // Cancel all pending requests
                foreach (var tcs in this.pendingEnumerationRequests.Values)
                {
                    tcs.TrySetCanceled();
                }

                foreach (var tcs in this.pendingParameterRequests.Values)
                {
                    tcs.TrySetCanceled();
                }

                foreach (var tcs in this.pendingGetRequests.Values)
                {
                    tcs.TrySetCanceled();
                }

                this.pendingEnumerationRequests.Clear();
                this.pendingParameterRequests.Clear();
                this.pendingGetRequests.Clear();
                this.inputEventSubscriptions.Clear();

                this.disposed = true;
            }
        }

        /// <summary>
        /// Processes received SimConnect data for input events.
        /// </summary>
        /// <param name="ppData">Pointer to the received data.</param>
        /// <param name="cbData">Size of the received data.</param>
        internal void ProcessReceivedData(IntPtr ppData, uint cbData)
        {
            try
            {
                var recv = Marshal.PtrToStructure<SimConnectRecv>(ppData);

                switch ((SimConnectRecvId)recv.Id)
                {
                    case SimConnectRecvId.EnumerateInputEvents:
                        this.ProcessEnumerateInputEvents(ppData);
                        break;
                    case SimConnectRecvId.EnumerateInputEventParams:
                        this.ProcessEnumerateInputEventParams(ppData);
                        break;
                    case SimConnectRecvId.GetInputEvent:
                        this.ProcessGetInputEvent(ppData);
                        break;
                    case SimConnectRecvId.SubscribeInputEvent:
                        this.ProcessSubscribeInputEvent(ppData);
                        break;
                }
            }
            catch (Exception ex)
            {
                SimConnectLogger.Error("Error processing input event data", ex);
            }
        }

        private uint GetNextRequestId()
        {
            lock (this.lockObject)
            {
                return this.nextRequestId++;
            }
        }

        private void ProcessEnumerateInputEvents(IntPtr ppData)
        {
            try
            {
                var recvEnumerate = Marshal.PtrToStructure<SimConnectRecvEnumerateInputEvents>(ppData);

                // Parse the rgData array from the received data
                var descriptors = new List<InputEventDescriptor>();

                if (recvEnumerate.ArraySize > 0)
                {
                    // Calculate the offset to the rgData array (after the header structure)
                    var headerSize = Marshal.SizeOf<SimConnectRecvEnumerateInputEvents>();
                    var dataPtr = IntPtr.Add(ppData, headerSize);
                    var availableDataSize = recvEnumerate.Size - (uint)headerSize;

                    // Calculate actual item size from available data
                    var actualItemSize = availableDataSize / recvEnumerate.ArraySize;

                    // Try to parse with the actual structure size
                    var currentPtr = dataPtr;

                    // Process all input events
                    for (int i = 0; i < recvEnumerate.ArraySize; i++)
                    {
                        try
                        {
                            var remainingBytes = availableDataSize - (uint)((long)currentPtr - (long)dataPtr);
                            if (remainingBytes < actualItemSize)
                            {
                                break;
                            }

                            // Try reading as fixed-size structure based on observed data
                            // Structure appears to be: 64-byte name + 8-byte hash + 4-byte type = 76 bytes total

                            // Read name (64 bytes - confirmed from hex dump)
                            var nameBytes = new byte[64];
                            Marshal.Copy(currentPtr, nameBytes, 0, 64);
                            var nullIndex = Array.IndexOf(nameBytes, (byte)0);
                            var nameLength = nullIndex >= 0 ? nullIndex : nameBytes.Length;
                            var name = System.Text.Encoding.UTF8.GetString(nameBytes, 0, nameLength);

                            currentPtr = IntPtr.Add(currentPtr, 64);

                            // Read hash (8 bytes - try as 64-bit first, then 32-bit if needed)
                            var hash = (ulong)Marshal.ReadInt64(currentPtr);
                            if (hash == 0)
                            {
                                // Try reading as 32-bit hash + 4-byte padding
                                currentPtr = IntPtr.Add(currentPtr, -8);
                                hash = (ulong)Marshal.ReadInt32(currentPtr);
                                currentPtr = IntPtr.Add(currentPtr, 8);
                            }
                            else
                            {
                                currentPtr = IntPtr.Add(currentPtr, 8);
                            }

                            // Read type (4 bytes) - use SimConnectInputEventType instead of SimConnectDataType
                            var typeValue = Marshal.ReadInt32(currentPtr);
                            var inputEventType = Enum.IsDefined(typeof(SimConnectInputEventType), typeValue)
                                ? (SimConnectInputEventType)typeValue
                                : SimConnectInputEventType.DoubleValue; // Default to None if invalid

                            // Convert to SimConnectDataType for the descriptor
                            var type = inputEventType switch
                            {
                                SimConnectInputEventType.DoubleValue => SimConnectDataType.FloatDouble,
                                SimConnectInputEventType.StringValue => SimConnectDataType.String128,
                                _ => SimConnectDataType.FloatDouble, // Default fallback
                            };

                            currentPtr = IntPtr.Add(currentPtr, 4);

                            // For 76-byte structure, we might have additional padding or node names
                            // Let's skip any remaining bytes to get to the next item
                            var nodeNames = string.Empty; // For now, assume no node names in this structure
                            currentPtr = IntPtr.Add(dataPtr, (int)(i + 1) * (int)actualItemSize);

                            // Create descriptor only if we have valid data
                            if (!string.IsNullOrEmpty(name))
                            {
                                var descriptor = new InputEventDescriptor(name, hash, type, nodeNames);
                                descriptors.Add(descriptor);
                            }
                        }
                        catch (Exception itemEx)
                        {
                            SimConnectLogger.Warning($"Error parsing input event item {i}: {itemEx.Message}");
                            break; // Stop processing if we hit an error with an individual item
                        }
                    }
                }

                if (this.pendingEnumerationRequests.TryRemove(recvEnumerate.RequestId, out var tcs))
                {
                    var result = descriptors.ToArray();
                    tcs.TrySetResult(result);

                    // Force immediate return to prevent any subsequent processing from interfering
                    return;
                }
            }
            catch (Exception ex)
            {
                SimConnectLogger.Error("Error processing enumerate input events", ex);

                // Try to complete any pending request with an empty array to prevent hanging
                if (!this.pendingEnumerationRequests.IsEmpty)
                {
                    var pendingRequest = this.pendingEnumerationRequests.FirstOrDefault();
                    if (pendingRequest.Value != null)
                    {
                        this.pendingEnumerationRequests.TryRemove(pendingRequest.Key, out _);
                        pendingRequest.Value.TrySetResult(Array.Empty<InputEventDescriptor>());
                    }
                }
            }
        }

        private void ProcessEnumerateInputEventParams(IntPtr ppData)
        {
            try
            {
                var recvParams = Marshal.PtrToStructure<SimConnectRecvEnumerateInputEventParams>(ppData);

                if (this.pendingParameterRequests.TryRemove(recvParams.RequestId, out var tcs))
                {
                    tcs.TrySetResult(recvParams.Value ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                SimConnectLogger.Error("Error processing enumerate input event params", ex);
            }
        }

        private void ProcessGetInputEvent(IntPtr ppData)
        {
            try
            {
                var recvGet = Marshal.PtrToStructure<SimConnectRecvGetInputEventHeader>(ppData);
                int headerSize = Marshal.SizeOf<SimConnectRecvGetInputEventHeader>();
                int totalSize = checked((int)recvGet.Size);
                int payloadSize = totalSize - headerSize;
                System.Diagnostics.Debug.WriteLine($"[InputEventManager] Payload size: {payloadSize}");
                IntPtr pValue = IntPtr.Add(ppData, headerSize);

                // Extract value based on the type
                object value;
                switch (recvGet.Type)
                {
                    case SimConnectInputEventType.DoubleValue:
                        value = Marshal.PtrToStructure<double>(pValue);
                        break;
                    case SimConnectInputEventType.StringValue:
                        byte[] buf = new byte[payloadSize];
                        Marshal.Copy(pValue, buf, 0, buf.Length);
                        int nul = Array.IndexOf(buf, (byte)0);
                        int len = nul >= 0 ? nul : buf.Length;
                        value = Encoding.ASCII.GetString(buf, 0, len);
                        break;
                    default:
                        byte[] defBuf = new byte[payloadSize];
                        Marshal.Copy(pValue, defBuf, 0, defBuf.Length);
                        value = defBuf;
                        value = BitConverter.ToDouble(defBuf, 0);
                        break;
                }

                var inputEventValue = new InputEventValue
                {
                    Type = recvGet.Type,
                    Value = value,
                };

                if (this.pendingGetRequests.TryRemove(recvGet.RequestId, out var tcs))
                {
                    tcs.TrySetResult(inputEventValue);
                }
            }
            catch (Exception ex)
            {
                SimConnectLogger.Error("Error processing get input event", ex);
            }
        }

        private void ProcessSubscribeInputEvent(IntPtr ppData)
        {
            try
            {
                var recvSubscribe = Marshal.PtrToStructure<SimConnectRecvSubscribeInputEvent>(ppData);

                // Compute pointer to the payload (value) directly after the header
                int headerSize = Marshal.SizeOf<SimConnectRecvSubscribeInputEvent>();
                int totalSize = checked((int)recvSubscribe.Size);
                int payloadSize = totalSize - headerSize;
                IntPtr pValue = IntPtr.Add(ppData, headerSize);

                // Extract value based on the type, mirroring GetInputEvent
                object value;
                switch (recvSubscribe.Type)
                {
                    case SimConnectInputEventType.DoubleValue:
                        value = payloadSize >= sizeof(double)
                            ? Marshal.PtrToStructure<double>(pValue)
                            : 0d;
                        break;
                    case SimConnectInputEventType.StringValue:
                        if (payloadSize > 0)
                        {
                            byte[] buf = new byte[payloadSize];
                            Marshal.Copy(pValue, buf, 0, buf.Length);
                            int nul = Array.IndexOf(buf, (byte)0);
                            int len = nul >= 0 ? nul : buf.Length;
                            value = Encoding.ASCII.GetString(buf, 0, len);
                        }
                        else
                        {
                            value = string.Empty;
                        }

                        break;
                    default:
                        value = null!;
                        break;
                }

                var inputEventValue = new InputEventValue
                {
                    Hash = recvSubscribe.Hash,
                    Type = recvSubscribe.Type,
                    Value = value,
                };

                // Invoke specific callback if registered
                if (this.inputEventSubscriptions.TryGetValue(recvSubscribe.Hash, out var callback))
                {
                    callback(inputEventValue);
                }

                // Also raise the general event
                this.InputEventChanged?.Invoke(this, new InputEventChangedEventArgs(inputEventValue));
            }
            catch (Exception ex)
            {
                SimConnectLogger.Error("Error processing subscribe input event", ex);
            }
        }
    }
}
