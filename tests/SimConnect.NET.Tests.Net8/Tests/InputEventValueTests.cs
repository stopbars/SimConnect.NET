// <copyright file="InputEventValueTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using SimConnect.NET;
using SimConnect.NET.InputEvents;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Tests for input event value types, descriptors, and data structures.
    /// </summary>
    public class InputEventValueTests : ISimConnectTest
    {
        /// <inheritdoc/>
        public string Name => "Input Event Values";

        /// <inheritdoc/>
        public string Description => "Tests input event value types, descriptors, mappings, and data structure operations";

        /// <inheritdoc/>
        public string Category => "InputEvents";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                // Test InputEventValue operations
                if (!await TestInputEventValueOperations())
                {
                    return false;
                }

                // Test InputEventDescriptor operations
                if (!await TestInputEventDescriptorOperations())
                {
                    return false;
                }

                // Test InputEventMapping operations
                if (!await TestInputEventMappingOperations())
                {
                    return false;
                }

                // Test InputGroup operations
                if (!await TestInputGroupOperations())
                {
                    return false;
                }

                // Test ControllerInfo operations
                if (!await TestControllerInfoOperations())
                {
                    return false;
                }

                // Test InputEventChangedEventArgs operations
                if (!await TestInputEventChangedEventArgsOperations())
                {
                    return false;
                }

                Console.WriteLine("   ✅ All input event value tests successful");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ Input event value test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Input event value test failed: {ex.Message}");
                return false;
            }
        }

        private static Task<bool> TestInputEventValueOperations()
        {
            Console.WriteLine("   🔍 Testing InputEventValue operations...");

            // Test double value creation and operations
            var doubleValue = new InputEventValue
            {
                Hash = 12345,
                Type = SimConnectInputEventType.DoubleValue,
                Value = 42.5,
            };

            Console.WriteLine($"      📊 Created double value: {doubleValue}");

            // Test double value extraction
            try
            {
                var extracted = doubleValue.GetDoubleValue();
                Console.WriteLine($"      📊 Extracted double: {extracted}");

                if (Math.Abs(extracted - 42.5) > 0.001)
                {
                    Console.WriteLine("      ❌ Double value extraction failed");
                    return Task.FromResult(false);
                }

                // Test TryGetDoubleValue
                if (doubleValue.TryGetDoubleValue(out var tryExtracted))
                {
                    Console.WriteLine($"      📊 TryGet double: {tryExtracted}");
                    if (Math.Abs(tryExtracted - 42.5) > 0.001)
                    {
                        Console.WriteLine("      ❌ TryGetDoubleValue failed");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    Console.WriteLine("      ❌ TryGetDoubleValue should have succeeded");
                    return Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ Double value operations failed: {ex.Message}");
                return Task.FromResult(false);
            }

            // Test string value creation and operations
            var stringValue = new InputEventValue
            {
                Hash = 67890,
                Type = SimConnectInputEventType.StringValue,
                Value = "Test String Value",
            };

            Console.WriteLine($"      📝 Created string value: {stringValue}");

            try
            {
                var extractedString = stringValue.GetStringValue();
                Console.WriteLine($"      📝 Extracted string: '{extractedString}'");

                if (extractedString != "Test String Value")
                {
                    Console.WriteLine("      ❌ String value extraction failed");
                    return Task.FromResult(false);
                }

                // Test TryGetStringValue
                if (stringValue.TryGetStringValue(out var tryExtractedString))
                {
                    Console.WriteLine($"      📝 TryGet string: '{tryExtractedString}'");
                    if (tryExtractedString != "Test String Value")
                    {
                        Console.WriteLine("      ❌ TryGetStringValue failed");
                        return Task.FromResult(false);
                    }
                }
                else
                {
                    Console.WriteLine("      ❌ TryGetStringValue should have succeeded");
                    return Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ String value operations failed: {ex.Message}");
                return Task.FromResult(false);
            }

            // Test type mismatch handling
            try
            {
                _ = doubleValue.GetStringValue(); // Should throw
                Console.WriteLine("      ❌ Expected exception for type mismatch");
                return Task.FromResult(false);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("      ✅ Correctly threw exception for type mismatch");
            }

            try
            {
                _ = stringValue.GetDoubleValue(); // Should throw
                Console.WriteLine("      ❌ Expected exception for type mismatch");
                return Task.FromResult(false);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("      ✅ Correctly threw exception for type mismatch");
            }

            // Test TryGet with wrong types
            if (doubleValue.TryGetStringValue(out _))
            {
                Console.WriteLine("      ❌ TryGetStringValue should have failed for double type");
                return Task.FromResult(false);
            }

            if (stringValue.TryGetDoubleValue(out _))
            {
                Console.WriteLine("      ❌ TryGetDoubleValue should have failed for string type");
                return Task.FromResult(false);
            }

            Console.WriteLine("      ✅ InputEventValue operations completed successfully");
            return Task.FromResult(true);
        }

        private static Task<bool> TestInputEventDescriptorOperations()
        {
            Console.WriteLine("   🔍 Testing InputEventDescriptor operations...");

            try
            {
                // Test descriptor creation
                var descriptor = new InputEventDescriptor(
                    "TestInputEvent",
                    0x123456789ABCDEF0,
                    SimConnectDataType.FloatDouble,
                    "Node1;Node2;Node3");

                Console.WriteLine($"      📋 Created descriptor: {descriptor}");

                // Verify properties
                if (descriptor.Name != "TestInputEvent")
                {
                    Console.WriteLine($"      ❌ Unexpected name: {descriptor.Name}");
                    return Task.FromResult(false);
                }

                if (descriptor.Hash != 0x123456789ABCDEF0)
                {
                    Console.WriteLine($"      ❌ Unexpected hash: {descriptor.Hash:X}");
                    return Task.FromResult(false);
                }

                if (descriptor.Type != SimConnectDataType.FloatDouble)
                {
                    Console.WriteLine($"      ❌ Unexpected type: {descriptor.Type}");
                    return Task.FromResult(false);
                }

                if (descriptor.NodeNames != "Node1;Node2;Node3")
                {
                    Console.WriteLine($"      ❌ Unexpected node names: {descriptor.NodeNames}");
                    return Task.FromResult(false);
                }

                // Test null handling in constructor
                try
                {
                    _ = new InputEventDescriptor(null!, 0, SimConnectDataType.FloatSingle, "nodes");
                    Console.WriteLine("      ❌ Expected ArgumentNullException for null name");
                    return Task.FromResult(false);
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("      ✅ Correctly threw ArgumentNullException for null name");
                }

                // Test empty node names
                var descriptorEmptyNodes = new InputEventDescriptor("Test", 0, SimConnectDataType.FloatSingle, null!);
                if (descriptorEmptyNodes.NodeNames != string.Empty)
                {
                    Console.WriteLine($"      ❌ Expected empty string for null node names, got: '{descriptorEmptyNodes.NodeNames}'");
                    return Task.FromResult(false);
                }

                Console.WriteLine("      ✅ InputEventDescriptor operations completed successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ InputEventDescriptor test failed: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        private static Task<bool> TestInputEventMappingOperations()
        {
            Console.WriteLine("   🔍 Testing InputEventMapping operations...");

            try
            {
                // Test basic mapping creation
                var basicMapping = new InputEventMapping(
                    "VK_F1",
                    1001,
                    100);

                Console.WriteLine($"      🗺️ Created basic mapping: {basicMapping}");

                // Verify basic properties
                if (basicMapping.InputDefinition != "VK_F1")
                {
                    Console.WriteLine($"      ❌ Unexpected input definition: {basicMapping.InputDefinition}");
                    return Task.FromResult(false);
                }

                if (basicMapping.DownEventId != 1001)
                {
                    Console.WriteLine($"      ❌ Unexpected down event ID: {basicMapping.DownEventId}");
                    return Task.FromResult(false);
                }

                if (basicMapping.DownValue != 100)
                {
                    Console.WriteLine($"      ❌ Unexpected down value: {basicMapping.DownValue}");
                    return Task.FromResult(false);
                }

                if (basicMapping.UpEventId.HasValue)
                {
                    Console.WriteLine($"      ❌ Up event ID should be null: {basicMapping.UpEventId}");
                    return Task.FromResult(false);
                }

                if (basicMapping.Maskable)
                {
                    Console.WriteLine($"      ❌ Maskable should be false by default: {basicMapping.Maskable}");
                    return Task.FromResult(false);
                }

                // Test full mapping creation
                var fullMapping = new InputEventMapping(
                    "VK_LCONTROL+VK_A",
                    2001,
                    200,
                    2002,
                    0,
                    true);

                Console.WriteLine($"      🗺️ Created full mapping: {fullMapping}");

                // Verify full properties
                if (fullMapping.InputDefinition != "VK_LCONTROL+VK_A")
                {
                    Console.WriteLine($"      ❌ Unexpected input definition: {fullMapping.InputDefinition}");
                    return Task.FromResult(false);
                }

                if (fullMapping.DownEventId != 2001)
                {
                    Console.WriteLine($"      ❌ Unexpected down event ID: {fullMapping.DownEventId}");
                    return Task.FromResult(false);
                }

                if (fullMapping.DownValue != 200)
                {
                    Console.WriteLine($"      ❌ Unexpected down value: {fullMapping.DownValue}");
                    return Task.FromResult(false);
                }

                if (!fullMapping.UpEventId.HasValue || fullMapping.UpEventId.Value != 2002)
                {
                    Console.WriteLine($"      ❌ Unexpected up event ID: {fullMapping.UpEventId}");
                    return Task.FromResult(false);
                }

                if (fullMapping.UpValue != 0)
                {
                    Console.WriteLine($"      ❌ Unexpected up value: {fullMapping.UpValue}");
                    return Task.FromResult(false);
                }

                if (!fullMapping.Maskable)
                {
                    Console.WriteLine($"      ❌ Maskable should be true: {fullMapping.Maskable}");
                    return Task.FromResult(false);
                }

                // Test null input definition handling
                try
                {
                    _ = new InputEventMapping(null!, 1000);
                    Console.WriteLine("      ❌ Expected ArgumentNullException for null input definition");
                    return Task.FromResult(false);
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("      ✅ Correctly threw ArgumentNullException for null input definition");
                }

                Console.WriteLine("      ✅ InputEventMapping operations completed successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ InputEventMapping test failed: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        private static Task<bool> TestInputGroupOperations()
        {
            Console.WriteLine("   🔍 Testing InputGroup operations...");

            try
            {
                // Note: InputGroup constructor is internal, so we can't test it directly
                // This test would require integration with InputGroupManager
                Console.WriteLine("      ⚠️ InputGroup constructor is internal - skipping direct tests");
                Console.WriteLine("      ℹ️ InputGroup functionality is tested via InputGroupManager in InputEventTests");

                Console.WriteLine("      ✅ InputGroup operations completed successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ InputGroup test failed: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        private static Task<bool> TestControllerInfoOperations()
        {
            Console.WriteLine("   🔍 Testing ControllerInfo operations...");

            try
            {
                // Test controller info creation
                var controllerInfo = new ControllerInfo(
                    "Xbox Controller",
                    12345,
                    67890,
                    ControllerType.Joystick);

                Console.WriteLine($"      🎮 Created controller info: {controllerInfo}");

                // Verify properties
                if (controllerInfo.DeviceName != "Xbox Controller")
                {
                    Console.WriteLine($"      ❌ Unexpected device name: {controllerInfo.DeviceName}");
                    return Task.FromResult(false);
                }

                if (controllerInfo.DeviceId != 12345)
                {
                    Console.WriteLine($"      ❌ Unexpected device ID: {controllerInfo.DeviceId}");
                    return Task.FromResult(false);
                }

                if (controllerInfo.ProductId != 67890)
                {
                    Console.WriteLine($"      ❌ Unexpected product ID: {controllerInfo.ProductId}");
                    return Task.FromResult(false);
                }

                if (controllerInfo.DeviceType != ControllerType.Joystick)
                {
                    Console.WriteLine($"      ❌ Unexpected device type: {controllerInfo.DeviceType}");
                    return Task.FromResult(false);
                }

                // Test null device name handling
                try
                {
                    _ = new ControllerInfo(null!, 0, 0, ControllerType.Keyboard);
                    Console.WriteLine("      ❌ Expected ArgumentNullException for null device name");
                    return Task.FromResult(false);
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("      ✅ Correctly threw ArgumentNullException for null device name");
                }

                // Test different controller types
                var joystickInfo = new ControllerInfo("Flight Stick", 1, 2, ControllerType.Joystick);
                var keyboardInfo = new ControllerInfo("Gaming Keyboard", 3, 4, ControllerType.Keyboard);
                var mouseInfo = new ControllerInfo("Gaming Mouse", 5, 6, ControllerType.Mouse);

                Console.WriteLine($"      🕹️ Joystick: {joystickInfo}");
                Console.WriteLine($"      ⌨️ Keyboard: {keyboardInfo}");
                Console.WriteLine($"      🖱️ Mouse: {mouseInfo}");

                Console.WriteLine("      ✅ ControllerInfo operations completed successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ ControllerInfo test failed: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        private static Task<bool> TestInputEventChangedEventArgsOperations()
        {
            Console.WriteLine("   🔍 Testing InputEventChangedEventArgs operations...");

            try
            {
                // Create test input event value
                var inputEventValue = new InputEventValue
                {
                    Hash = 12345,
                    Type = SimConnectInputEventType.DoubleValue,
                    Value = 42.0,
                };

                // Create event args
                var eventArgs = new InputEventChangedEventArgs(inputEventValue);

                Console.WriteLine($"      📡 Created event args: {eventArgs.InputEventValue}");

                // Verify properties
                if (eventArgs.InputEventValue != inputEventValue)
                {
                    Console.WriteLine("      ❌ InputEventValue property mismatch");
                    return Task.FromResult(false);
                }

                if (eventArgs.Timestamp == default)
                {
                    Console.WriteLine("      ❌ Timestamp should be set");
                    return Task.FromResult(false);
                }

                // Verify timestamp is recent (within last few seconds)
                var timeDiff = DateTime.UtcNow - eventArgs.Timestamp;
                if (timeDiff.TotalSeconds > 5)
                {
                    Console.WriteLine($"      ❌ Timestamp seems too old: {timeDiff.TotalSeconds} seconds");
                    return Task.FromResult(false);
                }

                Console.WriteLine($"      🕐 Event timestamp: {eventArgs.Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC");

                // Test null input event value handling
                try
                {
                    _ = new InputEventChangedEventArgs(null!);
                    Console.WriteLine("      ❌ Expected ArgumentNullException for null input event value");
                    return Task.FromResult(false);
                }
                catch (ArgumentNullException)
                {
                    Console.WriteLine("      ✅ Correctly threw ArgumentNullException for null input event value");
                }

                Console.WriteLine("      ✅ InputEventChangedEventArgs operations completed successfully");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ InputEventChangedEventArgs test failed: {ex.Message}");
                return Task.FromResult(false);
            }
        }
    }
}
