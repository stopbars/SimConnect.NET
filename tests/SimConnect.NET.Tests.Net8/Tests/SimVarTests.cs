// <copyright file="SimVarTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using SimConnect.NET;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Tests for SimVar get/set operations.
    /// </summary>
    public class SimVarTests : ISimConnectTest
    {
        /// <inheritdoc/>
        public string Name => "SimVar Operations";

        /// <inheritdoc/>
        public string Description => "Tests getting and setting various SimVar types";

        /// <inheritdoc/>
        public string Category => "SimVar";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                // Test basic position SimVars
                if (!await TestPositionSimVars(client, cts.Token))
                {
                    return false;
                }

                // Test different data types
                if (!await TestDataTypes(client, cts.Token))
                {
                    return false;
                }

                // Test fixed-size string fetching
                if (!await TestStringFetching(client, cts.Token))
                {
                    return false;
                }

                // Test SimVar setting
                if (!await TestSimVarSetting(client, cts.Token))
                {
                    return false;
                }

                // Test setting position via struct
                if (!await TestPositionStructSetAsync(client, cts.Token))
                {
                    return false;
                }

                // Test rapid consecutive requests
                if (!await TestRapidRequests(client, cts.Token))
                {
                    return false;
                }

                if (!await TestSubscriptions(client, cts.Token))
                {
                    return false;
                }

                Console.WriteLine("   ✅ All SimVar operations successful");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ SimVar test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ SimVar test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestPositionSimVars(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing position SimVars...");

            var latitude = await client.SimVars.GetAsync<double>("PLANE LATITUDE", "degrees", cancellationToken: cancellationToken);
            var longitude = await client.SimVars.GetAsync<double>("PLANE LONGITUDE", "degrees", cancellationToken: cancellationToken);
            var altitude = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet", cancellationToken: cancellationToken);

            Console.WriteLine($"      📍 Position: {latitude:F6}°, {longitude:F6}°, {altitude:F0}ft");

            // Basic validation
            if (latitude < -90 || latitude > 90)
            {
                Console.WriteLine("   ❌ Invalid latitude value");
                return false;
            }

            if (longitude < -180 || longitude > 180)
            {
                Console.WriteLine("   ❌ Invalid longitude value");
                return false;
            }

            var position = await client.SimVars.GetAsync<Position>(cancellationToken: cancellationToken);
            Console.WriteLine($"      🗺️  Position struct: {position.Latitude:F6}°, {position.Longitude:F6}°, {position.Altitude:F0}ft");
            if (position.Latitude < -90 || position.Latitude > 90)
            {
                Console.WriteLine("   ❌ Invalid latitude value");
                return false;
            }

            if (position.Longitude < -180 || position.Longitude > 180)
            {
                Console.WriteLine("   ❌ Invalid longitude value");
                return false;
            }

            if (position.Altitude < 0 || position.Altitude > 60000)
            {
                Console.WriteLine("   ❌ Invalid altitude value");
                return false;
            }

            return true;
        }

        private static async Task<bool> TestDataTypes(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing different data types...");

            // Test double
            var groundSpeed = await client.SimVars.GetAsync<double>("GROUND VELOCITY", "knots", cancellationToken: cancellationToken);
            Console.WriteLine($"      🏃 Ground speed: {groundSpeed:F2} kts");

            // Test int
            var transponder = await client.SimVars.GetAsync<int>("TRANSPONDER CODE:1", "BCO16", cancellationToken: cancellationToken);
            Console.WriteLine($"      📻 Transponder: {transponder}");

            // Test bool (as int)
            var onGround = await client.SimVars.GetAsync<int>("SIM ON GROUND", "Bool", cancellationToken: cancellationToken);
            Console.WriteLine($"      🛬 On ground: {onGround == 1}");

            return true;
        }

        private static async Task<bool> TestStringFetching(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing fixed-size string fetching...");

            var title = await client.SimVars.GetAsync<string>("TITLE", cancellationToken: cancellationToken);
            Console.WriteLine($"      ✈️ Aircraft title: {title}");

            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("   ❌ Aircraft title was empty");
                return false;
            }

            if (title.Contains('\0'))
            {
                Console.WriteLine("   ❌ Aircraft title contained null padding");
                return false;
            }

            return true;
        }

        private static async Task<bool> TestSimVarSetting(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing SimVar setting...");

            // Test setting transponder code (safe to change)
            var originalCode = await client.SimVars.GetAsync<int>("TRANSPONDER CODE:1", "BCO16", cancellationToken: cancellationToken);
            Console.WriteLine($"      📻 Original transponder: {originalCode}");

            const int testCode = 2468;
            await client.SimVars.SetAsync("TRANSPONDER CODE:1", "BCO16", testCode, cancellationToken: cancellationToken);
            await Task.Delay(500, cancellationToken); // Give it time to apply

            var newCode = await client.SimVars.GetAsync<int>("TRANSPONDER CODE:1", "BCO16", cancellationToken: cancellationToken);
            Console.WriteLine($"      📻 New transponder: {newCode}");

            // Restore original
            await client.SimVars.SetAsync("TRANSPONDER CODE:1", "BCO16", originalCode, cancellationToken: cancellationToken);

            if (newCode != testCode)
            {
                Console.WriteLine($"   ❌ Expected {testCode}, got {newCode}");
                return false;
            }

            Console.WriteLine("      ✅ Setting and restoring successful");
            return true;
        }

        private static async Task<bool> TestPositionStructSetAsync(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔧 Testing SetAsync with Position struct (altitude +15 ft, then restore)...");

            // Get current position (includes altitude)
            var original = await client.SimVars.GetAsync<Position>(cancellationToken: cancellationToken);
            Console.WriteLine($"      Original Altitude: {original.Altitude:F2} ft");

            var target = original;
            target.Altitude = original.Altitude + 15.0;

            // Attempt to set new altitude via struct
            await client.SimVars.SetAsync(target, cancellationToken: cancellationToken);

            var afterSet = await client.SimVars.GetAsync<Position>(cancellationToken: cancellationToken);
            Console.WriteLine($"      After Set Altitude: {afterSet.Altitude:F2} ft (target {target.Altitude:F2} ft)");

            // Tolerance for sim update/drift
            const double toleranceFt = 10.0;
            var deltaAfterSet = Math.Abs(afterSet.Altitude - target.Altitude);
            if (deltaAfterSet > toleranceFt)
            {
                Console.WriteLine($"   ❌ Altitude not within tolerance after set. Δ={deltaAfterSet:F2} ft");
                return false;
            }

            // Restore original altitude
            await client.SimVars.SetAsync(original, cancellationToken: cancellationToken);

            var restored = await client.SimVars.GetAsync<Position>(cancellationToken: cancellationToken);
            Console.WriteLine($"      Restored Altitude: {restored.Altitude:F2} ft (expected {original.Altitude:F2} ft)");

            var deltaRestore = Math.Abs(restored.Altitude - original.Altitude);
            if (deltaRestore > toleranceFt)
            {
                Console.WriteLine($"   ❌ Altitude not restored within tolerance. Δ={deltaRestore:F2} ft");
                return false;
            }

            Console.WriteLine("      ✅ Position struct SetAsync OK");
            return true;
        }

        private static async Task<bool> TestRapidRequests(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing rapid consecutive requests...");

            var tasks = new List<Task<double>>();
            for (int i = 0; i < 15; i++)
            {
                tasks.Add(client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet", cancellationToken: cancellationToken));
            }

            var results = await Task.WhenAll(tasks);
            Console.WriteLine($"      🏃 {results.Length} rapid requests completed");
            Console.WriteLine($"      📊 Altitude range: {results.Min():F0} - {results.Max():F0} feet");

            return results.All(r => !double.IsNaN(r) && !double.IsInfinity(r));
        }

        private static async Task<bool> TestSubscriptions(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing subscriptions...");
            int updatesReceived = 0;

            var title = await client.SimVars.GetAsync<string>("TITLE", cancellationToken: cancellationToken);

            using var subscription = client.SimVars.Subscribe<Position>(
                SimConnectPeriod.VisualFrame,
                (position) =>
                {
                    if (position.Altitude <= 0)
                    {
                        Console.WriteLine("   ❌ Received invalid position update (non-positive altitude)");
                    }
                    else if (position.Title != title)
                    {
                        Console.WriteLine($"   ❌ Received invalid position update (unexpected title: {position.Title})");
                    }
                    else
                    {
                        updatesReceived++;
                    }
                },
                cancellationToken: cancellationToken);

            // Allow 1 second for initial subscription updates before enumerating
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            if (subscription.Completion.IsFaulted)
            {
                Console.WriteLine($"   ❌ Subscription faulted: {subscription.Completion.Exception}");
                return false;
            }
            else if (subscription.Completion.IsCompleted)
            {
                Console.WriteLine("   ❌ Subscription completed prematurely");
                return false;
            }
            else if (subscription.Completion.IsCanceled)
            {
                Console.WriteLine("   ❌ Subscription was canceled prematurely");
                return false;
            }

            subscription.Dispose();

            // Non-throwing wait pattern: avoid first-chance TaskCanceledException by inspecting status
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            var finished = await Task.WhenAny(subscription.Completion, timeoutTask);
            if (finished != subscription.Completion)
            {
                Console.WriteLine("   ❌ Subscription did not complete (cancel/fault) within timeout");
                return false;
            }

            if (subscription.Completion.IsFaulted)
            {
                Console.WriteLine($"   ❌ Subscription faulted after dispose: {subscription.Completion.Exception}");
                return false;
            }

            // Subscriptions are expected to transition to Canceled when disposed
            if (!subscription.Completion.IsCanceled)
            {
                if (subscription.Completion.IsCompleted)
                {
                    Console.WriteLine("   ❌ Subscription completed instead of being canceled after dispose");
                }
                else
                {
                    Console.WriteLine("   ❌ Subscription did not cancel after dispose");
                }

                return false;
            }

            if (updatesReceived == 0)
            {
                Console.WriteLine("   ❌ No subscription updates received");
                return false;
            }

            Console.WriteLine($"      ✅ Received {updatesReceived} updates");
            return true;
        }
    }
}

/// <summary>
/// Represents the aircraft position using SimVars.
/// </summary>
public struct Position
{
    /// <summary>
    /// Gets or sets the latitude of the plane in degrees.
    /// </summary>
    [SimConnect("PLANE LATITUDE", "degrees")]
    public double Latitude;

    /// <summary>
    /// Gets or sets the longitude of the plane in degrees.
    /// </summary>
    [SimConnect("PLANE LONGITUDE", "degrees")]
    public double Longitude;

    /// <summary>
    /// Gets or sets the altitude of the plane in feet.
    /// </summary>
    [SimConnect("PLANE ALTITUDE", "feet")]
    public double Altitude;

    /// <summary>
    /// Gets or sets the aircraft title as reported by the simulator.
    /// </summary>
    [SimConnect("TITLE", SimConnectDataType.String128)]
    public string Title;
}
