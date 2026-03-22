# Phase 4: File Menu - Research

**Researched:** 2026-03-21
**Domain:** .NET MAUI file I/O — FilePicker, IFileSaver, CommunityToolkit.Maui, RelayCommand
**Confidence:** HIGH

## Summary

Phase 4 is almost entirely a UI-wiring task. The serialization layer (`CharacterFileService`, `CharacterSaveData`), the MAUI file-open layer (`MauiCharacterFileService.OpenAsync`), the save adapter (`CommunityToolkitFileSaverAdapter`), and the import service (`ShadowdarklingsImportService`) are all fully implemented and registered in DI. The only gap is that `CharacterViewModel` has no `SaveCommand`, `LoadCommand`, or `ImportCommand`, and `AppShell.xaml` only has `ExportCommand` in its File menu.

The pattern to follow is identical to how `ExportCommand` was wired: add `[RelayCommand]` async methods to `CharacterViewModel`, inject the needed services via a new constructor overload, register the VM correctly in DI, then add three `<MenuFlyoutItem>` entries (and optionally three `<ToolbarItem>` entries) in `AppShell.xaml`. No new packages are needed. No service logic changes are needed.

**Primary recommendation:** Add `SaveCommand`, `LoadCommand`, and `ImportCommand` to `CharacterViewModel` by injecting `MauiCharacterFileService` and `ShadowdarklingsImportService`. Wire these three commands into `AppShell.xaml`'s File menu. Write one unit test per command verifying correct ViewModel state after each operation.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| FILE-01 | User can save the current character to a .sdchar file via the File menu | `CharacterFileService.SaveAsync` is complete; needs `SaveCommand` on VM + AppShell XAML entry |
| FILE-02 | User can load a character from a .sdchar file via the File menu | `MauiCharacterFileService.OpenAsync` + `CharacterViewModel.LoadCharacter` are complete; needs `LoadCommand` on VM + AppShell XAML entry |
| FILE-03 | User can import a Shadowdarklings.net JSON export via the File menu | `ShadowdarklingsImportService.ImportAsync` + `CharacterViewModel.LoadCharacter` are complete; needs `ImportCommand` on VM + MAUI file picker call + AppShell XAML entry |
</phase_requirements>

## Standard Stack

### Core (already in project — no new installs)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.Maui.Controls | 10.0.41 | `FilePicker.Default.PickAsync`, `MenuFlyoutItem`, `ToolbarItem`, `Shell.MenuBarItems` | MAUI's built-in file picker and menu APIs |
| CommunityToolkit.Maui | 14.0.1 | `IFileSaver` / `FileSaver.Default` — native save-as dialog | Already wired via `CommunityToolkitFileSaverAdapter` |
| CommunityToolkit.Mvvm | 8.4.0 | `[RelayCommand]` source generator — async command generation | Already used by `ExportCommand` |

### No new packages are needed.

All required services are already registered as singletons in `MauiProgram.cs`:
- `IFileSaver` → `CommunityToolkitFileSaverAdapter`
- `CharacterFileService` → `MauiCharacterFileService` (which exposes both `SaveAsync` and `OpenAsync`)
- `ShadowdarklingsImportService`

## Architecture Patterns

### Recommended Project Structure (no new directories needed)

The three new commands follow the same file pattern as `ExportCommand`:
```
SdCharacterSheet/
├── ViewModels/
│   └── CharacterViewModel.cs      ← add SaveCommand, LoadCommand, ImportCommand
└── AppShell.xaml                  ← add three MenuFlyoutItem + ToolbarItem entries
```

### Pattern 1: RelayCommand async method (follow ExportCommand)

The existing `ExportCommand` shows the exact pattern to replicate:

```csharp
// Source: CharacterViewModel.cs (existing ExportCommand)
private readonly MarkdownExportService? _exportService;

[RelayCommand]
private async Task ExportAsync()
{
    if (_exportService is null) return;
    await _exportService.ExportAsync(this);
}

public CharacterViewModel(MarkdownExportService exportService) : this()
{
    _exportService = exportService;
}
```

Apply the same pattern for the three new services:

