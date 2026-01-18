# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GroveGames.DependencyInjection is a lightweight dependency injection framework for .NET and Godot with Native AOT support. The library provides hierarchical container management, constructor injection, and method injection via attributes. It targets .NET 10.0 for modern features and netstandard2.1 for Godot compatibility.

## Project Structure

The repository contains two main project types:

- **Core Library** (`src/GroveGames.DependencyInjection/`): The main .NET library targeting both `net10.0` and `netstandard2.1`
- **Godot Integration** (`src/GroveGames.DependencyInjection.Godot/`): Godot-specific extensions and plugin (addon only, no csproj)

The architecture is built around several key abstractions:

1. **Core Interfaces**: `IContainer`, `IContainerBuilder`, `IObjectResolver` define the DI contracts
2. **Container Types**: `Container` (child containers), `RootContainer` (application root)
3. **Lifetime Management**: Singleton and Transient registration patterns
4. **Injection Patterns**: Constructor injection and method injection via `[Inject]` attribute
5. **Installer Pattern**: `IInstaller` interface for modular dependency registration

## Development Commands

### Building
```bash
dotnet build                           # Build all projects
dotnet build -c Release               # Release build
```

### Testing
```bash
dotnet test                           # Run all tests
dotnet test tests/GroveGames.DependencyInjection.Tests/        # Core library tests
```

### Formatting
```bash
dotnet format                         # Format all code according to .editorconfig
dotnet format --verify-no-changes    # Check if code is properly formatted (CI/CD)
dotnet format whitespace             # Format whitespace only
dotnet format style                  # Apply code style fixes
```

### Packaging
```bash
dotnet pack -c Release                # Create NuGet packages
```

## Code Style & Formatting

The project uses automated formatting via GitHub Actions and comprehensive configuration files:

### Configuration Structure

The project uses a layered configuration approach:

#### Root Configuration (Core .NET Library)
- **EditorConfig** (`.editorconfig`): Core C# coding standards, 4-space indentation, naming conventions (private fields: `_camelCase`, static: `s_camelCase`, interfaces: `IPascalCase`)
- **Git Configuration** (`.gitignore`/`.gitattributes`): Focused on .NET build outputs, NuGet packages, IDE files, and basic OS artifacts
- **VS Code Settings** (`.vscode/settings.json`): C# formatting and save actions

#### Godot-Specific Configuration (`sandbox/GodotApplication/`)
- **EditorConfig** (`.editorconfig`): Inherits core C# settings plus Godot scene files (`.tscn`/`.tres`) with 2-space indentation
- **Git Configuration** (`.gitignore`/`.gitattributes`): Godot-specific files (`.godot/`, `.import/`), C# project artifacts, binary asset handling
- **Plugin Configuration** (`src/GroveGames.DependencyInjection.Godot/addons/GroveGames.DependencyInjection/plugin.cfg`): Godot plugin metadata
- **Project Configuration** (`project.godot`): Godot 4.3+ project with C# support

### Target Frameworks & Features
- **Multi-targeting**: `net10.0` (with AOT support) and `netstandard2.1` (for Godot compatibility)
- **Nullable Reference Types**: Enabled across the project
- **AOT Compatibility**: The `net10.0` target includes AOT analyzers and trimming support
- **Polyfills via Extension Members**: Custom polyfills using C# 14 extension members (`extension(Type)` syntax) in `Polyfills/` folder provide backward compatibility for netstandard2.1. These use `#if !NET6_0_OR_GREATER` preprocessor directives and are placed in the `System` namespace for seamless usage:
  - `CallerArgumentExpressionAttribute` for parameter name capture
  - `ArgumentNullException.ThrowIfNull` static method
- **Code Formatting**:
  - Automatic formatting on save configured in VS Code settings
  - `dotnet format` command respects all `.editorconfig` settings
  - GitHub Actions enforce formatting via `--verify-no-changes` flag
  - Supports whitespace, style, and analyzer-based formatting

## Platform Integration Structure

### Godot Addon Structure (`src/GroveGames.DependencyInjection.Godot/addons/GroveGames.DependencyInjection/`)
Standard Godot addon layout:
- `plugin.cfg` - Godot plugin configuration
- `Plugin.cs` - Plugin entry point (EditorPlugin)
- `LICENSE` → symlink to root LICENSE
- `README.md` → symlink to root README.md

The Godot addon requires the NuGet package (`GroveGames.DependencyInjection`) plus the addon files extracted to project's `addons/` folder.

## Testing Framework

- **Test Framework**: xUnit v3
- **Test Projects**:
  - `GroveGames.DependencyInjection.Tests` (core functionality)
- **Test Configuration**: Uses `xunit.runner.json` for xUnit configuration

## Build Configuration

Key build configurations:

- **Multi-targeting**: Projects support both modern .NET and legacy .NET Standard
- **AOT Support**: Native AOT compilation enabled for `net10.0` target
- **Documentation**: XML documentation generation enabled for all projects
- **Package Properties**: Centralized in `Directory.Build.props`

## SDK Version

The project targets .NET 10.0 SDK (see `global.json`). When working with this codebase, ensure you have .NET 10.0 SDK installed.

## GitHub Workflows

The project uses reusable workflows from `grovegs/workflows`:

- **Tests** (`tests.yml`): Runs on pushes/PRs to main/develop branches
- **Format** (`format.yml`): Validates code formatting
- **Release** (`release.yml`): Manual workflow for creating releases and publishing NuGet packages

## Development Sandbox

The `sandbox/` directory contains sample applications for testing and development:

- **ConsoleApplication**: Basic .NET console app for testing core functionality
- **GodotApplication**: Godot project with the DI addon for testing Godot integration
- **DotnetBenchmark**: Performance benchmarking application

### Godot Sandbox Setup

The Godot sandbox project uses a symlink to reference the Godot addon locally:

- `addons/GroveGames.DependencyInjection` → symlink to `src/GroveGames.DependencyInjection.Godot/addons/GroveGames.DependencyInjection`

These sandbox projects are useful for:
- Testing changes across different platforms
- Demonstrating usage examples
- Performance testing and benchmarking
- Integration validation

## Key Dependencies

- **Microsoft.SourceLink.GitHub**: For source linking in packages

Note: Custom polyfill extensions in the `System` namespace provide backward compatibility for netstandard2.1.

## DI Container Architecture

### Container Hierarchy

The DI system supports hierarchical containers:

1. **RootContainer**: Application-level container, created via `RootContainerFactory`
2. **Container**: Child containers that inherit from parent, created via `ContainerFactory`

### Registration Methods

- `AddSingleton<T>()` - Single instance for the container lifetime
- `AddSingleton<TRegistration, TImplementation>()` - Interface to implementation mapping
- `AddSingleton(instance)` - Pre-created instance registration
- `AddSingleton(factory)` - Factory-based lazy creation
- `AddTransient<T>()` - New instance per resolution
- `AddTransient<TRegistration, TImplementation>()` - Interface to implementation mapping
- `AddTransient(factory)` - Factory-based creation per resolution

### Injection Patterns

1. **Constructor Injection**: Dependencies resolved via constructor parameters
2. **Method Injection**: Methods marked with `[Inject]` attribute are called after construction

### Installer Pattern

Use `IInstaller` for modular dependency registration:

```csharp
public class GameInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.AddSingleton<IGameService, GameService>();
    }
}
```
