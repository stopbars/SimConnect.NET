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

                // Test SimVar setting
                if (!await TestSimVarSetting(client, cts.Token))
                {
                    return false;
                }

                // Test rapid consecutive requests
                if (!await TestRapidRequests(client, cts.Token))
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
}
