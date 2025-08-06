# SimConnect.NET AI Coding Agent Instructions

## Project Overview

SimConnect.NET is a managed C# wrapper for Microsoft Flight Simulator's SimConnect SDK, providing async/await patterns and high-level abstractions for flight simulation data access.

## Architecture & Core Components

### Three-Layer Design Pattern

-   **SimConnectClient**: Main entry point, manages connection lifecycle and background message processing
-   **Manager Layer**: Domain-specific managers (`SimVarManager`, `AircraftDataManager`, `SimObjectManager`)
-   **Native Layer**: P/Invoke bindings in `SimConnectNative.cs` with ANSI string marshaling

### Key Manager Responsibilities

-   **SimVarManager**: Dynamic SimVar operations with automatic data definition caching and async request handling
-   **AircraftDataManager**: High-level aircraft data aggregation (uses concurrent requests for efficiency)
-   **SimObjectManager**: AI object creation and lifecycle management

### Message Processing Pattern

The client runs a background message loop (`StartMessageProcessingLoopAsync`) with adaptive polling delays that routes messages based on `SimConnectRecvId` enum values. Each manager processes relevant messages via their own handlers.

## Development Conventions

### Code Style (StyleCop Enforced)

-   All public members require XML documentation with company copyright headers
-   Using directives outside namespace, newline at EOF required
-   `TreatWarningsAsErrors=true` in all projects except tests
-   File headers: `// <copyright file="FileName.cs" company="BARS">`

### Async Patterns

```csharp
// Concurrent requests for efficiency
var tasks = new[] {
    simVarManager.GetAsync<double>("VAR1", "unit"),
    simVarManager.GetAsync<double>("VAR2", "unit")
};
await Task.WhenAll(tasks);
```

### Error Handling

-   Use `SimConnectException` with `SimConnectError` enum for SimConnect-specific failures
-   `ObjectDisposedException.ThrowIf(disposed, nameof(Class))` for disposal checks
-   Debug.WriteLine for non-critical errors to avoid breaking message loops

## Build & Test Workflow

### Multi-targeting & Dependencies

-   Projects target both .NET 8.0 and 9.0 (`<TargetFrameworks>net8.0;net9.0</TargetFrameworks>`)
-   `SimConnect.dll` is bundled in `lib/` and copied to output (`CopyToOutputDirectory>PreserveNewest`)
-   NuGet package generation enabled with `GeneratePackageOnBuild=true`

### Test Structure

-   Tests implement `ISimConnectTest` interface with Name/Description/Category properties
-   Tests assume client is already connected and focus on specific functionality
-   No traditional test framework - custom test runner in `TestRunner.cs`
-   StyleCop disabled for test projects (`RunAnalyzersDuringBuild>false`)

### Build Commands

```bash
dotnet restore && dotnet build --configuration Release
dotnet pack src/SimConnect.NET/SimConnect.NET.csproj --output ./artifacts
```

## SimVar Integration Patterns

### Registry vs Dynamic Definitions

-   `SimVarRegistry.Get(name)` for predefined SimVars with type validation
-   Dynamic definitions created automatically for unregistered SimVars
-   Data definitions cached by `(Name, Unit)` tuple to avoid recreating

### Type Inference & Marshaling

-   Automatic `SimConnectDataType` inference from .NET types (int→Integer32, double→FloatDouble)
-   Boolean conversion: SimConnect int values (0/1) ↔ .NET bool
-   String handling with ANSI marshaling and configurable buffer sizes

## Common Integration Points

### P/Invoke Conventions

-   All SimConnect functions use ANSI string marshaling (`[MarshalAs(UnmanagedType.LPStr)]`)
-   Suppress CA2101 warning for ANSI marshaling requirement
-   IntPtr handle management with proper disposal patterns

### Concurrent Request Handling

-   Request/Response correlation via unique IDs (`nextRequestId`, `nextDefinitionId`)
-   `ConcurrentDictionary<uint, object>` for pending request tracking
-   TaskCompletionSource pattern for async request completion

When implementing new features, follow the established manager pattern, maintain StyleCop compliance, and use concurrent async patterns for SimVar operations.
