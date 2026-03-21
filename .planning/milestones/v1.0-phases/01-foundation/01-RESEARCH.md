# Phase 1: Foundation - Research

**Researched:** 2026-03-08
**Domain:** .NET 10 MAUI — domain model, file I/O, JSON import/save, cross-platform file picker
**Confidence:** HIGH (official docs verified; one known library issue documented with workaround)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **Target: .NET 10** (LTS, released Nov 2025, supported Nov 2025–Nov 2028)
- **All four platforms from day one**: net10.0-windows, net10.0-maccatalyst, net10.0-ios, net10.0-android
- **Single MAUI app project** — no separate class library; domain model lives inside the MAUI project
- **Windows: Unpackaged** (no MSIX) — simpler development, direct filesystem access, no sandbox
- **Windows distribution: self-contained executable / zip** — xcopy deploy, no installer required
- **OS file type association (.sdchar → app): nice-to-have, not required for Phase 1** — file picker is sufficient
- **Import scope**: `rolledStats` (not `stats`), `bonuses[]`, `maxHitPoints`, `XP`, `gear[]`, `magicItems[]`, `spellsKnown`; currency from `gold`/`silver`/`copper` fields directly, falling back to ledger sum
- **Silently ignored fields**: `stats`, `levels[]`, `armorClass`, `gearSlotsTotal`, `gearSlotsUsed`, `ledger[]`, `edits[]`, `ambitionTalentLevel`, `creationMethod`, `coreRulesOnly`, `activeSources`, and any unknown future fields (use `JsonIgnoreCondition`)
- **Currency import**: check for dedicated current-coins field first, fall back to summing `ledger[]` entries; store totals only, discard history
- **Initial HP on import**: `currentHP = maxHP`
- **AC model**: free-form contributor list (label + value), NOT derived from typed gear items; Shadowdarklings `armorClass` value is discarded on import
- **Save format**: JSON (System.Text.Json), separate `CharacterSaveData` DTO with version field, file extension `.sdchar` — never serialize ViewModel
- **Gear slots rule**: `max(STR stat, 10)` (not just STR score); 100 coins = 1 slot, first 100 coins of any denomination free
- **`bonusTo` format**: `"DEX:+2"` — maps to stat bonus model; `bonusTo` values starting with `"AC:"` map to AC contributor list

### Claude's Discretion
- Internal DTO schema design (property names, nesting)
- Null handling for optional fields in the Shadowdarklings JSON
- JSON serialization options (indented vs compact for .sdchar files)
- Error handling approach for malformed import files

### Deferred Ideas (OUT OF SCOPE)
- OS-level file type association (.sdchar double-click to open) — future phase or post-v1
- Ledger/transaction history view — v2 requirement (LEDG-01, LEDG-02), explicitly deferred
- Auto-save / IsDirty pattern — v2 requirement (PLSH-02), deferred
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| FILE-01 | User can import a Shadowdarklings.net JSON export to bootstrap a character | Import pipeline: `ShadowdarklingsImportService`, System.Text.Json with `JsonIgnoreCondition.WhenWritingDefault`, mapping rules from real example JSON |
| FILE-02 | User can load a character from a native .sdchar file | FilePicker (open), `CharacterFileService`, JSON deserialize `CharacterSaveData` DTO |
| FILE-03 | User can save a character to a .sdchar file | FileSaver (CommunityToolkit.Maui), `CharacterFileService`, JSON serialize `CharacterSaveData` DTO with version field |
</phase_requirements>

---

## Summary

Phase 1 establishes the foundation of the Shadowdark character sheet app: a domain model (Character, BonusSource, GearItem, MagicItem, AcComponent), a native `.sdchar` save format backed by a `CharacterSaveData` DTO, and an import pipeline that reads a Shadowdarklings JSON export. There is no UI in this phase — outputs are services and model objects.

The stack is .NET 10 MAUI targeting all four platforms. JSON work uses the built-in `System.Text.Json`. File open/save dialogs use MAUI's built-in `FilePicker` (open) and `CommunityToolkit.Maui`'s `FileSaver` (save-as dialog). The MVVM layer uses `CommunityToolkit.Mvvm` (8.4.0), though a `<LangVersion>preview</LangVersion>` workaround is required for .NET 10 compatibility until the library publishes a patched release.

