// <copyright file="AIObjectTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using SimConnect.NET;
using SimConnect.NET.AI;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Tests for AI object management functionality.
    /// </summary>
    public class AIObjectTests : ISimConnectTest
    {
        private const string TestSimObjectModel = "CoffeeCup";

        /// <inheritdoc/>
        public string Name => "AI Object Management";

        /// <inheritdoc/>
        public string Description => "Tests creation, tracking, and removal of AI simulation objects";

        /// <inheritdoc/>
        public string Category => "AI Objects";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                // Test single object creation/removal
                if (!await TestSingleObjectLifecycle(client, cts.Token))
                {
                    return false;
                }

                // Test multiple objects
                if (!await TestMultipleObjects(client, cts.Token))
                {
                    return false;
                }

                // Test object tracking
                if (!await TestObjectTracking(client, cts.Token))
                {
                    return false;
                }

                Console.WriteLine("   ✅ All AI object operations successful");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ AI object test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ AI object test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestSingleObjectLifecycle(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing single AI object lifecycle...");

            // Get current position to place object nearby
            var currentLat = await client.SimVars.GetAsync<double>("PLANE LATITUDE", "degrees", cancellationToken: cancellationToken);
            var currentLon = await client.SimVars.GetAsync<double>("PLANE LONGITUDE", "degrees", cancellationToken: cancellationToken);
            var currentAlt = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet", cancellationToken: cancellationToken);

            var position = new SimConnectDataInitPosition
            {
                Latitude = currentLat, // Slightly offset from aircraft
                Longitude = currentLon,
                Altitude = currentAlt,
                Heading = 90,
                OnGround = 1,
                Airspeed = 0,
            };

            Console.WriteLine($"      🎯 Creating AI object at {position.Latitude:F6}, {position.Longitude:F6}");

            var aiObject = await client.AIObjects.CreateObjectAsync(TestSimObjectModel, position, "Test Object", cancellationToken);
            Console.WriteLine($"      ✅ AI Object created with ID: {aiObject.ObjectId}");

            if (!aiObject.IsActive)
            {
                Console.WriteLine("   ❌ Created object should be active");
                return false;
            }

            if (client.AIObjects.ActiveObjectCount != 1)
            {
                Console.WriteLine($"   ❌ Expected 1 active object after create, got {client.AIObjects.ActiveObjectCount}");
                return false;
            }

            if (!ValidateCreatedObjectMetadata(aiObject, TestSimObjectModel, position))
            {
                return false;
            }

            if (!ValidateTypeLookup(client, TestSimObjectModel, aiObject, expectedCount: 1))
            {
                return false;
            }

            // Wait a moment, then remove it
            await Task.Delay(1000, cancellationToken);
            await client.AIObjects.RemoveObjectAsync(aiObject, cancellationToken);
            Console.WriteLine($"      ✅ AI Object removed");

            if (aiObject.IsActive)
            {
                Console.WriteLine("   ❌ Removed object should not be active");
                return false;
            }

            if (client.AIObjects.ActiveObjectCount != 0)
            {
                Console.WriteLine($"   ❌ Expected 0 active objects after remove, got {client.AIObjects.ActiveObjectCount}");
                return false;
            }

            if (!ValidateTypeLookup(client, TestSimObjectModel, aiObject, expectedCount: 0))
            {
                return false;
            }

            return true;
        }

        private static async Task<bool> TestMultipleObjects(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing multiple AI objects...");

            var currentLat = await client.SimVars.GetAsync<double>("PLANE LATITUDE", "degrees", cancellationToken: cancellationToken);
            var currentLon = await client.SimVars.GetAsync<double>("PLANE LONGITUDE", "degrees", cancellationToken: cancellationToken);

            var objects = new List<SimObject>();

            try
            {
                // Create 3 objects
                for (int i = 0; i < 3; i++)
                {
                    var position = new SimConnectDataInitPosition
                    {
                        Latitude = currentLat + (0.001 * (double)(i + 1)),
                        Longitude = currentLon + (0.001 * (double)(i + 1)),
                        Altitude = 100,
                        Heading = 90.0 + ((double)i * 30.0),
                        OnGround = 1,
                        Airspeed = 0,
                    };

                    var obj = await client.AIObjects.CreateObjectAsync(TestSimObjectModel, position, $"Test Object {i}", cancellationToken);
                    objects.Add(obj);
                    Console.WriteLine($"      ✅ Created object {i + 1} with ID: {obj.ObjectId}");

                    if (client.AIObjects.ActiveObjectCount != objects.Count)
                    {
                        Console.WriteLine($"   ❌ Expected {objects.Count} active objects after create, got {client.AIObjects.ActiveObjectCount}");
                        return false;
                    }

                    if (!ValidateCreatedObjectMetadata(obj, TestSimObjectModel, position))
                    {
                        return false;
                    }
                }

                Console.WriteLine($"      📊 Created {objects.Count} objects");

                // Verify they're all tracked
                if (client.AIObjects.ActiveObjectCount != objects.Count)
                {
                    Console.WriteLine($"   ❌ Expected {objects.Count} active objects, got {client.AIObjects.ActiveObjectCount}");
                    return false;
                }

                if (!ValidateTypeLookup(client, TestSimObjectModel, objects, expectedCount: objects.Count))
                {
                    return false;
                }

                // Remove all objects
                await client.AIObjects.RemoveAllObjectsAsync(cancellationToken);
                Console.WriteLine("      ✅ All objects removed");

                // Verify cleanup
                if (client.AIObjects.ActiveObjectCount != 0)
                {
                    Console.WriteLine($"   ❌ Expected 0 active objects after cleanup, got {client.AIObjects.ActiveObjectCount}");
                    return false;
                }

                if (!ValidateTypeLookup(client, TestSimObjectModel, objects, expectedCount: 0))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                // Cleanup on failure
                try
                {
                    await client.AIObjects.RemoveAllObjectsAsync(cancellationToken);
                }
                catch
                {
                    // Ignore cleanup errors
                }

                throw;
            }
        }

        private static async Task<bool> TestObjectTracking(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing object tracking...");

            var currentLat = await client.SimVars.GetAsync<double>("PLANE LATITUDE", "degrees", cancellationToken: cancellationToken);
            var currentLon = await client.SimVars.GetAsync<double>("PLANE LONGITUDE", "degrees", cancellationToken: cancellationToken);

            var position = new SimConnectDataInitPosition
            {
                Latitude = currentLat + 0.002,
                Longitude = currentLon + 0.002,
                Altitude = 100,
                Heading = 180,
                OnGround = 1,
                Airspeed = 0,
            };

            var aiObject = await client.AIObjects.CreateObjectAsync(TestSimObjectModel, position, "Tracking Test", cancellationToken);

            try
            {
                // Test getting object by ID
                var retrievedObject = client.AIObjects.GetObject(aiObject.ObjectId);
                if (retrievedObject == null)
                {
                    Console.WriteLine("   ❌ Could not retrieve object by ID");
                    return false;
                }

                if (retrievedObject.ObjectId != aiObject.ObjectId)
                {
                    Console.WriteLine("   ❌ Retrieved object has wrong ID");
                    return false;
                }

                Console.WriteLine($"      ✅ Object tracking verified: ID {aiObject.ObjectId}");

                if (!ValidateCreatedObjectMetadata(aiObject, TestSimObjectModel, position))
                {
                    return false;
                }

                if (!ValidateTypeLookup(client, TestSimObjectModel, aiObject, expectedCount: 1))
                {
                    return false;
                }

                if (!ValidateTypeLookup(client, TestSimObjectModel.ToUpperInvariant(), aiObject, expectedCount: 1))
                {
                    Console.WriteLine("   ❌ Type lookup should be case-insensitive");
                    return false;
                }

                if (client.AIObjects.GetObjectsByType("DefinitelyNotACoffeeCup").Any())
                {
                    Console.WriteLine("   ❌ Unknown object type lookup should be empty");
                    return false;
                }

                // Test user data
                if (retrievedObject.UserData?.ToString() != "Tracking Test")
                {
                    Console.WriteLine($"   ❌ User data mismatch: expected 'Tracking Test', got '{retrievedObject.UserData}'");
                    return false;
                }

                Console.WriteLine("      ✅ User data preserved correctly");

                var removed = await client.AIObjects.RemoveObjectAsync(aiObject.ObjectId, cancellationToken);
                if (!removed)
                {
                    Console.WriteLine("   ❌ Remove by object ID returned false for tracked object");
                    return false;
                }

                if (aiObject.IsActive)
                {
                    Console.WriteLine("   ❌ Remove by object ID should mark object inactive");
                    return false;
                }

                if (client.AIObjects.GetObject(aiObject.ObjectId) != null)
                {
                    Console.WriteLine("   ❌ Removed object should not be retrievable by ID");
                    return false;
                }

                if (!ValidateTypeLookup(client, TestSimObjectModel, aiObject, expectedCount: 0))
                {
                    return false;
                }

                var removedAgain = await client.AIObjects.RemoveObjectAsync(aiObject.ObjectId, cancellationToken);
                if (removedAgain)
                {
                    Console.WriteLine("   ❌ Remove by object ID should return false after cleanup");
                    return false;
                }

                Console.WriteLine("      ✅ Remove by ID cleaned up tracking correctly");
                return true;
            }
            finally
            {
                if (aiObject.IsActive)
                {
                    await client.AIObjects.RemoveObjectAsync(aiObject, cancellationToken);
                }
            }
        }

        private static bool ValidateCreatedObjectMetadata(SimObject aiObject, string expectedContainerTitle, SimConnectDataInitPosition expectedPosition)
        {
            if (aiObject.ContainerTitle != expectedContainerTitle)
            {
                Console.WriteLine($"   ❌ Expected container title '{expectedContainerTitle}', got '{aiObject.ContainerTitle}'");
                return false;
            }

            if (aiObject.RequestId == 0)
            {
                Console.WriteLine("   ❌ Created object should preserve its request ID");
                return false;
            }

            if (Math.Abs(aiObject.InitialPosition.Latitude - expectedPosition.Latitude) > double.Epsilon ||
                Math.Abs(aiObject.InitialPosition.Longitude - expectedPosition.Longitude) > double.Epsilon ||
                Math.Abs(aiObject.InitialPosition.Altitude - expectedPosition.Altitude) > double.Epsilon ||
                Math.Abs(aiObject.InitialPosition.Heading - expectedPosition.Heading) > double.Epsilon ||
                aiObject.InitialPosition.OnGround != expectedPosition.OnGround ||
                aiObject.InitialPosition.Airspeed != expectedPosition.Airspeed)
            {
                Console.WriteLine("   ❌ Created object should preserve its requested initial position");
                return false;
            }

            return true;
        }

        private static bool ValidateTypeLookup(SimConnectClient client, string containerTitle, SimObject expectedObject, int expectedCount)
        {
            return ValidateTypeLookup(client, containerTitle, new[] { expectedObject }, expectedCount);
        }

        private static bool ValidateTypeLookup(SimConnectClient client, string containerTitle, IEnumerable<SimObject> expectedObjects, int expectedCount)
        {
            var objectsByType = client.AIObjects.GetObjectsByType(containerTitle).ToList();
            if (objectsByType.Count != expectedCount)
            {
                Console.WriteLine($"   ❌ Expected {expectedCount} objects for type '{containerTitle}', got {objectsByType.Count}");
                return false;
            }

            if (expectedCount > 0)
            {
                var expectedIds = expectedObjects.Select(static obj => obj.ObjectId).ToHashSet();
                var actualIds = objectsByType.Select(static obj => obj.ObjectId).ToHashSet();
                if (!expectedIds.SetEquals(actualIds))
                {
                    Console.WriteLine($"   ❌ Type lookup for '{containerTitle}' returned the wrong object IDs");
                    return false;
                }
            }

            Console.WriteLine($"      ✅ Type lookup for '{containerTitle}' returned {objectsByType.Count} object(s)");
            return true;
        }
    }
}
