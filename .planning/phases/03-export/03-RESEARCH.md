# Phase 3: Export - Research

**Researched:** 2026-03-21
**Domain:** .NET MAUI file sharing, Markdown string generation, Shell ToolbarItem
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Export trigger**
- D-01: Export is reachable from anywhere in the app — a toolbar/nav bar action visible on all three tabs
- D-02: On desktop (Windows/macOS), export uses the native menu/toolbar; on mobile (iOS/Android), the same action triggers the native share sheet
- D-03: Export button is always tappable, even with no character loaded (exports a blank/default sheet)
- D-04: No preview step — tap → share/save immediately

**Filename**
- D-05: Default filename is `{Name}-{Class}{Level}.md` — e.g. `Brim-Thief4.md`

**Markdown structure**
- D-06: Section order: Identity → Stats → Attacks → Currency → Gear → Notes
- D-07: No horizontal rules between sections
- D-08: Stat names in bold; keep Markdown plain and readable
- D-09: Character name as a top-level `#` heading; sections use `##`

**Stats section**
- D-10: Each stat shows total + modifier, followed by an indented bullet list of bonus sources
- D-11: AC gets its own subsection using the same bonus-source breakdown pattern (bonus sources with `AC:` prefix)

**Attacks section**
- D-12: `## Attacks` is its own section — free-form text entries, one per line (bullet list)

**Gear section**
- D-13: Gear is a Markdown table with exactly `GearSlotTotal` rows (= max(STR, 10))
- D-14: Table header includes the slot count: `**Gear** (used / total slots)`
- D-15: Two columns: `Slot` (number) and `Item` (name)
- D-16: Multi-slot items: first row = item name, subsequent rows = `(cont. {item name})`
- D-17: Coin slots appear as a row with `Coins` when they occupy ≥1 slot
- D-18: Unused slots are empty numbered rows

**Currency section**
- D-19: `## Currency` section — one line each for GP, SP, CP

**HP / XP**
- D-20: HP and XP appear in the Identity section, one line each — e.g. `HP: 8 / 14` and `XP: 3 / 10`

**Spells section**
- D-21: `## Spells` section included if `SpellsKnown` is non-empty — plain string, no parsing
- D-22: `SpellsKnown` needs to be exposed on `CharacterViewModel` (it exists on the `Character` model but is not yet an observable property)

**Platform share/save behavior**
- D-23: Mobile (iOS/Android): export triggers native share sheet with the `.md` file
- D-24: Desktop (Windows/macOS): export triggers save-as dialog; uses existing `IFileSaver` / `CommunityToolkitFileSaverAdapter` infrastructure

### Claude's Discretion
- Exact MIME type for the `.md` file on each platform
- Whether the export service lives in `SdCharacterSheet.Core` or the MAUI project
- Error handling if save/share fails
- Exact Identity section layout (field labels, line breaks vs. table)
- How to handle empty string fields in Identity (omit or include blank)

### Deferred Ideas (OUT OF SCOPE)
- Markdown styling/formatting improvements — explicitly deferred to future milestones
- PDF export — out of scope for v1
- OS-level file type association for `.md` files — not required
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MRKD-01 | User can export the full character sheet as formatted Markdown for print/reference | Markdown builder pattern, platform share/save APIs, ToolbarItem placement at Shell level |
</phase_requirements>

---

## Summary

Phase 3 is a pure write-only feature: read from `CharacterViewModel`, format as Markdown, push to the OS. There is no new UI state to persist and no new domain model to introduce. The entire implementation is ~3 distinct pieces: (1) a Markdown builder service, (2) a platform-routing export service that calls either `Share.RequestAsync` (mobile) or `IFileSaver.SaveAsync` (desktop), and (3) a Shell-level `ToolbarItem` wired to an `[RelayCommand]` on `CharacterViewModel`.

The existing `IFileSaver` / `CommunityToolkitFileSaverAdapter` infrastructure already handles desktop save-as correctly — it just needs to be called with a `.md` filename. Mobile sharing uses the built-in MAUI `Share.Default.RequestAsync` API: write the Markdown content to `FileSystem.CacheDirectory`, then share the file via `ShareFileRequest`. No new NuGet packages are required.

The one ViewModel gap is `SpellsKnown`: it exists on `Character` but has no `[ObservableProperty]` on `CharacterViewModel`. Adding that property is a small Task 0 prerequisite. All other data the export needs (`TotalSTR`, `ModSTR`, `GearItems`, `Attacks`, `CoinSlots`, `GearSlotTotal`, `Character.Bonuses`) is already exposed.