The real Shadowdarklings export example (`examples/Brim.json`) confirms the field names, nesting, and data types documented in CONTEXT.md. The currency strategy (prefer `gold`/`silver`/`copper` top-level fields, fall back to ledger sum) is validated by the example which has both fields and a `ledger[]` array.

**Primary recommendation:** Scaffold the MAUI project first with `dotnet new maui`, set `WindowsPackageType=None` and `<LangVersion>preview</LangVersion>` immediately, then build domain model → DTO → services in that order. Do not serialize the ViewModel.

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET MAUI | 10.0 (ships with .NET 10) | Cross-platform framework | Locked decision; LTS through 2028 |
| System.Text.Json | Inbox (no package needed) | JSON serialization for .sdchar and import | Built-in, no extra dependency, fast |
| CommunityToolkit.Maui | 14.0.1 | `FileSaver` save dialog across all platforms | Official extension; FileSaver is not in MAUI core |
| CommunityToolkit.Mvvm | 8.4.0 | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` | Standard MAUI MVVM toolkit, source-generator based |

### Supporting (testing)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| xunit | 2.x | Unit tests for services, domain model | Separate `net10.0` class library test project |
| Microsoft.NET.Test.Sdk | latest | Test host runner | Required for `dotnet test` |
| xunit.runner.visualstudio | latest | VS test runner integration | Required for VS Test Explorer |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| System.Text.Json | Newtonsoft.Json | Newtonsoft is more forgiving of quirks but adds a dependency; System.Text.Json is sufficient for this use case |
| CommunityToolkit.Maui FileSaver | LukeMauiFilePicker (third-party) | LukeMauiFilePicker exists because MAUI FileSaver had bugs, but 14.x is stable; prefer official toolkit |
| xUnit | NUnit / MSTest | All three work; xUnit is idiomatic for .NET greenfield projects |

### Installation
```bash
dotnet new maui -n SdCharacterSheet
cd SdCharacterSheet

# MAUI Community Toolkit (FileSaver, etc.)
dotnet add package CommunityToolkit.Maui --version 14.0.1

# MVVM Community Toolkit (ObservableObject, RelayCommand)
dotnet add package CommunityToolkit.Mvvm --version 8.4.0

# Unit test project (separate, targets net10.0 not net10.0-platform)
dotnet new xunit -n SdCharacterSheet.Tests
cd SdCharacterSheet.Tests
dotnet add reference ../SdCharacterSheet/SdCharacterSheet.csproj
```

---

## Architecture Patterns

### Recommended Project Structure
```
SdCharacterSheet/
├── Models/
│   ├── Character.cs            # Domain entity (plain C#, no MAUI deps)
│   ├── BonusSource.cs          # Stat bonus / AC contributor
│   ├── GearItem.cs             # Mundane gear item
│   └── MagicItem.cs            # Magic item
├── DTOs/
│   ├── CharacterSaveData.cs    # .sdchar serialization target (versioned)
│   └── ShadowdarklingsJson.cs  # Deserialization shape for import JSON
├── Services/
│   ├── CharacterFileService.cs # Save/load .sdchar
│   └── ShadowdarklingsImportService.cs # Parse export, produce Character
├── ViewModels/
│   └── CharacterViewModel.cs   # ObservableObject; Phase 2 binds views to this
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   │   └── Info.plist          # Custom UTType for .sdchar
│   ├── MacCatalyst/
│   │   └── Entitlements.plist  # File access sandbox entitlement
│   └── Windows/
└── MauiProgram.cs              # DI registration
SdCharacterSheet.Tests/
├── Services/
│   ├── CharacterFileServiceTests.cs
│   └── ShadowdarklingsImportServiceTests.cs
└── Models/
    └── CharacterTests.cs
```

### Pattern 1: DTO Separation (save format)
**What:** `CharacterSaveData` is the serialization target for `.sdchar`. `CharacterViewModel` is never serialized. The file service maps between the two.
**When to use:** Always — the locked decision prohibits ViewModel serialization.
**Example:**
```csharp
// CharacterSaveData.cs  — the DTO
public class CharacterSaveData
{
    public int Version { get; init; } = 1;
    public string Name { get; init; } = "";
    public string Class { get; init; } = "";
    public string Ancestry { get; init; } = "";
    public int Level { get; init; }
    // ... all other fields ...
    public List<BonusSourceData> Bonuses { get; init; } = [];
    public List<GearItemData> Gear { get; init; } = [];
    public List<MagicItemData> MagicItems { get; init; } = [];
}

