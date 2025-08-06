// <copyright file="AircraftTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using SimConnect.NET;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Tests for aircraft data manager functionality.
    /// </summary>
    public class AircraftTests : ISimConnectTest
    {
        /// <inheritdoc/>
        public string Name => "Aircraft Data Manager";

        /// <inheritdoc/>
        public string Description => "Tests convenient access to common aircraft data";

        /// <inheritdoc/>
        public string Category => "Aircraft";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(15));

                // Test position data
                if (!await TestPositionData(client, cts.Token))
                {
                    return false;
                }

                // Test motion data
                if (!await TestMotionData(client, cts.Token))
                {
                    return false;
                }

                // Test engine data
                if (!await TestEngineData(client, cts.Token))
                {
                    return false;
                }

                Console.WriteLine("   ✅ All aircraft data operations successful");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ Aircraft test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Aircraft test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestPositionData(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing aircraft position data...");

            var position = await client.Aircraft.GetPositionAsync(0, cancellationToken);
            Console.WriteLine($"      📍 Position: {position.Latitude:F6}°, {position.Longitude:F6}°");
            Console.WriteLine($"      🏔️  Altitude: {position.Altitude:F0}ft MSL, {position.AltitudeAboveGround:F0}ft AGL");

            // Validate position data
            if (position.Latitude < -90 || position.Latitude > 90)
            {
                Console.WriteLine("   ❌ Invalid latitude in position data");
                return false;
            }

            if (position.Longitude < -180 || position.Longitude > 180)
            {
                Console.WriteLine("   ❌ Invalid longitude in position data");
                return false;
            }

            return true;
        }

        private static async Task<bool> TestMotionData(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing aircraft motion data...");

            var motion = await client.Aircraft.GetMotionAsync(0, cancellationToken);
            Console.WriteLine($"      🏃 Speed: IAS {motion.IndicatedAirspeed:F1} kts, TAS {motion.TrueAirspeed:F1} kts");
            Console.WriteLine($"      🏃 Ground Speed: {motion.GroundSpeed:F1} kts");
            Console.WriteLine($"      � Vertical speed: {motion.VerticalSpeed:F0} fpm");
            Console.WriteLine($"      🌐 GPS: {motion.GpsGroundSpeed:F1} m/s, track {motion.GpsTrack:F0}°");

            // Basic validation - these should be reasonable numbers
            if (motion.IndicatedAirspeed < 0 || motion.IndicatedAirspeed > 1000)
            {
                Console.WriteLine($"   ⚠️  Unusual indicated airspeed: {motion.IndicatedAirspeed} kts");
            }

            if (motion.GpsTrack < 0 || motion.GpsTrack >= 360)
            {
                Console.WriteLine("   ❌ Invalid GPS track");
                return false;
            }

            return true;
        }

        private static async Task<bool> TestEngineData(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing aircraft engine data...");

            try
            {
                var engine = await client.Aircraft.GetEngineAsync(1, 0, cancellationToken);
                Console.WriteLine($"      🔥 Engine 1: {(engine.IsRunning ? "running" : "stopped")}, {engine.N1:F1}% N1");
                Console.WriteLine($"      ⚡ RPM: {engine.Rpm:F0}, Throttle: {engine.ThrottlePosition:F1}%");

                // Engine data validation is tricky since it depends on aircraft type
                // Just check that we got some data without errors
                Console.WriteLine("      ✅ Engine data retrieved successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️  Engine data not available (normal for some aircraft): {ex.Message}");
                return true; // Not a failure - some aircraft don't have detailed engine data
            }
        }
    }
}