```csharp
// New fields
private readonly MauiCharacterFileService? _fileService;
private readonly ShadowdarklingsImportService? _importService;

// New constructor overload (chains to existing constructor)
public CharacterViewModel(
    MarkdownExportService exportService,
    MauiCharacterFileService fileService,
    ShadowdarklingsImportService importService) : this()
{
    _exportService = exportService;
    _fileService = fileService;
    _importService = importService;
}

[RelayCommand]
private async Task SaveAsync()
{
    if (_fileService is null) return;
    var character = BuildCharacterFromViewModel();
    await _fileService.SaveAsync(character);
}

[RelayCommand]
private async Task LoadAsync()
{
    if (_fileService is null) return;
    var character = await _fileService.OpenAsync();
    if (character is null) return;
    LoadCharacter(character);
}

[RelayCommand]
private async Task ImportAsync()
{
    if (_importService is null) return;
    var result = await FilePicker.Default.PickAsync(JsonPickOptions);
    if (result is null) return;
    using var stream = await result.OpenReadAsync();
    var character = await _importService.ImportAsync(stream);
    if (character is null) return;
    LoadCharacter(character);
}
```

**Key insight:** `CharacterViewModel` already has `LoadCharacter(Character character)` which handles full ViewModel state replacement. Both Load and Import converge on this single method.

### Pattern 2: BuildCharacterFromViewModel helper

`CharacterFileService.SaveAsync` takes a `Character` domain model, not a `CharacterViewModel`. The VM currently has `public Character Character { get; private set; }` but it is only updated by `LoadCharacter`, not by incremental edits.

**Correct approach:** Build a fresh `Character` from current VM state (mirroring what `MapToDto` already needs), using a `BuildCharacterFromViewModel()` helper:

```csharp
private Character BuildCharacterFromViewModel() => new()
{
    Name = Name,
    Class = Class,
    Ancestry = Ancestry,
    Level = Level,
    Title = Title,
    Alignment = Alignment,
    Background = Background,
    Deity = Deity,
    Languages = Languages,
    XP = XP,
    MaxXP = MaxXP,
    BaseSTR = BaseSTR,
    BaseDEX = BaseDEX,
    BaseCON = BaseCON,
    BaseINT = BaseINT,
    BaseWIS = BaseWIS,
    BaseCHA = BaseCHA,
    MaxHP = MaxHP,
    CurrentHP = CurrentHP,
    GP = GP,
    SP = SP,
    CP = CP,
    Bonuses = Character.Bonuses,  // bonuses only live on Character, not VM fields
    Gear = GearItems
        .Where(g => g.ItemType != "")
        .Select(g => new GearItem { Name = g.Name, Slots = g.Slots, ItemType = g.ItemType, Note = g.Note })
        .ToList(),
    MagicItems = GearItems
        .Where(g => g.ItemType == "")
        .Select(g => new MagicItem { Name = g.Name, Slots = g.Slots, Note = g.Note })
        .ToList(),
    Attacks = Attacks.ToList(),
    SpellsKnown = SpellsKnown,
    Notes = Notes,
};
```

`GearItemViewModel` has a `GearItemSource Source` enum property (`Gear` or `Magic`) that is the canonical discriminator. `ItemType` is also `""` for magic items, but `Source` is more semantically correct and immune to edge cases where a user-created gear item might have an empty `ItemType`.

### Pattern 3: AppShell XAML menu additions

Follow the exact `ExportCommand` pattern already in `AppShell.xaml`:

```xml
<Shell.MenuBarItems>
    <MenuBarItem Text="File">
        <MenuFlyoutItem Text="Save..."
                        Command="{Binding SaveCommand}" />
        <MenuFlyoutItem Text="Open..."
                        Command="{Binding LoadCommand}" />
        <MenuFlyoutItem Text="Import from Shadowdarklings..."
                        Command="{Binding ImportCommand}" />
        <MenuFlyoutSeparator />
        <MenuFlyoutItem Text="Export As..."
                        Command="{Binding ExportCommand}" />
    </MenuBarItem>
</Shell.MenuBarItems>
```

`Shell.MenuBarItems` renders as a native menu bar on macOS and Windows. On iOS and Android it is silently ignored — those platforms need `ToolbarItem` entries. Add toolbar items with `Order="Secondary"` (overflow menu) to avoid cluttering the toolbar:

```xml
<Shell.ToolbarItems>
    <ToolbarItem Text="Save" Order="Secondary" Command="{Binding SaveCommand}" />
    <ToolbarItem Text="Open" Order="Secondary" Command="{Binding LoadCommand}" />
    <ToolbarItem Text="Import" Order="Secondary" Command="{Binding ImportCommand}" />
    <ToolbarItem Text="Export" Order="Primary" Command="{Binding ExportCommand}" />
</Shell.ToolbarItems>
```

### Pattern 4: JSON file picker for Import

`ShadowdarklingsImportService.ImportAsync` takes a `Stream`. The caller must open a file picker and pass the stream. Pattern follows `MauiCharacterFileService.OpenAsync`:

```csharp
private static readonly FilePickerFileType JsonFileType = new(
    new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.iOS,     new[] { "public.json" } },
        { DevicePlatform.macOS,   new[] { "public.json" } },
        { DevicePlatform.Android, new[] { "application/json" } },
        { DevicePlatform.WinUI,   new[] { ".json" } },
    });

private static readonly PickOptions JsonPickOptions = new()
{
    PickerTitle = "Import Shadowdarklings character",
    FileTypes = JsonFileType,
};
```

### Pattern 5: DI registration update

`MauiProgram.cs` must pass both new services to `CharacterViewModel`:

```csharp
builder.Services.AddSingleton<CharacterViewModel>(sp =>
    new CharacterViewModel(
        sp.GetRequiredService<MarkdownExportService>(),
        sp.GetRequiredService<MauiCharacterFileService>(),
        sp.GetRequiredService<ShadowdarklingsImportService>()));
```

Note: `CharacterFileService` is registered as `MauiCharacterFileService` in DI — resolve it as `MauiCharacterFileService`, not the base type.

### Anti-Patterns to Avoid

- **Reading `Character` backing field directly for save:** The `Character` property is set only during `LoadCharacter`. If the user edits the sheet without loading a file, `Character` will be a stale empty object. Always use `BuildCharacterFromViewModel()` for save.
- **Calling `CharacterFileService` base type for `OpenAsync`:** `OpenAsync` only exists on `MauiCharacterFileService`. The DI registration is `CharacterFileService` → `MauiCharacterFileService`, so resolve as `MauiCharacterFileService`.
- **Adding `ToolbarItem` `Order="Primary"` for all three new items:** This would flood the toolbar on mobile. Use `Order="Secondary"` (overflow/kebab menu) for the new items.
- **Using `Shell.MenuBarItems` alone for mobile:** `MenuBarItems` is desktop-only. Add `ToolbarItem` entries too.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Native save-as dialog | Custom stream-to-file write | `CommunityToolkitFileSaverAdapter` + `FileSaver.Default` | Already wired; handles all 4 platforms including iOS sandbox |
| .sdchar serialization | Custom JSON writer | `CharacterFileService.SaveAsync` / `LoadFromStreamAsync` | Complete, tested, version-stamped |
| .sdchar file picker | Manual platform file picker | `MauiCharacterFileService.OpenAsync` | Complete; handles file type filtering per platform |
| JSON deserialization for import | Custom parser | `ShadowdarklingsImportService.ImportAsync` | Complete; handles currency edge cases, ledger fallback, unknown fields |
| Full character state replacement | Property-by-property manual assignment | `CharacterViewModel.LoadCharacter(Character)` | Complete; notifies all observers with `OnPropertyChanged(string.Empty)` |

**Key insight:** All service logic is complete and tested. This phase is 100% wiring.

## Common Pitfalls

### Pitfall 1: Stale Character backing field on save

**What goes wrong:** Calling `_fileService.SaveAsync(Character)` where `Character` is the backing field. The backing field is only updated by `LoadCharacter` — it does not reflect edits made via VM properties.

**Why it happens:** `Character` is set in `LoadCharacter` and never updated again. All user edits go to the VM's `[ObservableProperty]` fields.

**How to avoid:** Implement `BuildCharacterFromViewModel()` and pass its result to `SaveAsync`.

**Warning signs:** Save produces a file with all-default values even after editing.