// CharacterFileService.cs
public async Task SaveAsync(CharacterViewModel vm, CancellationToken ct)
{
    var dto = MapToDto(vm);                          // ViewModel → DTO
    var json = JsonSerializer.Serialize(dto, _options);
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
    var result = await _fileSaver.SaveAsync("character.sdchar", stream, ct);
    result.EnsureSuccess();
}

public async Task<CharacterSaveData?> LoadAsync(CancellationToken ct)
{
    var result = await FilePicker.Default.PickAsync(_pickOptions);
    if (result is null) return null;
    using var stream = await result.OpenReadAsync();
    return await JsonSerializer.DeserializeAsync<CharacterSaveData>(stream, _options);
}
```

### Pattern 2: Shadowdarklings Import Shape
**What:** A separate DTO (`ShadowdarklingsJson`) mirrors only the fields we read. `JsonIgnoreCondition.WhenWritingDefault` plus unknown-field tolerance handles extra fields.
**When to use:** Import only — never reuse this shape for save/load.
**Example:**
```csharp
// ShadowdarklingsJson.cs — import deserialization shape
// Source: examples/Brim.json (confirmed field names)
public class ShadowdarklingsJson
{
    public string Name { get; set; } = "";
    public string Class { get; set; } = "";
    public string Ancestry { get; set; } = "";
    public int Level { get; set; }
    public string Title { get; set; } = "";
    public string Alignment { get; set; } = "";
    public string Background { get; set; } = "";
    public string Deity { get; set; } = "";
    public string Languages { get; set; } = "";
    public int XP { get; set; }
    public int MaxHitPoints { get; set; }
    public string SpellsKnown { get; set; } = "";

    // Use rolledStats, NOT stats
    public StatBlock? RolledStats { get; set; }

    // Currency: prefer top-level fields; ledger is fallback
    public int? Gold { get; set; }
    public int? Silver { get; set; }
    public int? Copper { get; set; }
    public List<LedgerEntry>? Ledger { get; set; }

    public List<SdBonus>? Bonuses { get; set; }
    public List<SdGearItem>? Gear { get; set; }
    public List<SdMagicItem>? MagicItems { get; set; }
    public List<string>? Attacks { get; set; }

    // All other fields silently ignored by System.Text.Json default behavior
}

public class StatBlock
{
    public int STR { get; set; }
    public int DEX { get; set; }
    public int CON { get; set; }
    public int INT { get; set; }
    public int WIS { get; set; }
    public int CHA { get; set; }
}

public class SdBonus
{
    public string BonusName { get; set; } = "";
    public string BonusTo { get; set; } = "";      // e.g. "DEX:+2" or "AC:+1"
    public string SourceType { get; set; } = "";
    public string SourceCategory { get; set; } = "";
    public int GainedAtLevel { get; set; }
    public string SourceName { get; set; } = "";
}
```

### Pattern 3: System.Text.Json Options (shared static)
**What:** Reuse one `JsonSerializerOptions` instance across all calls to avoid metadata cache rebuilding.
**When to use:** Always — creating a new instance per call is a documented performance pitfall.
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/configure-options
private static readonly JsonSerializerOptions ImportOptions = new()
{
    PropertyNameCaseInsensitive = true,     // Shadowdarklings uses camelCase
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    // Unknown fields are silently ignored by default in System.Text.Json
};

private static readonly JsonSerializerOptions SaveOptions = new()
{
    WriteIndented = true,   // Human-readable .sdchar files; size is negligible
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
};
```

### Pattern 4: FilePickerFileType for .sdchar
**What:** Platform-specific file type declarations for the open dialog.
**When to use:** Both FilePicker (open) and FileSaver (save-as) need this.
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker
// iOS: requires UTImportedTypeDeclarations in Info.plist (see Pitfalls)
// macOS (maccatalyst): UTType identifier string (same as iOS)
// Android: use application/octet-stream or a custom MIME type — no known MIME for .sdchar
// Windows: file extension with dot

