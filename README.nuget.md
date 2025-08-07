# SimConnect.NET

A modern, high-performance C# wrapper for Microsoft Flight Simulator's SimConnect SDK. It simplifies flight simulation development by providing async/await patterns and high-level abstractions, enabling real-time aircraft data monitoring, AI object management, and seamless integration with the simulator through a clean, type-safe API.

## ⚠️ Beta Software Notice

SimConnect.NET is currently in beta development. APIs may change, features may be added or removed, and breaking changes are expected between releases. Use in production environments at your own discretion and always test thoroughly before deploying.

## Features

-   **Async/Await Support**: Modern asynchronous patterns for all SimConnect operations
-   **High-Level Abstractions**: Simplified API for common flight simulation tasks
-   **Type-Safe Interface**: Strong typing with automatic data type inference
-   **Real-Time Data**: Efficient aircraft data monitoring and SimVar access
-   **AI Object Management**: Create and manage AI aircraft and objects
-   **Multi-Framework Support**: Compatible with .NET 8.0 and .NET 9.0

## Quick Start

1. Install the NuGet package:

    ```
    dotnet add package SimConnect.NET
    ```

2. Basic usage example:

    ```csharp
    using SimConnect.NET;

    var client = new SimConnectClient();
    await client.ConnectAsync();

    // Get aircraft data
    var altitude = await client.SimVarManager.GetAsync<double>("PLANE ALTITUDE", "feet");
    var airspeed = await client.SimVarManager.GetAsync<double>("AIRSPEED INDICATED", "knots");

    Console.WriteLine($"Altitude: {altitude:F0} ft");
    Console.WriteLine($"Airspeed: {airspeed:F0} kts");
    ```

## Requirements

-   Microsoft Flight Simulator 2020 or later
-   .NET 8.0 or .NET 9.0 runtime
-   SimConnect SDK (included with Flight Simulator)

## Documentation

For comprehensive documentation, examples, and API reference, visit our GitHub repository: https://github.com/stopbars/SimConnect.NET

## Contributing

We welcome contributions from the community! Please visit our GitHub repository for contribution guidelines, issue reporting, and development information.

## License

This project is licensed under the MIT License. See the LICENSE file in the GitHub repository for details.

## Disclaimer

SimConnect.NET is an independent third-party software project. We are not affiliated with, endorsed by, or connected to Microsoft Flight Simulator or any other simulation software.