### Pitfall 2: Resolving wrong service type from DI

**What goes wrong:** Injecting `CharacterFileService` (base type) when you need `MauiCharacterFileService` — `OpenAsync` will not be available.

**Why it happens:** DI registration is `CharacterFileService` → `MauiCharacterFileService` (keyed by base type). Requesting `CharacterFileService` returns the MAUI subclass cast to the base type, hiding `OpenAsync`.

**How to avoid:** Inject as `MauiCharacterFileService` in the constructor parameter type.

### Pitfall 3: Null check omission on file picker cancellation

**What goes wrong:** `FilePicker.Default.PickAsync` and `MauiCharacterFileService.OpenAsync` both return `null` when the user cancels the dialog. Proceeding without null-checking will throw a `NullReferenceException`.

**Why it happens:** Cancellation is a normal user action, not an exception.

**How to avoid:** Always check `if (result is null) return;` before using the result.

### Pitfall 4: ImportAsync stream disposed before read completes

**What goes wrong:** Wrapping `fileResult.OpenReadAsync()` in a `using` and passing the stream to `ImportAsync` — if the stream is disposed before the async JSON deserialization finishes.

**Why it happens:** `JsonSerializer.DeserializeAsync` is async; disposal before completion causes an `ObjectDisposedException`.

**How to avoid:** Use `await using var stream = ...` so disposal is deferred until after the awaited import call completes.

### Pitfall 5: MenuFlyoutItem not showing on mobile

**What goes wrong:** Assuming `Shell.MenuBarItems` works on iOS/Android.

**Why it happens:** MAUI's `MenuBarItems` renders as a native menu bar only on macOS Catalyst and Windows. It is a no-op on iOS and Android.

**How to avoid:** Add `ToolbarItem` entries (with `Order="Secondary"`) alongside the `MenuBarItems` entries.

## Code Examples

Verified patterns from the existing codebase:

### ExportCommand (exact pattern to replicate)
```csharp
// Source: SdCharacterSheet/ViewModels/CharacterViewModel.cs
private readonly MarkdownExportService? _exportService;

[RelayCommand]
private async Task ExportAsync()
{
    if (_exportService is null) return;
    await _exportService.ExportAsync(this);
}

public CharacterViewModel(MarkdownExportService exportService) : this()
{
    _exportService = exportService;
}
```

### AppShell MenuBarItem (existing pattern)
```xml
<!-- Source: SdCharacterSheet/AppShell.xaml -->
<Shell.MenuBarItems>
    <MenuBarItem Text="File">
        <MenuFlyoutItem Text="Export As..."
                        Command="{Binding ExportCommand}" />
    </MenuBarItem>
</Shell.MenuBarItems>
```

### MauiCharacterFileService.OpenAsync (already complete)
```csharp
// Source: SdCharacterSheet/Services/MauiCharacterFileService.cs
public async Task<Character?> OpenAsync(CancellationToken ct = default)
{
    var fileResult = await FilePicker.Default.PickAsync(SdCharPickOptions);
    if (fileResult is null) return null;
    using var stream = await fileResult.OpenReadAsync();
    var dto = await LoadFromStreamAsync(stream);
    return dto is null ? null : MapFromDto(dto);
}
```

