# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.13-beta] - 2025-08-24

### Fixed

-   Defaults for `SimConnect_AddToDataDefinition` updated to match SDK, allowing multiple variables to be added to the same definition without overwriting each other.

### Changed

-   Synced defaults with MSFS SDK header `SimConnect.h` (SIMCONNECT_DATATYPE default to `SIMCONNECT_DATATYPE_FLOAT64` = 4, and `SIMCONNECT_UNUSED` for datumId = `0xFFFFFFFF`).
-   Updated P/Invoke signature in `SimConnectNative.SimConnect_AddToDataDefinition` to reflect these defaults.

### Notes

-   This unblocks dynamic structure retrieval scenarios where multiple fields are added to a single definition.

## [0.1.12-beta] - 2025-08-23

### Fixed

-   GetInputEventAsync: Correctly reads value bytes dynamically based on the reported type (double vs string) after parsing the header.
-   Hash assignment: Now sets the `Hash` from the requested hash rather than using the response `RequestId`.

### Changed

-   Removed `None` from `SimConnectInputEventType` (SDK header and observed behavior indicate only `DOUBLE` and `STRING` are valid).
-   Introduced `SimConnectRecvGetInputEventHeader` (no value field) and updated marshalling to read the header first, then parse the value based on type and remaining bytes.
-   Adjusted GetInputEventAsync response handling to use the new header-first pattern.

### Notes

-   Verified against SDK headers (`SIMCONNECT_INPUT_EVENT_TYPE_DOUBLE`, `SIMCONNECT_INPUT_EVENT_TYPE_STRING`) and by testing against enumerated input events.
-   The standard test case now detects the double value correctly.
-   API note: removing the `None` enum member may be a compile-time change if any consumer referenced it; valid types are now Double and String.

## [0.1.11-beta] - 2025-08-22

### Fixed

-   Corrected SetInputEvent to accept hash values as ulong, resolving type-mismatch issues.

## [0.1.10-beta] - 2025-08-21

### Added

-   Microsoft Flight Simulator 2024 detection logic (client now identifies whether it is connected to MSFS 2020 or MSFS 2024, enabling future version-specific behavior).

### Changed

-   Enhanced `SimConnectRecvOpen` structure handling to surface additional metadata required for differentiating simulator platform/version.

### Notes

-   Version detection is currently informational (no conditional behavior yet) but provides groundwork for future feature toggles or compatibility shims.

## [0.1.9-beta] - 2025-08-20

### Added

-   Client event transmission support via `InputEventManager.TransmitClientEventAsync` and `TransmitClientEventEx1Async`, including new `SimConnectEventOptions` enum.

### Changed

-   NuGet README sample updated (`SimVarManager` usage now accessed via shorthand `SimVars` property):
    -   `var altitude = await client.SimVarManager.GetAsync<double>("PLANE ALTITUDE", "feet");` → `var altitude = await client.SimVars.GetAsync<double>("PLANE ALTITUDE", "feet");`
    -   `var airspeed = await client.SimVarManager.GetAsync<double>("AIRSPEED INDICATED", "knots");` → `var airspeed = await client.SimVars.GetAsync<double>("AIRSPEED INDICATED", "knots");`

### Fixed

-   Improved test feedback: `TestInputEventGetSet` now detects and reports an unknown/none value type instead of failing silently (added explicit failure branch when the current value type is `None`). This aids diagnosing aircraft-specific input event behavior (e.g., C172, SR22T).

-   SimVar request timeout handling: added `SimVarManager.RequestTimeout` (default 10s) to properly fail stalled requests instead of hanging performance/stress tests. Requests now throw `TimeoutException` when exceeded.

## [0.1.8-beta] - 2025-08-10

### Added

-   Public `SimConnectClient.Handle` property (was internal) to allow advanced consumers to perform custom native interop scenarios not yet wrapped by the library.
-   `SimConnectClient.RawMessageReceived` event exposing a low-level hook with raw pointer, size, and message id for diagnostics, custom decoding, or experimentation with unwrapped message types.

### Notes

-   Raw message memory is only valid for the duration of the event callback; copy data immediately if you need to retain it.
-   This is an additive, non-breaking update.

## [0.1.7-beta] - 2025-08-10

### Added

-   Packaged native `SimConnect.dll` inside the NuGet under `runtimes/win-x64/native`, enabling automatic deployment of the unmanaged dependency.

### Changed

-   Updated project file to treat `lib/SimConnect.dll` as packed content (`Content` with `<Pack>true</Pack>` and RID-specific package path) instead of a build-only copy item.

