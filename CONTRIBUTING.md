# Contributing to SimConnect.NET

Thank you for your interest in contributing to SimConnect.NET! This guide will help you get started with contributing to our managed C# wrapper for Microsoft Flight Simulator's SimConnect SDK.

## Getting Started

### Prerequisites

-   **.NET 8.0 or 9.0 SDK**: Download from [Microsoft .NET](https://dotnet.microsoft.com/download)
-   **Visual Studio 2022** or **Visual Studio Code**: For development environment
-   **Microsoft Flight Simulator**: Required for testing SimConnect functionality
-   **Git**: For version control and collaboration

### Development Setup

1. **Fork and Clone**

    ```bash
    git clone https://github.com/stopbars/SimConnect.NET.git
    cd SimConnect.NET
    ```

2. **Restore Dependencies**

    ```bash
    dotnet restore
    ```

3. **Build the Project**

    ```bash
    dotnet build --configuration Debug
    ```

4. **Run Tests** (Optional - requires Flight Simulator)

    ```bash
    cd tests/SimConnect.NET.Tests.Net8
    dotnet run
    ```

## Development Guidelines

### Code Style

-   **StyleCop Enforcement**: All code must pass StyleCop analysis with zero warnings
-   **XML Documentation**: All public members require comprehensive XML documentation
-   **File Headers**: All source files must include the BARS copyright header:
    ```csharp
    // <copyright file="FileName.cs" company="BARS">
    // Copyright (c) BARS. All rights reserved.
    // </copyright>
    ```
-   **Modern C# Patterns**: Use latest C# language features, nullable reference types, and async/await patterns

### Architecture Guidelines

-   **Three-Layer Design**: Follow the established pattern of Client → Manager → Native layers
-   **Async/Await**: All SimConnect operations must use async patterns with proper cancellation support
-   **Concurrent Operations**: Use `Task.WhenAll()` for concurrent SimVar requests where appropriate
-   **Error Handling**: Use `SimConnectException` with appropriate `SimConnectError` enum values
-   **Resource Management**: Implement proper disposal patterns for unmanaged resources

### Naming Conventions

-   **Managers**: End with "Manager" (e.g., `SimVarManager`, `AircraftDataManager`)
-   **Events**: End with "EventArgs" for event argument classes
-   **Enums**: Use Pascal case with descriptive names matching SimConnect conventions
-   **SimVars**: Use exact SimConnect variable names in `SimVarRegistry`

## Contribution Process

### 1. Find or Create an Issue

-   Browse existing issues for bug fixes or feature requests
-   Create a new issue for significant changes or new features
-   Discuss the approach and implementation details before starting work
-   Use appropriate issue templates and labels

### 2. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/your-bug-fix
```

### 3. Make Your Changes

-   Follow the established architecture patterns and coding standards
-   Write comprehensive XML documentation for all public APIs
-   Ensure all code passes StyleCop analysis
-   Update relevant tests and documentation
-   Test with Microsoft Flight Simulator when applicable

### 4. Commit Your Changes

```bash
git add .
git commit -m "Add brief description of your changes"
```

Use clear, descriptive commit messages following conventional commits:

-   `feat: add support for custom SimVar definitions`
-   `fix: resolve SimConnect reconnection issue`
-   `docs: update SimVarManager API documentation`
-   `test: add integration tests for AircraftDataManager`

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Create a pull request with:

-   Clear description of changes and their purpose
-   Reference to related issues using `Fixes #123` or `Closes #123`
-   Test results and validation steps
-   Breaking changes documentation (if applicable)

## Testing

### Test Structure

SimConnect.NET uses a custom test framework designed for flight simulation scenarios:

1. **Test Interface**: All tests implement `ISimConnectTest` with Name, Description, and Category properties
2. **Test Runner**: Execute tests using the custom `TestRunner` in the test projects
3. **Integration Testing**: Tests assume an active SimConnect connection to Flight Simulator
4. **Category-based Organization**: Tests are organized by functionality (SimVar, Aircraft, AI Objects, etc.)

### Running Tests

```bash
# Build test project
cd tests/SimConnect.NET.Tests.Net8
dotnet build

# Run all tests (requires Flight Simulator running)
dotnet run

# Alternative: Run specific test categories
dotnet run -- --category SimVar
```

### Writing New Tests

When adding new functionality, create corresponding tests:

```csharp
public class YourNewTest : ISimConnectTest
{
    public string Name => "Your Test Name";
    public string Description => "Detailed description of what this test validates";
    public string Category => "TestCategory";

    public async Task<bool> RunAsync(SimConnectClient client)
    {
        // Test implementation
        // Return true for success, false for failure
    }
}
```

## Common Issues

### SimConnect Connection Issues

-   Ensure Microsoft Flight Simulator is running before executing tests
-   Verify SimConnect.dll is present in the output directory
-   Check Windows Event Viewer for SimConnect-specific errors
-   Confirm Flight Simulator's SimConnect is enabled in settings

### Build and StyleCop Issues

-   All warnings are treated as errors (`TreatWarningsAsErrors=true`)
-   Missing XML documentation will cause build failures
-   File headers must include proper copyright information
-   Using directives must be placed outside namespace declarations

## Getting Help

-   **Discord**: Join the BARS [Discord](https://stopbars.com/discord) server for real-time development help
-   **GitHub Issues**: [Create an issue](https://github.com/stopbars/SimConnect.NET/issues/new) for bugs or feature requests
-   **XML Documentation**: Review the comprehensive XML documentation in the source code
-   **Code Review**: Request reviews for complex changes or architectural decisions

## Documentation

Currently, SimConnect.NET relies on comprehensive XML documentation for all public APIs, which is enforced during build.

**Future Documentation Plans (TODO):**

-   API reference documentation generated from XML comments
-   Code examples and tutorials for common scenarios
-   Integration guides for different flight simulation use cases

## Multi-Targeting Support

SimConnect.NET targets both .NET 8.0 and 9.0. When contributing:

-   Ensure compatibility with both target frameworks
-   Use language features available in the lowest supported version
-   Test builds against both frameworks when possible
-   Consider performance implications across different runtime versions

## NuGet Package Guidelines

When making changes that affect the public API:

-   Update version numbers in `Directory.Build.props`
-   Follow semantic versioning principles
-   Document breaking changes in pull request descriptions
-   Consider backward compatibility for minor version updates

## Recognition

Contributors are recognized in:

-   Release notes for significant contributions
-   BARS website credits page (coming soon)
-   BARS Discord Role (@Contributer)

Your contributions directly support the ongoing development and improvement of SimConnect.NET and the broader BARS ecosystem. By getting involved, you help us build a more robust, performant, and feature-rich library that benefits the entire flight simulation development community.
