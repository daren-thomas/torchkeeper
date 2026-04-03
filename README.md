# TorchKeeper

A .NET MAUI character sheet app for Shadowdark RPG.

## Prerequisites

- .NET 10 SDK
- macOS with Xcode installed (for Mac Catalyst / iOS)
- .NET workloads: `ios`, `maccatalyst`

## Build

```bash
dotnet build TorchKeeper/TorchKeeper.csproj
```

## Run

```bash
dotnet run --project TorchKeeper/TorchKeeper.csproj -f net10.0-maccatalyst
```

Alternatively, use **Rider** or **Visual Studio** and select `net10.0-maccatalyst` as the target framework.

## Tests

```bash
dotnet test TorchKeeper.Tests/
```

## Notes

- Android target has been removed (no Android device available for testing).
- `$(MauiVersion)` fallback is hardcoded to `10.0.41` in the csproj for environments where the `maui` workload is not registered.