### Notes

-   Fixes `DllNotFoundException` (`Unable to load DLL 'SimConnect.dll'`) encountered by consumers of previous versions unless they manually supplied the DLL.
-   If you previously worked around the issue by copying the DLL manually, you can remove that step after upgrading.
-   Architecture currently targets Windows x64 (MSFS is x64); additional RIDs can be added in future if needed.

## [0.1.6-beta.1] - 2025-08-09

### Fixed

-   Corrected PackageReleaseNotes extraction (supersedes unlisted 0.1.6-beta which had a packaging metadata issue).

## [0.1.6-beta] - 2025-08-09

### Fixed

-   Fixed auto-generated PackageReleaseNotes extraction from CHANGELOG.md.

## [0.1.5-beta] - 2025-08-09

### Added

-   AI SimObject data setting: new `SimObjectManager.SetDataAsync<T>` for setting individual SimVars on spawned AI objects (e.g. custom visibility toggles like `BARS_LIGHT_GREEN`)
-   Batch setting support via `SimObjectManager.SetDataBatchAsync` to concurrently push multiple SimVar values to an AI object for lower latency updates
-   Automated packaging: NuGet PackageReleaseNotes now auto-derived from the latest version section of CHANGELOG.md via MSBuild target `DerivePackageReleaseNotes`.

### Notes

-   Non-breaking additive API; leverages existing dynamic SimVar definition caching. Pass an empty unit string for unit-less custom vars. Inactive objects throw `InvalidOperationException` to prevent silent failures.

## [0.1.4-beta] - 2025-08-08

### Added

-   SimConnectLogger: lightweight, async, file-based logger with severity levels; persists diagnostics under LocalAppData; optional debug mirroring
-   SimConnectErrorMapper: descriptive mappings from native error codes to SimConnectError with helpers to wrap as SimConnectException; no runtime behavior changes

### Changed

-   Centralized logging across AI/SimObject, input events, SimVar, and core client (message dispatch/reconnect), replacing scattered Debug.WriteLine calls
-   Level-appropriate messages to improve signal and reduce noise
-   Error handling paths now route through SimConnectErrorMapper for consistent, actionable messages

### Notes

-   Focus on observability and diagnostics; no public API changes; non-breaking

## [0.1.3-beta] - 2025-08-07

### Added

-   Enhanced build configuration for better debugging and CI/CD support
-   Portable debug symbols generation for improved debugging experience
-   Deterministic builds for reproducible packages
-   Source linking support for better debugging in CI environments

### Fixed

-   Fixed package logo not being included (corrected filename from icon.png to logo.png)

### Changed

-   Enabled portable debug type and debug symbols for better debugging support
-   Added deterministic build configuration for consistent package generation
-   Configured continuous integration build detection for enhanced CI/CD workflows
-   Added source root mapping for improved source linking in CI environments

## [0.1.2-beta] - 2025-08-07

### Changed

-   Updated PackageReleaseNotes to reference CHANGELOG.md for better version history tracking

## [0.1.1-beta] - 2025-08-07

### Added

-   NuGet-specific README.md for cleaner package documentation
-   Improved package metadata and documentation

### Changed

-   Separated GitHub README from NuGet package README for better presentation

## [0.1.0-beta] - 2025-08-07

### Added

-   Initial beta release of SimConnect.NET
-   SimConnectClient with async/await patterns and background message processing
-   SimVarManager for dynamic SimVar operations with automatic data definition caching
-   SimVarRegistry system for predefined SimVars with type validation
-   AircraftDataManager for high-level aircraft data aggregation (uses concurrent requests for efficiency)
-   SimObjectManager for AI object creation and lifecycle management
-   InputEventManager for input event handling and controller enumeration
-   InputGroupManager for organizing and prioritizing input events
-   Auto-reconnection capabilities with configurable attempts and delays
-   Connection health monitoring and status events
-   Comprehensive facility data support (airports, VOR/NDB stations, waypoints, jetways)
-   Extensive enum definitions for navigation aids, airports, and flight planning
-   Comprehensive P/Invoke bindings for SimConnect SDK with ANSI marshaling
-   Multi-targeting support for .NET 8.0 and 9.0
-   NuGet package configuration with symbol packages
-   Automated CI/CD pipeline with version detection and NuGet deployment

### Security

-   ANSI string marshaling for safe P/Invoke operations
-   Proper disposal patterns for unmanaged resources
-   Thread-safe concurrent collections for request handling