**Primary recommendation:** Implement a `MarkdownExportService` in the MAUI project (same layer as `MauiCharacterFileService`), with platform routing inside it. Keep the Markdown builder as a pure method (no I/O) so it is fully unit-testable without a device.

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `Microsoft.Maui.ApplicationModel.DataTransfer` (Share) | Ships with .NET MAUI 10 | Native share sheet on iOS/Android | Built-in — no additional package |
| `CommunityToolkit.Maui` (IFileSaver) | 14.0.1 (already installed) | Save-as dialog on Windows/macOS | Already wired in `MauiProgram.cs` |
| `CommunityToolkit.Mvvm` (`[RelayCommand]`) | 8.4.0 (already installed) | Expose export command on ViewModel | Consistent with existing patterns |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `System.Text.StringBuilder` | BCL | Build Markdown string efficiently | Always — avoids string concatenation in loops |
| `System.IO.MemoryStream` / `StreamWriter` | BCL | Convert string to `Stream` for `IFileSaver.SaveAsync` | Desktop path |
| `Microsoft.Maui.Storage.FileSystem` | Ships with MAUI | `FileSystem.CacheDirectory` path for temp file | Mobile share path only |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `Share.RequestAsync` (mobile) | `IFileSaver.SaveAsync` (mobile) | FileSaver on iOS shows a Files picker, not a share sheet — wrong UX for mobile |
| Platform routing in one service | Separate `IMobileExportService` / `IDesktopExportService` | Overkill for two-platform routing; a single `#if` or `DeviceInfo.Platform` check in one service is simpler |

**Installation:** No new packages needed.

---

## Architecture Patterns

### Recommended Project Structure
```
SdCharacterSheet/
├── Services/
│   └── MarkdownExportService.cs   # Markdown builder + platform routing
SdCharacterSheet.Core/
│   (no changes — export is MAUI-layer concern)
SdCharacterSheet.Tests/
│   └── Services/
│       └── MarkdownExportServiceTests.cs  # Pure Markdown builder tests
```

### Pattern 1: Pure Markdown Builder + Platform Routing in One Service
**What:** `MarkdownExportService` has two responsibilities: (a) `BuildMarkdown(CharacterViewModel)` — pure string building, no I/O; (b) `ExportAsync(CharacterViewModel)` — calls BuildMarkdown, then routes to share sheet or save-as based on `DeviceInfo.Platform`.
**When to use:** Always — keeps the testable logic (string formatting) separate from untestable I/O.

```csharp
// Source: architecture decision — follows MauiCharacterFileService pattern
public class MarkdownExportService
{
    private readonly SdCharacterSheet.Services.IFileSaver _fileSaver;

    public MarkdownExportService(SdCharacterSheet.Services.IFileSaver fileSaver)
        => _fileSaver = fileSaver;

    public string BuildMarkdown(CharacterViewModel vm) { /* pure */ }

    public async Task ExportAsync(CharacterViewModel vm, CancellationToken ct = default)
    {
        var markdown = BuildMarkdown(vm);
        var fileName = BuildFileName(vm);

        if (DeviceInfo.Platform == DevicePlatform.iOS ||
            DeviceInfo.Platform == DevicePlatform.Android)
            await ShareAsync(markdown, fileName, ct);
        else
            await SaveAsync(markdown, fileName, ct);
    }
}
```

### Pattern 2: Mobile Share via CacheDirectory Temp File
**What:** Write Markdown to `FileSystem.CacheDirectory/{fileName}`, then pass to `Share.Default.RequestAsync` as a `ShareFileRequest`. MAUI auto-detects MIME from the `.md` extension.
**When to use:** iOS and Android only.

```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/data/share
private async Task ShareAsync(string markdown, string fileName, CancellationToken ct)
{
    var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
    await File.WriteAllTextAsync(filePath, markdown, System.Text.Encoding.UTF8, ct);

    await Share.Default.RequestAsync(new ShareFileRequest
    {
        Title = "Export character sheet",
        File = new ShareFile(filePath)
    });
}
```

### Pattern 3: Desktop Save-As via Existing IFileSaver
**What:** Convert Markdown string to `MemoryStream`, call `_fileSaver.SaveAsync(fileName, stream, ct)`. The existing `CommunityToolkitFileSaverAdapter` handles the Windows/macOS save-as dialog and throws `IOException` on failure.
**When to use:** Windows and macOS.