private static readonly FilePickerFileType SdCharFileType = new(
    new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.iOS,        new[] { "com.example.sdcharactersheet.sdchar" } },
        { DevicePlatform.macOS,      new[] { "com.example.sdcharactersheet.sdchar" } },
        { DevicePlatform.Android,    new[] { "application/octet-stream" } },
        { DevicePlatform.WinUI,      new[] { ".sdchar" } },
    });

private static readonly PickOptions SdCharPickOptions = new()
{
    PickerTitle = "Open character file",
    FileTypes = SdCharFileType,
};
```

### Pattern 5: MauiProgram.cs Registration
**What:** Register `IFileSaver` and services so they are injectable (and testable via mocking).
```csharp
// MauiProgram.cs
builder.UseMauiCommunityToolkit();
builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
builder.Services.AddSingleton<CharacterViewModel>();
builder.Services.AddSingleton<CharacterFileService>();
builder.Services.AddSingleton<ShadowdarklingsImportService>();
```

### Anti-Patterns to Avoid
- **Serializing CharacterViewModel directly:** The ViewModel contains computed properties, MAUI event handlers, and ObservableProperty backing fields that must not appear in the file format. Always map to/from `CharacterSaveData`.
- **Creating `JsonSerializerOptions` per call:** Each new instance recomputes reflection metadata. Create one static instance and reuse it.
- **Using `stats` instead of `rolledStats` from Shadowdarklings JSON:** `stats` already includes bonuses; importing it would double-count them against our `bonuses[]` array.
- **Storing ledger history:** The app only stores currency totals. Importing the ledger is permitted only to derive totals; the array must then be discarded.
- **Accessing `FullPath` directly on `FileResult`:** Use `OpenReadAsync()` instead — `FullPath` does not always resolve to a physical path on all platforms.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Save-as file dialog (all platforms) | Custom platform-specific dialog code | `CommunityToolkit.Maui.FileSaver` | iOS/Android save dialogs are non-trivial; toolkit handles sandboxing, permissions, and UI |
| Open file dialog (all platforms) | Custom platform intent/picker code | `FilePicker.Default` (MAUI built-in) | Built-in, permission-safe, already cross-platform |
| JSON serialization | Manual string building | `System.Text.Json` | Edge cases in escaping, encoding, null, number formats |
| INotifyPropertyChanged boilerplate | Manual event raising | `CommunityToolkit.Mvvm` `[ObservableProperty]` | Source generators eliminate ~50 lines per property; thread-safe |
| Permission management (Android storage) | Manual permission requests | MAUI/Toolkit handles internally for file operations | API level-dependent permission sets are complex |

**Key insight:** File dialogs on iOS and Android involve sandbox restrictions, entitlements, and async permission flows that are not obvious from the desktop perspective. Never attempt to write directly to arbitrary paths on mobile; always go through the dialog APIs.

---

## Common Pitfalls

### Pitfall 1: iOS/macOS custom file extension not recognized
**What goes wrong:** FilePicker on iOS silently shows no files, or the filter is ignored, because `.sdchar` is not a known system UTType.
**Why it happens:** iOS requires the app to declare a custom UTType in `Info.plist` before the system will associate the extension with any UTI identifier string.
**How to avoid:** Add both `UTImportedTypeDeclarations` and `CFBundleDocumentTypes` to `Platforms/iOS/Info.plist`:
```xml
<key>UTImportedTypeDeclarations</key>
<array>
  <dict>
    <key>UTTypeDescription</key>
    <string>Shadowdark Character File</string>
    <key>UTTypeIdentifier</key>
    <string>com.example.sdcharactersheet.sdchar</string>
    <key>UTTypeConformsTo</key>
    <array><string>public.data</string></array>
    <key>UTTypeTagSpecification</key>
    <dict>
      <key>public.filename-extension</key>
      <array><string>sdchar</string></array>
    </dict>
  </dict>
