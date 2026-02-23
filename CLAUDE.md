# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is a library suite that sits between raw ASP.NET Core and consuming applications, providing:
- Modular bootstrapping (`RZ.AspNet.Bootstrapper`)
- API utilities and standardized error handling (`RZ.AspNet.Api`)
- Reactive MVVM framework for Blazor (`RZ.Foundation.Blazor`)
- MudBlazor UI component layer (`RZ.Foundation.Blazor.MudBlazor`)

All four projects are published as NuGet packages. The solution file is `RZ.Foundation.AspNet.slnx` (modern Visual Studio format).

## Commands

### Build & Pack NuGet packages
```powershell
# Pack all four projects to a destination directory
./build.ps1 <destination-path>
```

### Run tests
```bash
dotnet test tests/UnitTests/
```

### Run a single test
```bash
dotnet test tests/UnitTests/ --filter "FullyQualifiedName~TestClassName"
```

### Run the example app
```bash
dotnet run --project Examples/BlazorServer/
```

## Architecture

### Project Dependency Order
```
RZ.Foundation.Blazor.MudBlazor
    → RZ.Foundation.Blazor
        → RZ.AspNet.Bootstrapper
            → RZ.AspNet.Api
                → RZ.Foundation 8.x (parent framework, not in this repo)
```

### Global Usings (all projects inherit)
From `src/CommonGlobalUsings.cs`:
```csharp
global using JetBrains.Annotations;
global using LanguageExt;
global using static RZ.Foundation.AOT.Prelude;
global using static RZ.Foundation.Prelude;
```
This means functional programming types (`Option`, `Either`, etc.) and `[PublicAPI]` are available everywhere without imports.

### RZ.AspNet.Bootstrapper — Module Pattern

`AppModule` is the central abstraction. Each module implements two phases:
- `InstallServices()` — configures the DI container
- `InstallMiddleware()` — configures the ASP.NET pipeline

Modules are composed and executed via `RunPipeline()`:
```csharp
await app.RunPipeline(module1, module2, ...);
```

`CommonModules` provides factory methods for standard combinations. Lifecycle hooks use:
```csharp
builder.Services.AspHostEvents()
       .OnStart(() => ...)
       .OnShutdown(() => ...);
```

### RZ.AspNet.Api — API Utilities

`WebApiSettings` extension methods for `IHostApplicationBuilder`:
- `EnableHttpInformationLogging()` — logs HTTP details only for responses ≥ 300
- `EnableAllProxyForwards()` — configures X-Forwarded-* headers for proxy/gateway deployments
- `SetupAnyBearerTokenAuthentication()` — permissive bearer auth for API-gateway-protected services (does NOT validate tokens)

`ApiControllerBase` provides standardized error-to-HTTP-status mapping using `ErrorInfo` from RZ.Foundation.

### RZ.Foundation.Blazor — Reactive MVVM

**ViewModel hierarchy:**
- `ViewModel` → extends `ReactiveUI.ReactiveObject` (base reactive object)
- `AppViewModel` → adds `IServiceProvider`, `ILogger`, `IHostEnvironment`, `ShellViewModel`, and background task running with error propagation
- `ActivatableViewModel` → adds MVVM Light activation lifecycle

**`ShellViewModel`** manages application state:
- Navigation stack: `Push()`, `PushModal()`, `Replace()`, `CloseCurrentView()`
- Notification system via `Notify()`
- Dark mode, navigation drawer state, app mode (Page vs Modal)

**`ViewFinder`** resolves Views from ViewModels by naming convention: `MyViewModel` → searches for `MyView` or `My` component. Results are cached in a `ConcurrentDictionary`.

**`VmToolkit<T>`** is a DI toolkit injected into ViewModels, providing: `Logger<T>`, `IHostEnvironment`, `ShellViewModel`, `IServiceProvider`, `EventBubble`.

**`EventBubble`** enables decoupled event communication between components.

Registration:
```csharp
builder.Services.AddRzBlazorSettings();
```

### RZ.Foundation.Blazor.MudBlazor — UI Layer

Wraps `RZ.Foundation.Blazor` with MudBlazor components. Key reusable components in the `BlazorViews` namespace: `DualPanel`, `ShellAppBar`, `ShellNavMenu`, `ViewStack`.

Registration:
```csharp
builder.Services.AddMudServices();
builder.Services.AddRzMudBlazorSettings(); // registers MudBlazor + RZ.Foundation.Blazor
```

`SeverityMapper` maps `MessageSeverity` → MudBlazor `Severity`.

### Localization Note

For Blazor Server with interactive rendering, the only reliable way to set locale is at the Razor page level:
```razor
@{ CultureInfo.CurrentUICulture = new(lang); }
```
Neither `SetParametersAsync` nor redirect-based approaches work in all rendering modes.

## Key Conventions

- **Target framework:** .NET 10 for library projects, .NET 9 for tests
- **Language version:** `preview` (latest C# features in use)
- **Nullable:** enabled (strict)
- **Error types:** Use `ErrorInfo` / `ErrorInfoException` from RZ.Foundation for domain errors
- **Public API surface:** Mark with `[PublicAPI]` attribute
- **Async:** Prefer `ValueTask` for hot-path async operations
- **Centralized package versions:** All NuGet versions managed in `Directory.Packages.props`