```csharp
// Source: existing CommunityToolkitFileSaverAdapter pattern
private async Task SaveAsync(string markdown, string fileName, CancellationToken ct)
{
    var bytes = System.Text.Encoding.UTF8.GetBytes(markdown);
    using var stream = new MemoryStream(bytes);
    await _fileSaver.SaveAsync(fileName, stream, ct);
}
```

### Pattern 4: Shell-Level ToolbarItem (All Tabs)
**What:** Add `ToolbarItem` to `AppShell.ToolbarItems` in XAML. Shell derives from `Page` so `ToolbarItems` is available at the Shell level — the item appears in the nav bar on every tab without modifying each page.
**When to use:** When an action must appear on all tabs identically.

```xml
<!-- AppShell.xaml — Source: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/toolbaritem -->
<Shell ...>
    <Shell.ToolbarItems>
        <ToolbarItem Text="Export"
                     Order="Primary"
                     Command="{Binding ExportCommand}"
                     CommandParameter="{Binding .}" />
    </Shell.ToolbarItems>
    <TabBar> ... </TabBar>
</Shell>
```

The `Command` binding requires `AppShell.BindingContext` to be set to a VM that exposes `ExportCommand`. Options:
- Add `ExportCommand` directly to `CharacterViewModel` (simplest — already the Shell's implicit data context via DI)
- Or set `AppShell.BindingContext` explicitly in `AppShell()` constructor

### Pattern 5: Filename Sanitization
**What:** Build filename as `{Name}-{Class}{Level}.md`, fall back to `Character.md` when Name or Class is empty. Strip filesystem-unsafe characters.
**When to use:** Always — Name and Class come from user text input.

```csharp
private static string BuildFileName(CharacterViewModel vm)
{
    var name = string.IsNullOrWhiteSpace(vm.Name) ? "Character" : vm.Name.Trim();
    var cls  = string.IsNullOrWhiteSpace(vm.Class) ? "" : vm.Class.Trim();
    var raw  = string.IsNullOrEmpty(cls) ? name : $"{name}-{cls}{vm.Level}";
    // Strip chars invalid on Windows: \ / : * ? " < > |
    var safe = string.Concat(raw.Split(Path.GetInvalidFileNameChars()));
    return safe + ".md";
}
```

### Pattern 6: SpellsKnown ViewModel Gap
**What:** `Character.SpellsKnown` exists on the model but `CharacterViewModel` has no `[ObservableProperty]` for it. Export reads it for the optional Spells section (D-21).
**Resolution options:**
1. Add `[ObservableProperty] private string spellsKnown = "";` to `CharacterViewModel` and populate it in `LoadCharacter()` — consistent with all other fields. Preferred.
2. Read directly from `vm.Character.SpellsKnown` inside the export service — avoids touching the ViewModel but creates an inconsistency where all other data goes through observable properties.

**Recommendation:** Option 1 — add the property. It is one line + one assignment in `LoadCharacter()`, and it keeps the ViewModel as the single source of truth for the export service.

### Anti-Patterns to Avoid
- **Sharing via plain text `ShareTextRequest`:** Some Markdown apps will not receive it as a file. Use `ShareFileRequest` with a temp `.md` file instead.
- **Writing temp file to `AppDataDirectory` on Android:** Android's `FileProvider` may expose data unexpectedly. Use `FileSystem.CacheDirectory` which is the documented safe location for shared temp files.
- **Calling `Share.RequestAsync` on desktop:** On Windows, the Share contract UI exists but is not a save-as dialog. Route desktop to `IFileSaver` instead.
- **Building the Markdown table by computing slot rows on the fly during I/O:** Build the full slot row list in a pure helper first, then serialize. Keeps logic testable.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Mobile file sharing | Custom `UIActivityViewController` bridge | `Share.Default.RequestAsync` + `ShareFileRequest` | MAUI built-in, cross-platform, correct MIME inference |
| Desktop save-as dialog | Win32 `SaveFileDialog` P/Invoke | Existing `CommunityToolkitFileSaverAdapter` / `IFileSaver` | Already wired, tested, handles cancellation |
| Markdown special character escaping | Custom escaper | None needed for this output — output is controlled (stat names are enum-like, values are ints) | User Notes is free text but goes into a `## Notes` section body, not a table cell — no escaping required |
| Platform detection | Compiler directives (`#if ANDROID`) | `DeviceInfo.Platform` runtime check | Supports single-file conditional without TFM splits |

**Key insight:** Every platform I/O primitive the export needs already exists in the project. This phase is purely additive: one new service, one ViewModel property, one toolbar item.

---

## Common Pitfalls

### Pitfall 1: ToolbarItem Not Appearing on Tab Pages When Defined at Shell Level
**What goes wrong:** A `ToolbarItem` added to `Shell.ToolbarItems` may not render on tab-based pages on all platforms due to known MAUI Shell issues with navigation bar visibility in `TabBar` mode.
**Why it happens:** MAUI's Shell navigation bar rendering differs between flyout mode and tab mode. Per-page `ToolbarItems` defined inside each `ContentPage` are more reliably rendered.
**How to avoid:** If `Shell.ToolbarItems` proves unreliable during testing, add the `ToolbarItem` to each tab page's `ContentPage.ToolbarItems` instead — same command binding, repeated three times in XAML. This is the safe fallback.
**Warning signs:** Export button visible on one tab but not others, or absent after tab switch.

### Pitfall 2: Android FileProvider Scope Warning
**What goes wrong:** `Share.Default.RequestAsync` on Android writes files via a `FileProvider`. If the file is outside the declared provider path, a `Java.Lang.IllegalArgumentException` is thrown at runtime.
**Why it happens:** Android requires all shared files to go through the `FileProvider` with declared paths.
**How to avoid:** Always write the temp `.md` file to `FileSystem.CacheDirectory` (not a subdirectory of it, not `AppDataDirectory`). MAUI's default `FileProvider` configuration covers `CacheDirectory` directly.
**Warning signs:** `IllegalArgumentException: Failed to find configured root` in Android logcat.

### Pitfall 3: Gear Table Row Count Mismatch
**What goes wrong:** The table has the wrong number of rows — either too few (items overflow) or too many (extra blank rows beyond `GearSlotTotal`).
**Why it happens:** Coin slots are not physical `GearItemViewModel` entries — they are a computed count (`CoinSlots`). Multi-slot items consume N physical slot rows. The table builder must walk the slot list carefully.
**How to avoid:** Build a `List<string>` of exactly `GearSlotTotal` items — allocate the list upfront, fill gear items (expanding multi-slot items), add coin rows, then fill remaining slots with empty strings. Assert `list.Count == GearSlotTotal` in a unit test.
**Warning signs:** Table row count doesn't match `GearSlotTotal`; coin slots not appearing.

### Pitfall 4: ExportCommand Binding on AppShell
**What goes wrong:** `Command="{Binding ExportCommand}"` on a Shell-level `ToolbarItem` fails silently if `AppShell.BindingContext` is not set.
**Why it happens:** Shell does not automatically inherit a `BindingContext` from the DI container. The binding must be wired explicitly.
**How to avoid:** In `AppShell.cs` constructor, resolve `CharacterViewModel` from DI and set it as `BindingContext`. Or add `ExportCommand` as a code-behind handler on `AppShell` that delegates to the injected service.
**Warning signs:** Export button tap does nothing; no error thrown.

### Pitfall 5: UTF-8 BOM in Exported File
**What goes wrong:** Using `Encoding.Default` or `new StreamWriter(stream)` may write a UTF-8 BOM, causing some Markdown readers to display garbage characters at the start.
**Why it happens:** `Encoding.Default` on Windows is UTF-8-with-BOM.
**How to avoid:** Use `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)` or `Encoding.UTF8` (which is BOM-less in .NET Core / .NET 5+).
**Warning signs:** Markdown file begins with `ï»¿` in a hex editor.

---

## Code Examples

Verified patterns from official sources:

### Mobile Share Sheet (file)
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/data/share
var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
await File.WriteAllTextAsync(filePath, markdown, Encoding.UTF8, ct);
await Share.Default.RequestAsync(new ShareFileRequest
{
    Title = "Export character sheet",
    File = new ShareFile(filePath)
});
```

### Desktop Save-As (via CommunityToolkitFileSaverAdapter)
```csharp
// Source: existing CommunityToolkitFileSaverAdapter.cs in this project
var bytes = Encoding.UTF8.GetBytes(markdown);
using var stream = new MemoryStream(bytes);
await _fileSaver.SaveAsync(fileName, stream, ct);
// Throws IOException on failure (per CommunityToolkitFileSaverAdapter implementation)
```

### Shell ToolbarItem (XAML)
```xml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/toolbaritem -->
<Shell.ToolbarItems>
    <ToolbarItem Text="Export"
                 Order="Primary"
                 Command="{Binding ExportCommand}" />
</Shell.ToolbarItems>
```

### Platform Routing
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/data/share (platform differences)
if (DeviceInfo.Platform == DevicePlatform.iOS ||
    DeviceInfo.Platform == DevicePlatform.Android)
    await ShareAsync(markdown, fileName, ct);
else
    await SaveAsync(markdown, fileName, ct);
```

### Gear Slot Table Builder Logic
```csharp
// Source: architecture pattern — derived from CONTEXT.md D-13 through D-18
var rows = new List<string>(vm.GearSlotTotal);

foreach (var item in vm.GearItems)
{
    rows.Add(item.Name);
    for (int i = 1; i < item.Slots; i++)
        rows.Add($"(cont. {item.Name})");
}

if (vm.CoinSlots > 0)
{
    for (int i = 0; i < vm.CoinSlots; i++)
        rows.Add("Coins");
}

while (rows.Count < vm.GearSlotTotal)
    rows.Add("");

// rows is now exactly GearSlotTotal entries
```

### Stat Section Builder
```csharp
// Source: architecture pattern — from CONTEXT.md D-10, D-11
// Stats section (STR, DEX, CON, INT, WIS, CHA)
foreach (var row in vm.StatRows)
{
    var sign = row.TotalScore >= 10 ? "+" : "";
    // D-10 format: **STR** 16 (+3)
    sb.AppendLine($"**{row.StatName}** {row.TotalScore} ({row.ModifierDisplay})");
    foreach (var bonus in row.BonusSources)
    {
        var value = bonus.BonusTo.Split(':').ElementAtOrDefault(1) ?? "";
        sb.AppendLine($"  - {bonus.Label}: {value}");
    }
}

// AC subsection — D-11: bonuses where BonusTo starts with "AC:"
var acBonuses = vm.Character.Bonuses.Where(b => b.BonusTo.StartsWith("AC:")).ToList();
var acTotal = acBonuses.Sum(b => {
    var v = b.BonusTo.Split(':').ElementAtOrDefault(1) ?? "0";
    return int.TryParse(v, out var n) ? n : 0;
});
sb.AppendLine($"**AC** {acTotal}");
foreach (var b in acBonuses)
{
    var value = b.BonusTo.Split(':').ElementAtOrDefault(1) ?? "";
    sb.AppendLine($"  - {b.Label}: {value}");
}
```

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 |
| Config file | SdCharacterSheet.Tests/SdCharacterSheet.Tests.csproj |
| Quick run command | `dotnet test SdCharacterSheet.Tests --filter "Category=Unit"` |
| Full suite command | `dotnet test SdCharacterSheet.Tests` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MRKD-01 | `BuildMarkdown` produces correct section order (Identity → Stats → Attacks → Currency → Gear → Notes) | unit | `dotnet test SdCharacterSheet.Tests --filter "FullyQualifiedName~MarkdownExportServiceTests"` | ❌ Wave 0 |
| MRKD-01 | Stats section: each stat shows `**NAME** total (mod)` followed by bonus source bullets | unit | same | ❌ Wave 0 |
| MRKD-01 | AC subsection rendered using `AC:` prefixed bonuses | unit | same | ❌ Wave 0 |
| MRKD-01 | Gear table has exactly `GearSlotTotal` rows | unit | same | ❌ Wave 0 |
| MRKD-01 | Multi-slot items expand to `(cont. Name)` rows | unit | same | ❌ Wave 0 |
| MRKD-01 | Coin rows appear when `CoinSlots > 0` | unit | same | ❌ Wave 0 |
| MRKD-01 | Spells section included only when `SpellsKnown` non-empty | unit | same | ❌ Wave 0 |
| MRKD-01 | Filename: `{Name}-{Class}{Level}.md` | unit | same | ❌ Wave 0 |
| MRKD-01 | Empty Name/Class falls back to safe filename | unit | same | ❌ Wave 0 |

**Note:** Platform share/save I/O (`ExportAsync`) is manual-only — requires a running MAUI app on device/simulator. All automated tests target the pure `BuildMarkdown` and `BuildFileName` methods.

### Sampling Rate
- **Per task commit:** `dotnet test SdCharacterSheet.Tests --filter "Category=Unit"`
- **Per wave merge:** `dotnet test SdCharacterSheet.Tests`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `SdCharacterSheet.Tests/Services/MarkdownExportServiceTests.cs` — covers MRKD-01 (all rows above)

Note: `MarkdownExportService` must be structured so `BuildMarkdown` and `BuildFileName` are callable from the `net10.0` test project without MAUI dependencies. Use the same inline-stub pattern established in Phase 2 — OR structure the service so the pure builder methods take only plain CLR types (or a thin interface) that the test project can satisfy directly. The simplest approach: accept `CharacterViewModel` directly — the test project already references `SdCharacterSheet.Core` and can define a test-local stub as in `CharacterViewModelTests.cs`. However, `CharacterViewModel` lives in the MAUI project (not Core), which creates a TFM dependency issue. Resolution: extract the data the builder needs into a plain DTO or use an interface — see "Open Questions" below.

---

## Open Questions

1. **Test access to `CharacterViewModel` from `net10.0` test project**
   - What we know: `CharacterViewModel` is in the MAUI project (`net10.0-ios;net10.0-maccatalyst;net10.0-windows`). The test project targets `net10.0` and only references `SdCharacterSheet.Core`. Prior phases solved this with inline test stubs.
   - What's unclear: Should `MarkdownExportService.BuildMarkdown` accept `CharacterViewModel` directly (requiring a test stub), or should it accept a plain DTO / interface that the test can construct without MAUI?
   - Recommendation: **Introduce a `CharacterExportData` record** (plain CLR, lives in `SdCharacterSheet.Core`) that `BuildMarkdown` accepts. `CharacterViewModel` maps to it before calling the service. This keeps `BuildMarkdown` in Core (testable), keeps platform I/O in the MAUI service, and avoids TFM issues entirely. Alternative: use the inline-stub pattern from `CharacterViewModelTests.cs` — simpler but creates a larger stub to maintain.

2. **AppShell BindingContext for ExportCommand**
   - What we know: `Shell.ToolbarItems` supports `Command="{Binding ExportCommand}"` but requires `BindingContext` to be set.
   - What's unclear: Should `ExportCommand` live on `CharacterViewModel` (simplest) or on a dedicated `ExportViewModel`?
   - Recommendation: Add `ExportCommand` directly to `CharacterViewModel` with an injected `MarkdownExportService`. This avoids a new ViewModel class and is consistent with existing RelayCommand patterns. Set `AppShell.BindingContext` to the injected `CharacterViewModel` in `AppShell.cs`.

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Xamarin.Essentials.Share` | `Microsoft.Maui.ApplicationModel.DataTransfer.Share` | MAUI launch | Built-in MAUI API, same model |
| Custom FileSaver per platform | `CommunityToolkit.Maui.Storage.IFileSaver` | CommunityToolkit.Maui v5+ | Already in project |

---

## Sources

### Primary (HIGH confidence)
- [MAUI Share API — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/data/share?view=net-maui-10.0) — `Share.RequestAsync`, `ShareFileRequest`, platform differences, Android FileProvider guidance
- [CommunityToolkit.Maui FileSaver — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/essentials/file-saver) — `SaveAsync` signature, permissions, platform behavior
- [MAUI ToolbarItem — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/toolbaritem?view=net-maui-10.0) — Shell-level toolbar, `Order`, `Command` binding
- Existing project source (`IFileSaver.cs`, `CommunityToolkitFileSaverAdapter.cs`, `CharacterViewModel.cs`, `AppShell.xaml`) — confirmed API shapes and existing wiring

### Secondary (MEDIUM confidence)
- WebSearch results confirming `Share.RequestAsync` iOS quirks (must not be called from a non-root ViewController context) — not a risk for MAUI command handler which runs on the main thread

### Tertiary (LOW confidence)
- None

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — all libraries already in project; APIs verified against official docs
- Architecture: HIGH — all patterns confirmed against official MAUI documentation + existing codebase
- Pitfalls: HIGH — Android FileProvider and Shell ToolbarItem issues confirmed in official docs and known MAUI issues
- Test architecture: MEDIUM — test isolation approach for `BuildMarkdown` requires a design decision (see Open Questions)

**Research date:** 2026-03-21
**Valid until:** 2026-09-21 (stable MAUI APIs; CommunityToolkit.Maui versioning may shift)
