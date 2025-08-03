// <copyright file="AIObjectTests.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
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

            var aiObject = await client.AIObjects.CreateObjectAsync("BARS_Stopbar_On", position, "Test Object", cancellationToken);
            Console.WriteLine($"      ✅ AI Object created with ID: {aiObject.ObjectId}");

            if (!aiObject.IsActive)
            {
                Console.WriteLine("   ❌ Created object should be active");
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
                        Latitude = currentLat + (0.001 * (i + 1)),
                        Longitude = currentLon + (0.001 * (i + 1)),
                        Altitude = 100,
                        Heading = 90 + (i * 30),
                        OnGround = 1,
                        Airspeed = 0,
                    };

                    var obj = await client.AIObjects.CreateObjectAsync("BARS_Stopbar_On", position, $"Test Object {i}", cancellationToken);
                    objects.Add(obj);
                    Console.WriteLine($"      ✅ Created object {i + 1} with ID: {obj.ObjectId}");
                }

                Console.WriteLine($"      📊 Created {objects.Count} objects");

                // Verify they're all tracked
                if (client.AIObjects.ActiveObjectCount != objects.Count)
                {
                    Console.WriteLine($"   ❌ Expected {objects.Count} active objects, got {client.AIObjects.ActiveObjectCount}");
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

            var aiObject = await client.AIObjects.CreateObjectAsync("BARS_Stopbar_On", position, "Tracking Test", cancellationToken);

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

                // Test user data
                if (retrievedObject.UserData?.ToString() != "Tracking Test")
                {
                    Console.WriteLine($"   ❌ User data mismatch: expected 'Tracking Test', got '{retrievedObject.UserData}'");
                    return false;
                }

                Console.WriteLine("      ✅ User data preserved correctly");

                return true;
            }
            finally
            {
                await client.AIObjects.RemoveObjectAsync(aiObject, cancellationToken);
            }
        }
    }
}