</array>
<key>CFBundleDocumentTypes</key>
<array>
  <dict>
    <key>CFBundleTypeName</key>
    <string>Shadowdark Character File</string>
    <key>CFBundleTypeRole</key>
    <string>Editor</string>
    <key>LSItemContentTypes</key>
    <array><string>com.example.sdcharactersheet.sdchar</string></array>
    <key>LSHandlerRank</key>
    <string>Owner</string>
  </dict>
</array>
```
The UTI string in `FilePickerFileType` must exactly match `UTTypeIdentifier`.
**Warning signs:** File picker opens but shows 0 results on device (not simulator).

### Pitfall 2: macOS App Sandbox blocks FilePicker / FileSaver
**What goes wrong:** FilePicker or FileSaver fails silently or throws on macOS Catalyst when App Sandbox is enabled (required for Mac App Store distribution).
**Why it happens:** The sandbox container restricts file system access; entitlements must be declared.
**How to avoid:** Add to `Platforms/MacCatalyst/Entitlements.plist`:
```xml
<key>com.apple.security.files.user-selected.read-write</key>
<true/>
```
For Phase 1 (personal use / direct distribution), App Sandbox is NOT required, so this only matters if Mac App Store distribution is pursued later.
**Warning signs:** FilePicker or FileSaver `SaveAsync` returns failure on macOS; no dialog appears.

### Pitfall 3: CommunityToolkit.Mvvm 8.4.0 fails to compile on .NET 10
**What goes wrong:** Build error `MVVMTK0041` + `CS9248`/`CS8050` immediately after installing the package in a .NET 10 project.
**Why it happens:** The source generator requires C# 14 features not enabled by default in .NET 10 stable tooling (as of release).
**How to avoid:** Add to the `.csproj` file:
```xml
<LangVersion>preview</LangVersion>
```
This is the official workaround per the toolkit maintainers (GitHub issue #1139). A patch release is expected but not yet published as of March 2026.
**Warning signs:** First build after adding the NuGet package fails with generator diagnostic errors.

### Pitfall 4: Using `stats` instead of `rolledStats` from Shadowdarklings JSON
**What goes wrong:** STR/DEX etc. appear too high because `stats` already includes bonuses from the `bonuses[]` array. Importing both double-counts the bonuses.
**Why it happens:** Shadowdarklings exports `stats` (final computed values) and `rolledStats` (base rolls) separately.
**How to avoid:** Always deserialize only `rolledStats` into the domain model's base stat values. Confirmed by example: `rolledStats.DEX = 14` while `stats.DEX = 16` (2-point bonus from a Talent).
**Warning signs:** Stats look inflated after import compared to character sheet.

### Pitfall 5: Windows self-contained publish — version-specific RID removed in .NET 10
**What goes wrong:** `dotnet publish` with `-p:RuntimeIdentifierOverride=win10-x64` fails with `NETSDK1083`.
**Why it happens:** .NET 10 dropped version-specific Windows RIDs (`win10-x64`). Use portable RIDs instead.
**How to avoid:** Use `-p:RuntimeIdentifierOverride=win-x64` (no version number).
**Warning signs:** Publish command fails with NETSDK1083 error on Windows in .NET 10.

### Pitfall 6: Android — `application/octet-stream` shows all binary files
**What goes wrong:** The Android FilePicker filter `application/octet-stream` shows all binary files, not just `.sdchar`, giving users a poor experience.
**Why it happens:** Android has no mechanism to declare a custom MIME type for an unknown extension without registering it system-wide (which requires an installed app with an intent filter).
**How to avoid:** This is an accepted limitation for Phase 1. For open-file on Android, using `application/octet-stream` and relying on the user to navigate to the correct file is the pragmatic approach. Post-v1 can explore file provider URIs or document-picker intents for refinement.
**Warning signs:** Not a code error — a known UX limitation on Android for unknown MIME types.

### Pitfall 7: New `JsonSerializerOptions` per call
**What goes wrong:** Slow serialization and GC pressure as metadata cache is rebuilt on every call.
**Why it happens:** The options object lazily builds reflection metadata on first use; creating a new one per call discards this work.
**How to avoid:** Declare one `static readonly JsonSerializerOptions` for import and one for save/load. Reuse across all calls.

---

## Code Examples

### Complete import flow (condensed)
```csharp
// Source: official docs + Brim.json example analysis
public async Task<Character?> ImportAsync(Stream jsonStream)
{
    ShadowdarklingsJson? sdJson;
    try
    {
        sdJson = await JsonSerializer.DeserializeAsync<ShadowdarklingsJson>(
            jsonStream, ImportOptions);
    }
    catch (JsonException ex)
    {
        // Return null or throw a typed ImportException — discretion area
        return null;
    }

    if (sdJson is null) return null;

    // Currency: prefer top-level fields
    int gp = sdJson.Gold ?? sdJson.Ledger?.Sum(e => e.GoldChange) ?? 0;
    int sp = sdJson.Silver ?? sdJson.Ledger?.Sum(e => e.SilverChange) ?? 0;
    int cp = sdJson.Copper ?? sdJson.Ledger?.Sum(e => e.CopperChange) ?? 0;

    var character = new Character
    {
        Name = sdJson.Name,
        Class = sdJson.Class,
        Ancestry = sdJson.Ancestry,
        Level = sdJson.Level,
        Title = sdJson.Title,
        Alignment = sdJson.Alignment,
        Background = sdJson.Background,
        Deity = sdJson.Deity,
        Languages = sdJson.Languages,
        XP = sdJson.XP,
        MaxHP = sdJson.MaxHitPoints,
        CurrentHP = sdJson.MaxHitPoints,  // assume full health on import
        SpellsKnown = sdJson.SpellsKnown,
        GP = gp, SP = sp, CP = cp,
        BaseSTR = sdJson.RolledStats?.STR ?? 0,
        BaseDEX = sdJson.RolledStats?.DEX ?? 0,
        // ... etc for all 6 stats ...
        Bonuses = MapBonuses(sdJson.Bonuses),
        Gear = MapGear(sdJson.Gear),
        MagicItems = MapMagicItems(sdJson.MagicItems),
        Attacks = sdJson.Attacks ?? [],
    };
    return character;
}