### CharacterViewModel.LoadCharacter (already complete)
```csharp
// Source: SdCharacterSheet/ViewModels/CharacterViewModel.cs
// Notify everything — full character replacement
OnPropertyChanged(string.Empty);
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual INotifyPropertyChanged | `[ObservableProperty]` source generator | Phase 1 | No property boilerplate needed |
| Custom file type negotiation | `FilePickerFileType` per `DevicePlatform` | Phase 1/3 | Type already declared in `MauiCharacterFileService` |
| Direct stream writes | `CommunityToolkitFileSaverAdapter` | Phase 3 | Handles iOS sandbox, Windows save dialog |

## Open Questions

1. **GearItemViewModel discriminator — RESOLVED**
   - Answer: `GearItemViewModel` has a `GearItemSource Source` enum (`Gear` | `Magic`). Use `g.Source == GearItemSource.Gear` / `GearItemSource.Magic` to split gear from magic items in `BuildCharacterFromViewModel`. `ItemType` is `""` for magic items but `Source` is the cleaner discriminator.

2. **Save should use current VM state vs Character backing field**
   - What we know: `Character` backing field is not kept in sync with VM edits
   - Recommendation: Confirmed — use `BuildCharacterFromViewModel()` helper. Do NOT use `Character` directly.

3. **Toolbar UX on mobile**
   - What we know: `Order="Secondary"` puts items in an overflow menu on Android/iOS
   - What's unclear: Whether a "..." overflow button is always visible, or only when secondary items exist
   - Recommendation: Use `Order="Secondary"` for Save/Open/Import. Export stays `Order="Primary"` as established.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 |
| Config file | none (convention-based) |
| Quick run command | `dotnet test SdCharacterSheet.Tests/ --filter "Category=Unit" -x` |
| Full suite command | `dotnet test SdCharacterSheet.Tests/ -x` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| FILE-01 | `SaveCommand` calls `_fileService.SaveAsync` with current VM state | unit | `dotnet test SdCharacterSheet.Tests/ --filter "FullyQualifiedName~SaveCommand" -x` | Wave 0 |
| FILE-02 | `LoadCommand` calls `_fileService.OpenAsync` and passes result to `LoadCharacter` | unit | `dotnet test SdCharacterSheet.Tests/ --filter "FullyQualifiedName~LoadCommand" -x` | Wave 0 |
| FILE-03 | `ImportCommand` opens a JSON file, calls `_importService.ImportAsync`, calls `LoadCharacter` | unit | `dotnet test SdCharacterSheet.Tests/ --filter "FullyQualifiedName~ImportCommand" -x` | Wave 0 |
| FILE-01-02-03 | AppShell menu items visible — all platforms | manual | N/A | manual-only |

**Note on E2E:** File dialog tests (actual native dialog opening, file appearing on disk) require device/simulator and are manual-only. Unit tests use null/stub services.

### Sampling Rate
- **Per task commit:** `dotnet test SdCharacterSheet.Tests/ --filter "Category=Unit" -x`
- **Per wave merge:** `dotnet test SdCharacterSheet.Tests/ -x`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `SdCharacterSheet.Tests/ViewModels/CharacterViewModelFileCommandTests.cs` — covers FILE-01, FILE-02, FILE-03 unit tests
- [ ] Stub implementations of `MauiCharacterFileService` and `ShadowdarklingsImportService` for unit testing (or test-local fakes)

**Note:** `MauiCharacterFileService` calls `FilePicker.Default.PickAsync` which requires the MAUI runtime. Unit tests must use a test-local fake or extract an `ICharacterFileService` interface. The existing `NullFileSaver` pattern in `CharacterFileServiceTests` shows the project's preferred approach — test-local stubs.

## Sources

### Primary (HIGH confidence)
- Codebase direct read — `SdCharacterSheet/Services/MauiCharacterFileService.cs`, `CharacterFileService.cs`, `ShadowdarklingsImportService.cs`, `CharacterViewModel.cs`, `AppShell.xaml`, `MauiProgram.cs` — all verified directly
- `SdCharacterSheet.Tests/` — xUnit test structure, existing test patterns, NullFileSaver stub pattern

### Secondary (MEDIUM confidence)
- MAUI `Shell.MenuBarItems` desktop-only behavior — confirmed by codebase evidence (existing Export works on macOS/Windows via this path) and MAUI documentation patterns
- `FilePicker.Default.PickAsync` null-on-cancel behavior — confirmed by `MauiCharacterFileService.OpenAsync` which already handles this (`if (fileResult is null) return null`)

### Tertiary (LOW confidence)
- `ToolbarItem` `Order="Secondary"` overflow menu visibility on Android/iOS — standard MAUI behavior; not independently verified against device

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all packages already in csproj, versions confirmed
- Architecture: HIGH — direct codebase inspection; all services implemented; pattern mirrors existing ExportCommand exactly
- Pitfalls: HIGH — derived directly from code reading (stale Character field, null cancellation, service resolution type)

**Research date:** 2026-03-21
**Valid until:** 2026-04-21 (stable stack — MAUI 10, CommunityToolkit.Maui 14)
