// <copyright file="SystemEventStateTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using SimConnect.NET.Events;

namespace SimConnect.NET.Tests.Net8.Tests
{
    internal class SystemEventStateTests : ISimConnectTest
    {
        public string Name => "SystemEventState";

        public string Description => "Verifies system event subscribe, SetSystemEventState, and Unsubscribe behavior";

        public string Category => "System Event";

        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            if (!client.IsConnected)
            {
                Console.WriteLine("   ❌ Client should already be connected");
                return false;
            }

            const uint systemEventId = 101;
            const string systemEventName = "1sec";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(40));

            int totalEvents = 0;

            EventHandler<SimSystemEventReceivedEventArgs> handler = (sender, e) =>
            {
                if (e.EventId != systemEventId)
                {
                    return;
                }

                Interlocked.Increment(ref totalEvents);
            };

            client.SystemEventReceived += handler;

            try
            {
                await client.SubscribeToEventAsync(systemEventName, systemEventId, cts.Token).ConfigureAwait(false);
                await client.SetSystemEventStateAsync(systemEventId, SimConnectState.On, cts.Token).ConfigureAwait(false);
                Console.WriteLine("   ⏳ Waiting for initial system event...");

                if (!await WaitForConditionAsync(() => Volatile.Read(ref totalEvents) > 0, TimeSpan.FromSeconds(15), cts.Token).ConfigureAwait(false))
                {
                    Console.WriteLine("   ❌ Did not receive initial system event");
                    return false;
                }

                Console.WriteLine("   ✅ Initial system event received");
                await client.SetSystemEventStateAsync(systemEventId, SimConnectState.Off, cts.Token).ConfigureAwait(false);
                int afterOffBaseline = Volatile.Read(ref totalEvents);

                Console.WriteLine("   ⏳ Waiting to confirm events are suppressed after SetSystemEventState(Off)...");
                await Task.Delay(TimeSpan.FromSeconds(6), cts.Token).ConfigureAwait(false);

                int afterOffCount = Volatile.Read(ref totalEvents) - afterOffBaseline;
                if (afterOffCount > 0)
                {
                    Console.WriteLine("   ❌ Received system events while state was Off");
                    return false;
                }

                Console.WriteLine("   ✅ No events received while state was Off");
                await client.SetSystemEventStateAsync(systemEventId, SimConnectState.On, cts.Token).ConfigureAwait(false);
                int afterOnBaseline = Volatile.Read(ref totalEvents);

                Console.WriteLine("   ⏳ Waiting for events after SetSystemEventState(On)...");
                if (!await WaitForConditionAsync(() => Volatile.Read(ref totalEvents) > afterOnBaseline, TimeSpan.FromSeconds(15), cts.Token).ConfigureAwait(false))
                {
                    Console.WriteLine("   ❌ Did not receive event after turning state On");
                    return false;
                }

                Console.WriteLine("   ✅ Events resumed after turning state On");

                await client.UnsubscribeFromEventAsync(systemEventId, cts.Token).ConfigureAwait(false);
                int afterUnsubscribeBaseline = Volatile.Read(ref totalEvents);

                Console.WriteLine("   ⏳ Waiting to ensure no events after unsubscribe...");
                await Task.Delay(TimeSpan.FromSeconds(6), cts.Token).ConfigureAwait(false);

                int afterUnsubscribeCount = Volatile.Read(ref totalEvents) - afterUnsubscribeBaseline;
                if (afterUnsubscribeCount > 0)
                {
                    Console.WriteLine("   ❌ Received system events after unsubscribe");
                    return false;
                }

                Console.WriteLine("   ✅ No events after unsubscribe");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("   ❌ System event state test timed out");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ System event state test failed: {ex.Message}");
                return false;
            }
            finally
            {
                client.SystemEventReceived -= handler;

                try
                {
                    await client.UnsubscribeFromEventAsync(systemEventId, CancellationToken.None).ConfigureAwait(false);
                }
                catch
                {
                    // Best-effort cleanup; ignore errors
                }
            }
        }

        private static async Task<bool> WaitForConditionAsync(Func<bool> predicate, TimeSpan timeout, CancellationToken token)
        {
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (predicate())
                {
                    return true;
                }

                await Task.Delay(200, token).ConfigureAwait(false);
            }

            return predicate();
        }
    }
}
