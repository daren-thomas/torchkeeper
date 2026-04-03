# SD Character Sheet

A .NET MAUI character sheet app for Shadowdark RPG.

## Prerequisites

- .NET 10 SDK
- macOS with Xcode installed (for Mac Catalyst / iOS)
- .NET workloads: `ios`, `maccatalyst`

## Build

```bash
dotnet build SdCharacterSheet/SdCharacterSheet.csproj
```

## Run

Use **Rider** or **Visual Studio** — both handle Mac Catalyst code signing automatically.

Select `net10.0-maccatalyst` as the target framework and hit Run.

> `dotnet build -t:Run` does not work for Mac Catalyst from the command line because
> the bundling step requires code signing, which only the IDE tooling sets up.

## Tests

```bash
dotnet test SdCharacterSheet.Tests/
```

## Notes

- Android target has been removed (no Android device available for testing).
- `$(MauiVersion)` fallback is hardcoded to `10.0.41` in the csproj for environments where the `maui` workload is not registered.