// bonusTo parsing: "DEX:+2" → stat = DEX, value = +2
// bonusTo starting with "AC:" → goes to AC contributor list, not stat bonuses
private static List<BonusSource> MapBonuses(List<SdBonus>? bonuses)
{
    if (bonuses is null) return [];
    return bonuses
        .Where(b => !string.IsNullOrEmpty(b.BonusTo))  // skip non-stat bonuses
        .Select(b => new BonusSource
        {
            Label = b.SourceName,
            BonusTo = b.BonusTo,
            SourceType = b.SourceType,
            GainedAtLevel = b.GainedAtLevel,
        })
        .ToList();
}
```

### Save to .sdchar
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/essentials/file-saver
public async Task SaveCharacterAsync(CharacterViewModel vm, CancellationToken ct)
{
    var dto = MapToDto(vm);  // always map ViewModel → DTO
    var json = JsonSerializer.Serialize(dto, SaveOptions);
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
    var result = await _fileSaver.SaveAsync($"{vm.Name}.sdchar", stream, ct);
    if (!result.IsSuccessful)
        throw new IOException("Save failed", result.Exception);
}
```

### Load from .sdchar
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker
public async Task<CharacterSaveData?> LoadCharacterAsync(CancellationToken ct)
{
    var fileResult = await FilePicker.Default.PickAsync(SdCharPickOptions);
    if (fileResult is null) return null;    // user cancelled
    using var stream = await fileResult.OpenReadAsync();  // use OpenReadAsync, not FullPath
    return await JsonSerializer.DeserializeAsync<CharacterSaveData>(stream, LoadOptions);
}
```

### csproj configuration (four platforms, unpackaged Windows)
```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/publish-unpackaged-cli -->
<PropertyGroup>
  <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
  <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">
    $(TargetFrameworks);net10.0-windows10.0.19041.0
  </TargetFrameworks>
  <WindowsPackageType>None</WindowsPackageType>
  <LangVersion>preview</LangVersion>  <!-- Required: CommunityToolkit.Mvvm 8.4.0 + .NET 10 -->
</PropertyGroup>

<!-- Workaround for WindowsAppSDK bug #3337 -->
<PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows' and '$(RuntimeIdentifierOverride)' != ''">
  <RuntimeIdentifier>$(RuntimeIdentifierOverride)</RuntimeIdentifier>
