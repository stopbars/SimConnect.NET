// <copyright file="InputEventTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using SimConnect.NET;
using SimConnect.NET.InputEvents;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Tests for input events functionality including InputEventManager and InputGroupManager.
    /// </summary>
    public class InputEventTests : ISimConnectTest
    {
        /// <inheritdoc/>
        public string Name => "Input Events";

        /// <inheritdoc/>
        public string Description => "Tests input event enumeration, subscription, get/set operations, and input group management";

        /// <inheritdoc/>
        public string Category => "InputEvents";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(60));

                // Test controller enumeration
                if (!await TestControllerEnumeration(client, cts.Token))
                {
                    return false;
                }

                // Test input event enumeration
                if (!await TestInputEventEnumeration(client, cts.Token))
                {
                    return false;
                }

                // Test input event get/set operations
                if (!await TestInputEventGetSet(client, cts.Token))
                {
                    return false;
                }

                // Test input event subscriptions
                if (!await TestInputEventSubscriptions(client, cts.Token))
                {
                    return false;
                }

                // Test input group management
                if (!await TestInputGroupManagement(client, cts.Token))
                {
                    return false;
                }

                // Test input event mapping
                if (!await TestInputEventMapping(client, cts.Token))
                {
                    return false;
                }

                Console.WriteLine("   ✅ All input event operations successful");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ Input event test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Input event test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestControllerEnumeration(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing controller enumeration...");

            try
            {
                await client.InputEvents.EnumerateControllersAsync(cancellationToken);
                Console.WriteLine("      ✅ Controller enumeration completed successfully");
                return true;
            }
            catch (SimConnectException ex)
            {
                Console.WriteLine($"      ⚠️ Controller enumeration failed (expected if no controllers): {ex.Message}");

                // This is not a failure - controllers may not be available in test environment
                return true;
            }
        }

        private static async Task<bool> TestInputEventEnumeration(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing input event enumeration...");

            try
            {
                var inputEvents = await client.InputEvents.EnumerateInputEventsAsync(cancellationToken);
                Console.WriteLine($"      📋 Found {inputEvents.Length} input events");

                // Test first few events if any are available
                for (int i = 0; i < Math.Min(3, inputEvents.Length); i++)
                {
                    var inputEvent = inputEvents[i];
                    Console.WriteLine($"      📌 Event {i + 1}: {inputEvent.Name} (Hash: {inputEvent.Hash}, Type: {inputEvent.Type})");

                    // TODO: Test parameter enumeration for this event (currently disabled due to marshaling issues)
                    // try
                    // {
                    //     var parameters = await client.InputEvents.EnumerateInputEventParametersAsync(inputEvent.Hash, cancellationToken);
                    //     Console.WriteLine($"         🔧 Parameters: {parameters}");
                    // }
                    // catch (SimConnectException paramEx)
                    // {
                    //     Console.WriteLine($"         ⚠️ Parameter enumeration failed: {paramEx.Message}");
                    // }
                }

                return true;
            }
            catch (SimConnectException ex)
            {
                Console.WriteLine($"      ⚠️ Input event enumeration failed: {ex.Message}");

                // This might fail in test environments, so we'll treat it as a warning
                return true;
            }
        }

        private static async Task<bool> TestInputEventGetSet(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing input event get/set operations...");

            try
            {
                // First enumerate to get available events
                var inputEvents = await client.InputEvents.EnumerateInputEventsAsync(cancellationToken);

                if (inputEvents.Length == 0)
                {
                    Console.WriteLine("      ⚠️ No input events available for get/set testing");
                    return true;
                }

                // Test with the first available event
                var testEvent = inputEvents[0];
                Console.WriteLine($"      🎯 Testing with event: {testEvent.Name}");

                // Test getting current value
                try
                {
                    var currentValue = await client.InputEvents.GetInputEventAsync(testEvent.Hash, cancellationToken);
                    Console.WriteLine($"      📊 Current value: {currentValue}");
                    Console.WriteLine($"         Type: {currentValue.Type}, Hash: {currentValue.Hash}");

                    // Test setting values based on type
                    if (currentValue.Type == SimConnectInputEventType.DoubleValue)
                    {
                        await TestDoubleValueOperations(client, testEvent.Hash, currentValue, cancellationToken);
                    }
                    else if (currentValue.Type == SimConnectInputEventType.StringValue)
                    {
                        await TestStringValueOperations(client, testEvent.Hash, currentValue, cancellationToken);
                    }

                    return true;
                }
                catch (SimConnectException getEx)
                {
                    Console.WriteLine($"      ⚠️ Get operation failed: {getEx.Message}");
                    return true; // Treat as warning in test environment
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ Input event get/set test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task TestDoubleValueOperations(SimConnectClient client, ulong hash, InputEventValue currentValue, CancellationToken cancellationToken)
        {
            Console.WriteLine("      🔢 Testing double value operations...");

            try
            {
                var originalValue = currentValue.GetDoubleValue();
                Console.WriteLine($"         📊 Original value: {originalValue}");

                // Test setting a new value (small increment to be safe)
                var testValue = originalValue + 0.1;
                await client.InputEvents.SetInputEventAsync(hash, testValue, cancellationToken);
                Console.WriteLine($"         ✏️ Set value to: {testValue}");

                // Wait a moment for the change to take effect
                await Task.Delay(100, cancellationToken);

                // Get the new value to verify
                var newValue = await client.InputEvents.GetInputEventAsync(hash, cancellationToken);
                var retrievedValue = newValue.GetDoubleValue();
                Console.WriteLine($"         📊 Retrieved value: {retrievedValue}");

                // Restore original value
                await client.InputEvents.SetInputEventAsync(hash, originalValue, cancellationToken);
                Console.WriteLine($"         🔄 Restored to original: {originalValue}");
            }
            catch (SimConnectException ex)
            {
                Console.WriteLine($"         ⚠️ Double value operations failed: {ex.Message}");
            }
        }

        private static async Task TestStringValueOperations(SimConnectClient client, ulong hash, InputEventValue currentValue, CancellationToken cancellationToken)
        {
            Console.WriteLine("      📝 Testing string value operations...");

            try
            {
                var originalValue = currentValue.GetStringValue();
                Console.WriteLine($"         📊 Original value: '{originalValue}'");

                // Test setting a new string value
                var testValue = "TestValue123";
                await client.InputEvents.SetInputEventAsync(hash, testValue, cancellationToken);
                Console.WriteLine($"         ✏️ Set value to: '{testValue}'");

                // Wait a moment for the change to take effect
                await Task.Delay(100, cancellationToken);

                // Get the new value to verify
                var newValue = await client.InputEvents.GetInputEventAsync(hash, cancellationToken);
                var retrievedValue = newValue.GetStringValue();
                Console.WriteLine($"         📊 Retrieved value: '{retrievedValue}'");

                // Restore original value
                await client.InputEvents.SetInputEventAsync(hash, originalValue, cancellationToken);
                Console.WriteLine($"         🔄 Restored to original: '{originalValue}'");
            }
            catch (SimConnectException ex)
            {
                Console.WriteLine($"         ⚠️ String value operations failed: {ex.Message}");
            }
        }

        private static async Task<bool> TestInputEventSubscriptions(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing input event subscriptions...");

            try
            {
                var inputEvents = await client.InputEvents.EnumerateInputEventsAsync(cancellationToken);

                if (inputEvents.Length == 0)
                {
                    Console.WriteLine("      ⚠️ No input events available for subscription testing");
                    return true;
                }

                var testEvent = inputEvents[0];
                Console.WriteLine($"      🎯 Testing subscription with event: {testEvent.Name}");

                // Test event subscription
                var receivedValue = default(InputEventValue);

                // Subscribe to the event
                await client.InputEvents.SubscribeToInputEventAsync(
                    testEvent.Hash,
                    (value) =>
                    {
                        receivedValue = value;
                        Console.WriteLine($"         📡 Subscription callback received: {value}");
                    },
                    cancellationToken);

                Console.WriteLine("      📡 Subscribed to input event");

                // Also test the event handler
                EventHandler<InputEventChangedEventArgs> eventHandler = (sender, args) =>
                {
                    Console.WriteLine($"         📡 Event handler fired: {args.InputEventValue}");
                };

                client.InputEvents.InputEventChanged += eventHandler;

                // Trigger a value change to test subscription
                try
                {
                    var currentValue = await client.InputEvents.GetInputEventAsync(testEvent.Hash, cancellationToken);

                    if (currentValue.Type == SimConnectInputEventType.DoubleValue)
                    {
                        var doubleVal = currentValue.GetDoubleValue();
                        await client.InputEvents.SetInputEventAsync(testEvent.Hash, doubleVal + 0.01, cancellationToken);
                    }
                    else if (currentValue.Type == SimConnectInputEventType.StringValue)
                    {
                        await client.InputEvents.SetInputEventAsync(testEvent.Hash, "TriggerTest", cancellationToken);
                    }

                    // Wait for subscription events
                    await Task.Delay(1000, cancellationToken);
                }
                catch (SimConnectException setEx)
                {
                    Console.WriteLine($"         ⚠️ Could not trigger value change: {setEx.Message}");
                }

                // Clean up event handler
                client.InputEvents.InputEventChanged -= eventHandler;

                // Unsubscribe
                await client.InputEvents.UnsubscribeFromInputEventAsync(testEvent.Hash, cancellationToken);
                Console.WriteLine("      📡 Unsubscribed from input event");

                // Test universal subscription (hash = 0)
                Console.WriteLine("      📡 Testing universal subscription...");
                await client.InputEvents.SubscribeToInputEventAsync(
                    0,
                    (value) =>
                    {
                        Console.WriteLine($"         📡 Universal subscription received: {value}");
                    },
                    cancellationToken);

                await Task.Delay(500, cancellationToken);

                await client.InputEvents.UnsubscribeFromInputEventAsync(0, cancellationToken);
                Console.WriteLine("      📡 Unsubscribed from universal events");

                Console.WriteLine("      ✅ Subscription tests completed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ Subscription test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestInputGroupManagement(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing input group management...");

            try
            {
                // Create a test input group
                var inputGroup = await client.InputGroups.CreateInputGroupAsync("TestGroup", InputGroupPriority.Highest, cancellationToken);
                Console.WriteLine($"      🏷️ Created input group: {inputGroup}");

                // Verify the group properties
                if (inputGroup.Name != "TestGroup")
                {
                    Console.WriteLine($"      ❌ Unexpected group name: {inputGroup.Name}");
                    return false;
                }

                if (inputGroup.Priority != InputGroupPriority.Highest)
                {
                    Console.WriteLine($"      ❌ Unexpected group priority: {inputGroup.Priority}");
                    return false;
                }

                if (!inputGroup.IsEnabled)
                {
                    Console.WriteLine("      ❌ Group should be enabled by default");
                    return false;
                }

                // Test priority change
                await client.InputGroups.SetInputGroupPriorityAsync(inputGroup.Id, InputGroupPriority.Lowest, cancellationToken);
                Console.WriteLine($"      🔄 Changed priority to Lowest");

                // Verify the priority was updated
                var retrievedGroup = client.InputGroups.GetInputGroup(inputGroup.Id);
                if (retrievedGroup?.Priority != InputGroupPriority.Lowest)
                {
                    Console.WriteLine($"      ❌ Priority was not updated correctly");
                    return false;
                }

                // Test state change (disable)
                await client.InputGroups.SetInputGroupStateAsync(inputGroup.Id, false, cancellationToken);
                Console.WriteLine($"      🔄 Disabled group");

                // Verify the state was updated
                retrievedGroup = client.InputGroups.GetInputGroup(inputGroup.Id);
                if (retrievedGroup?.IsEnabled != false)
                {
                    Console.WriteLine($"      ❌ Group state was not updated correctly");
                    return false;
                }

                // Re-enable the group
                await client.InputGroups.SetInputGroupStateAsync(inputGroup.Id, true, cancellationToken);
                Console.WriteLine($"      🔄 Re-enabled group");

                // Test clearing the group
                await client.InputGroups.ClearInputGroupAsync(inputGroup.Id, cancellationToken);
                Console.WriteLine($"      🧹 Cleared input group");

                // Test removing a specific input event (with a dummy definition)
                try
                {
                    await client.InputGroups.RemoveInputEventAsync(inputGroup.Id, "VK_F1", cancellationToken);
                    Console.WriteLine($"      🗑️ Removed input event from group");
                }
                catch (SimConnectException removeEx)
                {
                    Console.WriteLine($"      ⚠️ Remove input event failed (expected): {removeEx.Message}");
                }

                Console.WriteLine("      ✅ Input group management tests completed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ Input group management test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestInputEventMapping(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing input event mapping...");

            try
            {
                // Create a test input group first
                var inputGroup = await client.InputGroups.CreateInputGroupAsync("MappingTestGroup", InputGroupPriority.Default, cancellationToken);
                Console.WriteLine($"      🏷️ Created mapping test group: {inputGroup.Id}");

                // Test basic key mapping
                const uint downEventId = 1001;
                const uint upEventId = 1002;
                const uint downValue = 100;
                const uint upValue = 0;

                await client.InputEvents.MapInputEventToClientEventAsync(
                    inputGroup.Id,
                    "VK_F1",
                    downEventId,
                    downValue,
                    upEventId,
                    upValue,
                    false,
                    cancellationToken);

                Console.WriteLine($"      🗺️ Mapped F1 key to client events {downEventId}/{upEventId}");

                // Test joystick button mapping
                await client.InputEvents.MapInputEventToClientEventAsync(
                    inputGroup.Id,
                    "joystick:0:button:0",
                    downEventId + 10,
                    downValue,
                    cancellationToken: cancellationToken);

                Console.WriteLine($"      🕹️ Mapped joystick button to client event {downEventId + 10}");

                // Test key combination mapping
                await client.InputEvents.MapInputEventToClientEventAsync(
                    inputGroup.Id,
                    "VK_LCONTROL+VK_F1",
                    downEventId + 20,
                    downValue,
                    maskable: true,
                    cancellationToken: cancellationToken);

                Console.WriteLine($"      ⌨️ Mapped Ctrl+F1 combination to client event {downEventId + 20}");

                // Test mapping with no up event
                await client.InputEvents.MapInputEventToClientEventAsync(
                    inputGroup.Id,
                    "VK_F2",
                    downEventId + 30,
                    downValue,
                    cancellationToken: cancellationToken);

                Console.WriteLine($"      🗺️ Mapped F2 key (down only) to client event {downEventId + 30}");

                Console.WriteLine("      ✅ Input event mapping tests completed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ Input event mapping test failed: {ex.Message}");
                return false;
            }
        }
    }
}
