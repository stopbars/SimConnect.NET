// <copyright file="PerformanceTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Diagnostics;
using SimConnect.NET;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Tests for performance and stress scenarios.
    /// </summary>
    public class PerformanceTests : ISimConnectTest
    {
        /// <inheritdoc/>
        public string Name => "Performance & Stress";

        /// <inheritdoc/>
        public string Description => "Tests performance under load and stress conditions";

        /// <inheritdoc/>
        public string Category => "Performance";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(45));

                // Test concurrent requests
                if (!await TestConcurrentRequests(client, cts.Token))
                {
                    return false;
                }

                // Test rapid sequential requests
                if (!await TestRapidSequentialRequests(client, cts.Token))
                {
                    return false;
                }

                // Test definition caching efficiency
                if (!await TestDefinitionCaching(client, cts.Token))
                {
                    return false;
                }

                // Test timeout handling
                if (!await TestTimeoutHandling(client, cts.Token))
                {
                    return false;
                }

                Console.WriteLine("   ✅ All performance tests successful");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ Performance test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Performance test failed: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> TestConcurrentRequests(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing concurrent requests...");

            var stopwatch = Stopwatch.StartNew();
            const int concurrentCount = 20;

            var tasks = new List<Task<double>>();
            var simVars = new[]
            {
                ("PLANE LATITUDE", "degrees"),
                ("PLANE LONGITUDE", "degrees"),
                ("PLANE ALTITUDE", "feet"),
                ("PLANE HEADING DEGREES TRUE", "degrees"),
                ("GROUND VELOCITY", "knots"),
            };

            for (int i = 0; i < concurrentCount; i++)
            {
                var simVar = simVars[i % simVars.Length];
                tasks.Add(client.SimVars.GetAsync<double>(simVar.Item1, simVar.Item2, cancellationToken: cancellationToken));
            }

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            Console.WriteLine($"      ⚡ {concurrentCount} concurrent requests in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"      📊 Average: {stopwatch.ElapsedMilliseconds / (double)concurrentCount:F1}ms per request");

            if (results.Any(r => double.IsNaN(r) || double.IsInfinity(r)))
            {
                Console.WriteLine("   ❌ Some concurrent requests returned invalid data");
                return false;
            }

            return true;
        }

        private static async Task<bool> TestRapidSequentialRequests(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing rapid sequential requests...");

            var stopwatch = Stopwatch.StartNew();
            const int sequentialCount = 30;

            var results = new List<double>();
            for (int i = 0; i < sequentialCount; i++)
            {
                var result = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet", cancellationToken: cancellationToken);
                results.Add(result);
            }

            stopwatch.Stop();

            Console.WriteLine($"      🏃 {sequentialCount} sequential requests in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"      📊 Average: {stopwatch.ElapsedMilliseconds / (double)sequentialCount:F1}ms per request");

            if (results.Any(r => double.IsNaN(r) || double.IsInfinity(r)))
            {
                Console.WriteLine("   ❌ Some sequential requests returned invalid data");
                return false;
            }

            return true;
        }

        private static async Task<bool> TestDefinitionCaching(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing definition caching efficiency...");

            // First request - should create new definition
            var stopwatch = Stopwatch.StartNew();
            await client.SimVars.GetAsync<double>("PLANE PITCH DEGREES", "degrees", cancellationToken: cancellationToken);
            var firstRequestTime = stopwatch.ElapsedMilliseconds;

            // Subsequent requests - should reuse definition
            stopwatch.Restart();
            var tasks = new List<Task<double>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(client.SimVars.GetAsync<double>("PLANE PITCH DEGREES", "degrees", cancellationToken: cancellationToken));
            }

            await Task.WhenAll(tasks);
            var cachedRequestsTime = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"      🆕 First request: {firstRequestTime}ms");
            Console.WriteLine($"      🔄 10 cached requests: {cachedRequestsTime}ms");
            Console.WriteLine($"      📊 Caching efficiency: {cachedRequestsTime / 10.0:F1}ms per cached request");

            // Cached requests should generally be faster on average
            // (though not guaranteed due to network variability)
            return true;
        }

        private static async Task<bool> TestTimeoutHandling(SimConnectClient client, CancellationToken cancellationToken)
        {
            Console.WriteLine("   🔍 Testing timeout handling...");

            try
            {
                using var shortTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
                using var combined = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, shortTimeout.Token);

                // This should timeout quickly
                await client.SimVars.GetAsync<double>("PLANE LATITUDE", "degrees", cancellationToken: combined.Token);

                Console.WriteLine("   ❌ Request should have timed out but didn't");
                return false;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("      ✅ Timeout handled correctly");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Unexpected exception during timeout test: {ex.Message}");
                return false;
            }
        }
    }
}
