// <copyright file="ConnectionTests.cs" company="AussieScorcher">
// Copyright (c) AussieScorcher. All rights reserved.
// </copyright>

using SimConnect.NET;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Tests for SimConnect connection functionality.
    /// </summary>
    public class ConnectionTests : ISimConnectTest
    {
        /// <inheritdoc/>
        public string Name => "Connection Lifecycle";

        /// <inheritdoc/>
        public string Description => "Tests connection, status checking, and disconnection";

        /// <inheritdoc/>
        public string Category => "Connection";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                // Test that client is already connected
                if (!client.IsConnected)
                {
                    Console.WriteLine("   ❌ Client should already be connected");
                    return false;
                }

                Console.WriteLine("   ✅ Connection status verified");

                // Test that we can access managers
                var simVars = client.SimVars;
                var aircraft = client.Aircraft;
                var aiObjects = client.AIObjects;

                Console.WriteLine("   ✅ All managers accessible");

                // Test a simple operation to verify connection is working
                var testValue = await simVars.GetAsync<double>("PLANE LATITUDE", "degrees", cancellationToken: cancellationToken);
                Console.WriteLine($"   ✅ Basic operation successful (lat: {testValue:F6})");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}
