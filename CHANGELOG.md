# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
