# SimConnect.NET

🚧 **Work In Progress** 🚧

A simple async C# wrapper for Microsoft Flight Simulator's SimConnect SDK.

## What it does

-   Connect to Microsoft Flight Simulator
-   Read aircraft data (position, speed, altitude, etc.)
-   Set aircraft parameters
-   Create AI objects

## Quick Start

```csharp
var client = new SimConnectClient();
await client.ConnectAsync();

// Get some aircraft data
var altitude = await client.Aircraft.GetAltitudeAsync();
var speed = await client.Aircraft.GetIndicatedAirspeedAsync();

await client.DisconnectAsync();
```

## Status

-   ✅ Basic connection and data reading
-   ✅ Aircraft position and motion data
-   ⏳ More SimVar support
-   ⏳ Event handling
-   ⏳ Better error handling
-   ⏳ Documentation

## Requirements

-   .NET 8.0 or 9.0
-   Microsoft Flight Simulator (for testing)

## Build

```bash
dotnet build
```