</PropertyGroup>
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Xamarin.Essentials.FilePicker` | `Microsoft.Maui.Storage.FilePicker` | MAUI launch (~2022) | Same API surface, now inbox to MAUI |
| Manual `INotifyPropertyChanged` | `CommunityToolkit.Mvvm` `[ObservableProperty]` source generators | v8.x (2023) | 80% boilerplate reduction |
| MSIX packaged as default | Unpackaged (`WindowsPackageType=None`) as viable default | .NET 9 default (2024) | Direct file I/O, no sandbox on Windows dev |
| `win10-x64` RID | `win-x64` portable RID | .NET 10 (2025) | Old RID string causes NETSDK1083 error |
| `IgnoreNullValues = true` (deprecated) | `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault` | .NET 6 | Old property is obsolete, produces compiler warning |

**Deprecated/outdated:**
- `JsonSerializerOptions.IgnoreNullValues`: replaced by `DefaultIgnoreCondition`. Using the old property on .NET 10 produces an `[Obsolete]` warning.
- `win10-x64` / `win10-x86` RIDs: removed in .NET 10. Use `win-x64` / `win-x86`.

---

## Open Questions

1. **Bundle ID / reverse-domain for UTType identifier**
   - What we know: The UTType string `com.example.sdcharactersheet.sdchar` is used in the examples above; it must be unique and match what's in Info.plist.
   - What's unclear: The actual bundle ID for this app hasn't been established yet — it will be set during project scaffold.
   - Recommendation: Planner should include a task to choose the bundle ID at project creation and update all UTType strings consistently.

2. **Android file access on API ≥ 34 (no permissions needed)**
   - What we know: Android 14+ (`targetSdk=34`) does not require `READ_EXTERNAL_STORAGE` for file picker operations. The toolkit doc says no permissions needed for API 34+.
   - What's unclear: Whether the default MAUI project template sets `targetSdk=34` or targets a lower API.
   - Recommendation: Verify `targetSdk` in `Platforms/Android/AndroidManifest.xml` during project creation. If targeting lower, add storage permissions.

3. **CharacterViewModel singleton lifecycle on mobile**
   - What we know: The VM is registered as a singleton so the Gear tab sees STR changes from the Stats tab.
   - What's unclear: Whether MAUI's service container singleton lifetime persists across app suspension/resume on iOS/Android or requires state restoration.
   - Recommendation: For Phase 1 (no UI), this is a non-issue. Flag for Phase 2 when tabs and lifecycle events are wired.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.x |
| Config file | `SdCharacterSheet.Tests/SdCharacterSheet.Tests.csproj` (net10.0, separate project) |
| Quick run command | `dotnet test SdCharacterSheet.Tests/ --filter "Category=Unit" --no-build` |
| Full suite command | `dotnet test SdCharacterSheet.Tests/` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| FILE-01 | Import Brim.json produces correct Character (name, class, rolledStats, bonuses, gear, magic items, currency, HP) | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests"` | ❌ Wave 0 |
| FILE-01 | Import uses rolledStats not stats (no double-counting) | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_UsesRolledStats"` | ❌ Wave 0 |
| FILE-01 | Import: currency reads top-level gold/silver/copper fields | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_Currency_TopLevelFields"` | ❌ Wave 0 |
| FILE-01 | Import: currency falls back to ledger sum when top-level fields absent | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_Currency_LedgerFallback"` | ❌ Wave 0 |
| FILE-01 | Import: bonusTo "AC:..." entries map to AC contributors, not stat bonuses | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_AcBonuses"` | ❌ Wave 0 |
| FILE-01 | Import: unknown JSON fields are silently ignored (no exception) | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_UnknownFields_Ignored"` | ❌ Wave 0 |
| FILE-01 | Import: currentHP = maxHP on import | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_CurrentHP_EqualsMaxHP"` | ❌ Wave 0 |
| FILE-02 | Round-trip: save DTO to JSON and reload produces identical DTO | unit | `dotnet test --filter "FullyQualifiedName~CharacterFileServiceTests.RoundTrip_SaveLoad_NoDataLoss"` | ❌ Wave 0 |
| FILE-03 | Saved .sdchar JSON contains `Version` field | unit | `dotnet test --filter "FullyQualifiedName~CharacterFileServiceTests.Save_ContainsVersionField"` | ❌ Wave 0 |
| FILE-02/03 | File dialog operations (FilePicker, FileSaver) on real devices | manual | N/A — requires device/emulator | manual-only |

