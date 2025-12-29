// <copyright file="SystemEventSubscriptionTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

namespace SimConnect.NET.Tests.Net8.Tests
{
    internal class SystemEventSubscriptionTests : ISimConnectTest
    {
        public string Name => "SystemEventSubscription";

        public string Description => "Tests system event subscription";

        public string Category => "System Event";

        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!client.IsConnected)
                {
                    Console.WriteLine("   ❌ Client should already be connected");
                    return false;
                }

                Console.WriteLine("   ✅ Connection status verified");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(15));

                bool testEventReceived = false;
                client.SystemEventReceived += (sender, e) =>
                {
                    switch (e.EventId)
                    {
                        case 100:
                            Console.WriteLine("4 seconds has passed!");
                            testEventReceived = true;
                            break;
                    }
                };

                await client.SubscribeToEventAsync("4sec", 100, cts.Token);

                Console.WriteLine("Listening for events...");

                while (!testEventReceived && !cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(500, cts.Token);
                }
                if (!testEventReceived)
                {
                    Console.WriteLine("   ❌ Did not receive expected system event");
                    return false;
                }
                Console.WriteLine("   ✅ Received expected system event");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ Connection test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}
