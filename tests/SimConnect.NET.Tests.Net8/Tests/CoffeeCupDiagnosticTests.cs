// <copyright file="CoffeeCupDiagnosticTests.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using SimConnect.NET.Events;

namespace SimConnect.NET.Tests.Net8.Tests
{
    /// <summary>
    /// Runs a controlled built-in SimObject creation diagnostic at the user aircraft position.
    /// </summary>
    internal sealed class SimObjectCreationDiagnosticTests : ISimConnectTest
    {
        private const string ModelTitle = "CoffeeCup";

        /// <inheritdoc/>
        public string Name => "SimObject creation diagnostic";

        /// <inheritdoc/>
        public string Description => "Creates and removes one selected SimObject at the user aircraft position (CoffeeCup by default; SIMCONNECT_DIAGNOSTIC_MODEL overrides it)";

        /// <inheritdoc/>
        public string Category => "AI Diagnostic";

        /// <inheritdoc/>
        public async Task<bool> RunAsync(SimConnectClient client, CancellationToken cancellationToken = default)
        {
            var modelTitle = Environment.GetEnvironmentVariable("SIMCONNECT_DIAGNOSTIC_MODEL") ?? ModelTitle;

            void OnError(object? sender, SimConnectErrorEventArgs args)
            {
                Console.WriteLine(
                    $"      SimConnect error: code={(uint)args.Error} name={args.Error} sendId={args.SendId} index={args.Index} context={args.Context} exception={args.Exception}");
            }

            client.ErrorOccurred += OnError;
            try
            {
                var latitude = await client.SimVars.GetAsync<double>("PLANE LATITUDE", "degrees", cancellationToken: cancellationToken);
                var longitude = await client.SimVars.GetAsync<double>("PLANE LONGITUDE", "degrees", cancellationToken: cancellationToken);
                var altitude = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet", cancellationToken: cancellationToken);
                var heading = await client.SimVars.GetAsync<double>("PLANE HEADING DEGREES TRUE", "degrees", cancellationToken: cancellationToken);
                var position = new SimConnectDataInitPosition
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    Altitude = altitude,
                    Heading = heading,
                    Pitch = 0,
                    Bank = 0,
                    OnGround = 1,
                    Airspeed = 0,
                };

                Console.WriteLine(
                    $"      Spawn: model={modelTitle} lat={latitude:F8} lon={longitude:F8} altFeet={altitude:F2} heading={heading:F2} simulator={(client.IsMSFS2024 ? "MSFS 2024" : "MSFS 2020")}");

                var created = await client.AIObjects.CreateObjectAsync(
                    modelTitle,
                    position,
                    "Controlled CoffeeCup diagnostic",
                    cancellationToken);
                Console.WriteLine($"      {modelTitle} succeeded: objectId={created.ObjectId}");
                await client.AIObjects.RemoveObjectAsync(created, cancellationToken);
                return true;
            }
            finally
            {
                client.ErrorOccurred -= OnError;
            }
        }
    }
}
