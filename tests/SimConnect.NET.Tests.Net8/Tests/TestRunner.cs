// <copyright file="TestRunner.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Runtime.InteropServices;
using SimConnect.NET;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Enhanced test runner for SimConnect.NET integration tests.
    /// </summary>
    public class TestRunner
    {
        private readonly List<ISimConnectTest> tests;
        private SimConnectClient? client;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunner"/> class.
        /// </summary>
        public TestRunner()
        {
            this.tests = new List<ISimConnectTest>
            {
                new ConnectionTests(),
                new SimVarTests(),
                new AircraftTests(),
                new AIObjectTests(),
                new InputEventTests(),
                new InputEventValueTests(),
                new PerformanceTests(),
            };
        }

        /// <summary>
        /// Main entry point for the enhanced test suite.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🚁 SimConnect.NET Enhanced Integration Tests");
            Console.WriteLine("=============================================");
            Console.WriteLine();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("❌ SimConnect.NET requires Windows and Microsoft Flight Simulator.");
                Environment.Exit(1);
                return;
            }

            var runner = new TestRunner();

            try
            {
                await runner.RunAllTestsAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Fatal error: {ex.Message}");
                Environment.Exit(1);
            }
            finally
            {
                // Ensure cleanup
                if (runner.client?.IsConnected == true)
                {
                    try
                    {
                        await runner.client.DisconnectAsync();
                        Console.WriteLine("🔌 Disconnected from SimConnect");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  Error during disconnect: {ex.Message}");
                    }
                }

                runner.client?.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Runs all tests with enhanced reporting and filtering.
        /// </summary>
        /// <param name="args">Command line arguments for test filtering.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAllTestsAsync(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            // Parse command line arguments
            var options = ParseArguments(args);
            var testsToRun = this.FilterTests(options);

            Console.WriteLine($"📋 Running {testsToRun.Count} of {this.tests.Count} available tests");
            if (options.Categories.Any())
            {
                Console.WriteLine($"🏷️  Filtered by categories: {string.Join(", ", options.Categories)}");
            }

            Console.WriteLine();

            // Connect to SimConnect
            if (!await this.ConnectToSimConnectAsync())
            {
                return;
            }

            var results = new List<TestResult>();

            // Run tests
            foreach (var test in testsToRun)
            {
                var result = await this.RunSingleTestAsync(test);
                results.Add(result);

                if (!result.Passed && options.StopOnFirstFailure)
                {
                    Console.WriteLine("🛑 Stopping on first failure as requested");
                    break;
                }

                // Small delay between tests
                await Task.Delay(500);
                Console.WriteLine();
            }

            stopwatch.Stop();

            // Report results
            PrintTestSummary(results, stopwatch.Elapsed);
        }

        private static TestOptions ParseArguments(string[] args)
        {
            var options = new TestOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--category" when i + 1 < args.Length:
                        options.Categories.Add(args[++i]);
                        break;
                    case "--stop-on-failure":
                        options.StopOnFirstFailure = true;
                        break;
                    case "--verbose":
                        options.Verbose = true;
                        break;
                }
            }

            return options;
        }

        private static void PrintTestSummary(List<TestResult> results, TimeSpan totalDuration)
        {
            var passed = results.Count(r => r.Passed);
            var failed = results.Count(r => !r.Passed);
            var totalTests = results.Count;

            Console.WriteLine("📊 Test Results Summary");
            Console.WriteLine("========================");
            Console.WriteLine($"✅ Passed: {passed}");
            Console.WriteLine($"❌ Failed: {failed}");
            Console.WriteLine($"📈 Total:  {totalTests}");
            Console.WriteLine($"⏱️  Duration: {totalDuration.TotalSeconds:F1}s");
            Console.WriteLine();

            if (failed > 0)
            {
                Console.WriteLine("❌ Failed Tests:");
                foreach (var result in results.Where(r => !r.Passed))
                {
                    Console.WriteLine($"   • {result.Test.Name} ({result.Test.Category})");
                    if (result.Exception != null)
                    {
                        Console.WriteLine($"     Error: {result.Exception.Message}");
                    }
                }

                Console.WriteLine();
            }

            // Performance summary
            Console.WriteLine("⚡ Performance Summary:");
            var avgDuration = results.Average(r => r.Duration.TotalMilliseconds);
            var slowestTest = results.OrderByDescending(r => r.Duration).First();
            var fastestTest = results.OrderBy(r => r.Duration).First();

            Console.WriteLine($"   Average: {avgDuration:F0}ms per test");
            Console.WriteLine($"   Slowest: {slowestTest.Test.Name} ({slowestTest.Duration.TotalMilliseconds:F0}ms)");
            Console.WriteLine($"   Fastest: {fastestTest.Test.Name} ({fastestTest.Duration.TotalMilliseconds:F0}ms)");

            var successRate = (passed / (double)totalTests) * 100;
            Console.WriteLine($"   Success Rate: {successRate:F1}%");
        }

        private List<ISimConnectTest> FilterTests(TestOptions options)
        {
            if (!options.Categories.Any())
            {
                return this.tests.ToList();
            }

            return this.tests.Where(test => options.Categories.Contains(test.Category, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        private async Task<bool> ConnectToSimConnectAsync()
        {
            try
            {
                Console.WriteLine("🔌 Connecting to SimConnect...");
                this.client = new SimConnectClient("SimConnect.NET Enhanced Test Suite");
                await this.client.ConnectAsync();

                if (!this.client.IsConnected)
                {
                    Console.WriteLine("❌ Failed to connect to SimConnect");
                    return false;
                }

                Console.WriteLine("✅ Connected to SimConnect successfully");
                Console.WriteLine();
                return true;
            }
            catch (SimConnectException ex)
            {
                Console.WriteLine($"❌ SimConnect connection error: {ex.Message} (Code: {ex.ErrorCode})");
                Console.WriteLine("💡 Make sure Microsoft Flight Simulator is running and SimConnect is enabled");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected connection error: {ex.Message}");
                return false;
            }
        }

        private async Task<TestResult> RunSingleTestAsync(ISimConnectTest test)
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"🧪 Running: {test.Name}");
            Console.WriteLine($"   📝 {test.Description}");

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                var passed = await test.RunAsync(this.client!, cts.Token);

                stopwatch.Stop();

                var status = passed ? "✅ PASSED" : "❌ FAILED";
                Console.WriteLine($"{status} - {test.Name} ({stopwatch.ElapsedMilliseconds}ms)");

                return new TestResult
                {
                    Test = test,
                    Passed = passed,
                    Duration = stopwatch.Elapsed,
                    Exception = null,
                };
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                Console.WriteLine($"⏰ TIMEOUT - {test.Name} ({stopwatch.ElapsedMilliseconds}ms)");

                return new TestResult
                {
                    Test = test,
                    Passed = false,
                    Duration = stopwatch.Elapsed,
                    Exception = new TimeoutException("Test timed out"),
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"💥 EXCEPTION - {test.Name}: {ex.Message}");

                return new TestResult
                {
                    Test = test,
                    Passed = false,
                    Duration = stopwatch.Elapsed,
                    Exception = ex,
                };
            }
        }

        private class TestOptions
        {
            public List<string> Categories { get; } = new();

            public bool StopOnFirstFailure { get; set; }

            public bool Verbose { get; set; }
        }

        private class TestResult
        {
            required public ISimConnectTest Test { get; init; }

            public bool Passed { get; init; }

            public TimeSpan Duration { get; init; }

            public Exception? Exception { get; init; }
        }
    }
}