**Manual-only justification:** FilePicker and FileSaver both require native UI dialogs that cannot be invoked in a headless xUnit test runner. Test the JSON round-trip in unit tests; verify the dialog on each platform target manually.

### Sampling Rate
- **Per task commit:** `dotnet test SdCharacterSheet.Tests/ --filter "Category=Unit" -x`
- **Per wave merge:** `dotnet test SdCharacterSheet.Tests/`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `SdCharacterSheet.Tests/SdCharacterSheet.Tests.csproj` — xUnit test project does not exist yet; create targeting `net10.0`
- [ ] `SdCharacterSheet.Tests/Services/ShadowdarklingsImportServiceTests.cs` — covers FILE-01
- [ ] `SdCharacterSheet.Tests/Services/CharacterFileServiceTests.cs` — covers FILE-02, FILE-03
- [ ] `SdCharacterSheet.Tests/TestData/Brim.json` — copy of `examples/Brim.json` for test fixture (or reference via relative path)
- [ ] Framework install: `dotnet add SdCharacterSheet.Tests package xunit && dotnet add SdCharacterSheet.Tests package Microsoft.NET.Test.Sdk && dotnet add SdCharacterSheet.Tests package xunit.runner.visualstudio`

---

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn — File picker (.NET MAUI 10)](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker?view=net-maui-10.0) — `FilePickerFileType` syntax, platform differences, permissions
- [Microsoft Learn — FileSaver (CommunityToolkit.Maui)](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/essentials/file-saver) — `SaveAsync` API, entitlement requirements, DI registration
- [Microsoft Learn — Publish unpackaged Windows app (.NET MAUI 10)](https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/publish-unpackaged-cli?view=net-maui-10.0) — `WindowsPackageType=None`, portable RID for .NET 10
- [Microsoft Learn — System.Text.Json configure options](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/configure-options) — options reuse pattern, `DefaultIgnoreCondition`
- [.NET MAUI support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/maui) — LTS timeline confirmation
- `examples/Brim.json` (in repository) — authoritative Shadowdarklings JSON field names, data types, and structure

### Secondary (MEDIUM confidence)
- [GitHub discussion #19570 — FilePicker custom UTType on iOS](https://github.com/dotnet/maui/discussions/19570) — `Info.plist` configuration pattern for custom extensions; confirmed by multiple community reports
- [InfoQ — .NET 10 RC2 release](https://www.infoq.com/news/2025/10/dotnet-10-rc-2-release/) — .NET 10 / MAUI 10 GA Nov 2025 confirmed
- [NuGet — CommunityToolkit.Maui 14.0.1](https://www.nuget.org/packages/CommunityToolkit.Maui) — confirmed .NET 10 target support

### Tertiary (LOW confidence — flag for validation)
- [GitHub issue #1139 — CommunityToolkit.Mvvm 8.4.0 .NET 10 failure](https://github.com/CommunityToolkit/dotnet/issues/1139) — `LangVersion=preview` workaround; fix committed but unpublished as of late 2025. **Validate at project creation:** if a newer patch release is available, prefer it over the `preview` workaround.
- Android `application/octet-stream` as custom file MIME — community pattern; no official MAUI doc specifically addressing unknown extension MIME mapping on Android.

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all packages verified against official NuGet and Microsoft docs
- Architecture patterns: HIGH — DTO separation and System.Text.Json patterns directly from official docs; import field mapping confirmed against real Shadowdarklings export
- FilePicker/FileSaver: HIGH — official Microsoft Learn docs, verified API signatures
- CommunityToolkit.Mvvm .NET 10 issue: MEDIUM — GitHub issue confirmed, workaround documented, but patch release status uncertain
- iOS UTType configuration: MEDIUM — GitHub community discussion, consistent with Apple UTI documentation patterns

**Research date:** 2026-03-08
**Valid until:** 2026-09-08 (stable ecosystem; re-verify CommunityToolkit.Mvvm .NET 10 issue status before project creation)
