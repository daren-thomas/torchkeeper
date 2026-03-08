# Domain Pitfalls

**Domain:** .NET MAUI cross-platform character sheet app (Shadowdark RPG)
**Researched:** 2026-03-08

---

## Critical Pitfalls

Mistakes that cause rewrites or major structural rework.

---

### Pitfall 1: Calculated Properties That Don't Update When Inputs Change

**What goes wrong:** You bind a UI label to a computed property like `StatModifier` (which derives from `BaseScore` plus bonus sources). When `BaseScore` changes, the label never updates because MAUI's binding engine only re-reads a property when `PropertyChanged` fires for *that specific property name*. Computed properties have no backing field — they never raise their own `PropertyChanged` event.

**Why it happens:** Developers treat computed properties like regular C# properties. They work fine for static reads, but MAUI bindings are one-time evaluated at bind-time unless notified. The `SetProperty` helper in CommunityToolkit.Mvvm only raises `PropertyChanged` for the property being set, not for properties that *depend* on it.

**Consequences:** Stat modifiers display stale values. HP totals don't update when gear is added. Slot counts freeze. The app looks like it's broken, and the bug is nearly invisible during initial development when you set values directly and check the screen.

**Prevention:**
- Use `[NotifyPropertyChangedFor(nameof(ComputedProperty))]` on every backing field that feeds a calculated value. This is the MVVM Toolkit source generator approach (requires `partial class` and `[ObservableProperty]`).
- Alternatively, in manual `SetProperty` calls, chain `OnPropertyChanged(nameof(StatModifier))` calls explicitly after updating the backing field.
- Create a deliberate list of the dependency graph at design time: "StatModifier depends on BaseScore and ActiveBonuses. When either changes, fire PropertyChanged for StatModifier."
- Unit-test each calculated property by mutating all inputs and asserting the output value changes.

**Detection warning signs:**
- UI values look correct on initial load but freeze after edits.
- Values update on some platforms but not others (hot reload masks the bug during development).

**Phase:** Address in Phase 1 (core data model) — establish the pattern before any UI is wired up.

---

### Pitfall 2: File Picker Platform Divergence Causing Silent Failures

**What goes wrong:** `FilePicker.Default.PickAsync` is called with a custom file type filter. It works on Windows (uses file extensions: `.sdchar`). It silently shows all files on iOS (requires UTType strings like `public.data`). It crashes or shows no files on Android (needs MIME type: `application/octet-stream`). On Mac Catalyst in App Store builds, the picker fails to open entirely without specific entitlements.

**Why it happens:** Each platform uses a completely different file type identification system:
- Windows: file extensions (`.cbr`, `.cbz`)
- Android: MIME types (`application/comics`)
- iOS/macOS: Uniform Type Identifiers (`public.my.comic.extension`)

`FilePickerFileType` requires a per-platform dictionary. Developers who test only on one platform ship a picker that misbehaves or silently accepts wrong file types on others.

Additionally, on Mac Catalyst App Store builds, the file picker will not open unless specific entitlements are added to the app:
```xml
<key>com.apple.security.files.user-selected.read-only</key>
<true/>
<key>com.apple.security.files.downloads.read-only</key>
<true/>
```

**Consequences:** Users cannot open their character files on iOS or Android. The Mac Catalyst build is rejected from the App Store. The app becomes platform-only in practice.

**Prevention:**
- Define `FilePickerFileType` with all four platform entries from day one:
  ```csharp
  var sdcharType = new FilePickerFileType(
      new Dictionary<DevicePlatform, IEnumerable<string>>
      {
          { DevicePlatform.WinUI, new[] { ".sdchar" } },
          { DevicePlatform.Android, new[] { "application/octet-stream" } },
          { DevicePlatform.iOS, new[] { "public.data" } },
          { DevicePlatform.macOS, new[] { "public.data" } },
      });
  ```
- Add Mac Catalyst entitlements in the iOS entitlements file before first TestFlight build.
- Use `OpenReadAsync()` rather than `FullPath` — the full path property is not reliable across platforms (particularly Android content URIs).
- Test the file picker on all four targets before any milestone is called complete.

**Detection warning signs:**
- File picker works in Windows debug but produces no results on Android emulator.
- Mac Catalyst build passes debug but fails App Store validation.

**Phase:** Phase 1 (save/load). Set up the full platform dictionary before the first working save.

---

### Pitfall 3: Windows MSIX Packaged App Virtual File System Hiding Files

**What goes wrong:** The app saves a character file to what appears to be a user-writable path (e.g., `Documents\ShadowdarkSheet\character.sdchar`). On Windows with MSIX packaging (the default for new MAUI projects), the OS redirects writes to a virtualized location under `%LOCALAPPDATA%\Packages\[AppId]\`. The file is created, the app can read it back, but the user cannot find it in Explorer at the path shown in the UI.

**Why it happens:** Packaged Windows apps (MSIX) operate over a virtual file system. File system write operations to some locations are transparently redirected. Microsoft documents this behavior but it is not obvious during development because the app reads back its own writes successfully through the same virtualized layer.

**Consequences:** Users get confused when they try to back up or share character files. The file sharing story breaks. "Open in file manager" buttons point to the wrong path.

**Prevention:**
- For user-facing character saves, use `FileSavePicker` (save dialog) rather than programmatic path writes. This routes through the OS file picker and places the file where the user expects it.
- For recent-file tracking, store the path returned by the save dialog, not a constructed path.
- If app-internal storage is needed (autosave), use `FileSystem.Current.AppDataDirectory` explicitly and document that this is internal-only storage.
- Consider shipping as an unpackaged Windows app (set `<WindowsPackageType>None</WindowsPackageType>`) if App Store distribution is not a goal — this eliminates the virtual file system entirely.

**Detection warning signs:**
- File is "saved successfully" but not visible in the expected Explorer folder.
- App can re-open the file but user cannot locate it manually.

**Phase:** Phase 1 (save/load). Decide packaged vs. unpackaged at project setup, not after.

---

### Pitfall 4: Rigid Native File Format Baked In Too Early

**What goes wrong:** The first working save uses a direct `JsonSerializer.Serialize(characterViewModel)` call, serializing the ViewModel class directly. Three phases later, new fields are added, bonus sources are restructured, or the inventory model changes. Old save files fail to deserialize. Users lose characters.

**Why it happens:** Serializing the ViewModel is fast to implement but ties the file format to the internal data model. Any refactor that changes a property name, type, or nesting structure breaks existing files. Without a versioning strategy, there is no path to migrate old saves.

**Consequences:** Breaking file format changes require a migration step. Without one, existing characters are corrupted on update. With one, the migration code is untested and often discarded after the "one-time fix." The format is also forced to carry ViewModel-specific concerns (IsSelected, IsExpanded state) that have no business being persisted.

**Prevention:**
- Define a separate `CharacterSaveData` class (a plain data object, no ViewModel inheritance) as the canonical serialization target from day one.
- Add a `int Version = 1` field to the root save object.
- Write a `CharacterSaveDataMigrator` that takes a `JObject` and a version number, and returns a migrated `JObject`. Even if V1 migration is a no-op, the scaffold exists.
- Keep the save format minimal: only the user's intentional data, not computed values. Calculated modifiers should be re-derived at load time, not stored.
- Use `[JsonIgnore]` or similar attributes to explicitly exclude ViewModel state from the serialized form if using a shared class.

**Detection warning signs:**
- You add a new required field and realize you need a default value for old files.
- A rename refactor causes a deserialization null somewhere.

**Phase:** Phase 1 (save/load). This is a day-one architecture decision.

---

### Pitfall 5: Over-Engineering the Bonus System Upfront

**What goes wrong:** Shadowdark's bonus structure (`sourceType`, `sourceCategory`, `bonusTo`, `gainedAtLevel`) looks like it calls for a generic rule engine. Developers build an abstract `IBonusSource` hierarchy, a `BonusEvaluator` with visitor pattern, a toggleable source graph with cascading enable/disable logic. The system handles every conceivable bonus type but has no working UI for two months.

**Why it happens:** The Brim.json example shows structured bonuses and the requirements ask for per-source toggle. This looks like a complex problem that warrants a complex solution. It is not — Shadowdark's bonus system is simple: a flat list of named bonuses, each with a target stat and a numeric value. Toggle means "include or exclude from the sum."

**Consequences:** Engineering debt in infrastructure that isn't validated by real usage. The toggle UI gets built for an abstraction that doesn't match what users actually need. When you discover that "situational toggle" means "check a box before rolling," not "complex rule graph," you rewrite anyway.

**Prevention:**
- Start with the simplest data model that passes: `List<BonusEntry>` where each entry has `string Name`, `string TargetStat`, `int Value`, `bool IsActive`.
- Implement toggle as `IsActive = !IsActive` and recalculate the sum.
- Add complexity only when a concrete requirement demands it. "Equipped item bonus" vs. "talent bonus" is a display categorization, not a separate code path.
- Write the slot count and stat modifier display to work first. Generalize later if the actual bonus interactions require it.

**Detection warning signs:**
- You are designing interfaces for bonus sources before any UI renders a modifier.
- The bonus system has more than three classes before you have a working stat block screen.

**Phase:** Phase 1/2. Deliberately constrain the initial implementation. Flag any urge to abstract as requiring a working concrete case first.

---

## Moderate Pitfalls

---

### Pitfall 6: StackLayout Nesting Instead of Grid for Form-Style Layouts

**What goes wrong:** The stat block (STR/DEX/CON etc.) is laid out as nested `StackLayout` rows (horizontal StackLayout inside vertical StackLayout for each row). Performance degrades with each additional nesting level because `StackLayout` triggers extra layout passes. On Android especially, deeply nested StackLayouts produce janky scrolling.

**Prevention:**
- Use `Grid` for any tabular or form-style layout. The stat block is a two-column grid (label + value) — model it that way from the start.
- Use `HorizontalStackLayout` and `VerticalStackLayout` (the MAUI-optimized variants) instead of `StackLayout` for simple stacks that don't need Grid alignment.
- Avoid StackLayout-inside-StackLayout for anything more than two levels deep.

**Phase:** Phase 2 (stat block UI). Apply Grid patterns during the first UI build, not as a later optimization.

---

### Pitfall 7: iOS Keyboard Obscuring Entry Fields Without Scroll Wrappers

**What goes wrong:** Character fields (name, notes, gear entries) are in a scrollable form. When the user taps an Entry near the bottom of the screen on iOS, the software keyboard obscures the field. The user can't see what they're typing.

**Why it happens:** MAUI on iOS provides automatic keyboard scroll adjustment (`KeyboardAutoManagerScroll`), but this only works correctly when the Entry is inside a `ScrollView`. If the layout is a plain `Grid` or `StackLayout` without a wrapping `ScrollView`, the automatic scroll has nowhere to scroll to and the field stays hidden.

**Prevention:**
- Always wrap form content in a `ScrollView`.
- Test keyboard behavior on an iOS device or simulator early (not only Windows or macOS) since keyboard avoidance is iOS-specific.
- On Android, set `android:windowSoftInputMode="adjustResize"` in the Android manifest to ensure the layout resizes rather than being covered.

**Phase:** Phase 2 (character data entry). Verify on iOS during the first form implementation.

---

### Pitfall 8: Missing `[NotifyPropertyChangedFor]` on Gear Slot Totals

**What goes wrong:** `SlotsUsed` is calculated from the gear list. An `ObservableCollection<GearItem>` is used for the list. Items are added and removed, and the collection fires `CollectionChanged` — but `SlotsUsed` is a computed property on the ViewModel that reads `.Sum(g => g.Slots)`. The `CollectionChanged` event does not automatically trigger re-evaluation of computed properties that read from the collection. The slots-used display stays stale.

**Why it happens:** `ObservableCollection<T>` notifies when items are added/removed/replaced in the *collection* itself. It does not notify that any external property *derived from* the collection has changed. The binding engine has no way to know that `SlotsUsed` reads from the gear list.

**Prevention:**
- Subscribe to `GearItems.CollectionChanged` in the ViewModel constructor and call `OnPropertyChanged(nameof(SlotsUsed))` in the handler.
- Also subscribe to `PropertyChanged` on individual `GearItem` instances if slot counts on items can change (e.g., if the user edits item quantity). Unsubscribe when items are removed.
- Write this pattern once and apply it to: SlotsUsed, SlotsTotal (if gear count affects it), and coin weight.

**Phase:** Phase 2 (inventory). Establish this pattern during the first CollectionView implementation.

---

### Pitfall 9: Importing Shadowdarklings JSON Directly Into App Data Model Without Isolation

**What goes wrong:** The import routine deserializes `Brim.json` directly into the app's domain model classes. The Shadowdarklings format has quirks: `bonusTo` uses a colon-delimited string (`"DEX:+2"`), `attacks` is a pre-formatted string array, `ledger` carries transaction history, and field names like `Rolled12TalentOrTwoStatPoints` are Shadowdarklings-specific. Mapping these directly into the app model either forces the app model to carry Shadowdarklings-specific fields forever, or causes mapping errors when Shadowdarklings changes its format.

**Prevention:**
- Create `ShadowdarlingsImportDto` classes that mirror the Shadowdarklings JSON shape exactly (including the quirky names).
- Write a `ShadowdarlingsImporter` service that maps `ShadowdarlingsImportDto` to the app's domain model. All the parsing of `"DEX:+2"` strings happens inside this service.
- The app's domain model never knows about the Shadowdarklings format. If Shadowdarklings changes their export schema, only the DTO and importer need updating.
- The import is one-way (as specified in requirements), so there is no round-trip concern.

**Phase:** Phase 1 (import). Structure the importer as a dedicated service before writing any parsing logic.

---

### Pitfall 10: Treating Mac Catalyst as "Free" macOS Support

**What goes wrong:** Mac Catalyst is MAUI's macOS target. It runs an iOS-derived app via a compatibility layer — it is not a native macOS app. Several iOS behaviors carry over that feel wrong on desktop: no right-click context menus by default, no keyboard shortcut support, title bar behavior differs, window resizing triggers unusual layout reflows. The assumption that "builds on macOS = good macOS experience" leads to a rough desktop feel.

**Prevention:**
- Test on Mac Catalyst explicitly and treat it as a separate platform during QA, not a bonus platform.
- For save/open file dialogs on Mac, use `FilePicker`/`FileSavePicker` rather than programmatic path access — the Mac App Sandbox blocks arbitrary file system access.
- Accept that some desktop polish (proper menu bar items, keyboard shortcuts for save/open) requires platform-specific code or will simply be absent.
- If shipping to the Mac App Store, add App Sandbox entitlements early: `com.apple.security.files.user-selected.read-write` for file read/write.

**Phase:** Phase 1 and onward. Establish mac Catalyst as a test target from the first save/load cycle.

---

## Minor Pitfalls

---

### Pitfall 11: Using `FullPath` from FilePicker Instead of `OpenReadAsync`

**What goes wrong:** `fileResult.FullPath` works on Windows and macOS but returns a content URI on Android (not a real file path). Attempting to `File.OpenRead(fullPath)` on Android throws or reads nothing.

**Prevention:** Always use `await fileResult.OpenReadAsync()` to get a stream. Never assume `FullPath` is a valid filesystem path on all platforms.

**Phase:** Phase 1. Write the file open utility once using `OpenReadAsync` and never use `FullPath` directly.

---

### Pitfall 12: `Auto`-Sized Grid Rows/Columns in High-Frequency-Update Layouts

**What goes wrong:** The stat block uses `Auto` height rows for each stat entry. Every time a stat value changes and triggers a UI update, MAUI recalculates the height of each `Auto` row. With six stats and multiple bonus indicators, this produces unnecessary layout passes.

**Prevention:**
- Use fixed or `*` (star) sizing for Grid rows where the height is known or proportional.
- Reserve `Auto` for rows where content genuinely varies in size (freeform note fields, dynamic lists).

**Phase:** Phase 2 (stat block). Use fixed sizing in the initial design.

---

### Pitfall 13: Workload Installation Conflicts Between VS and dotnet CLI

**What goes wrong:** Installing `dotnet workload install maui` after the Visual Studio installer has already configured MAUI workloads can produce a state where VS cannot locate the workloads. Build errors appear even though everything looks installed.

**Prevention:**
- Install MAUI workloads through one mechanism only: either Visual Studio Installer OR `dotnet workload install maui`, not both.
- If the conflict occurs, the fix requires uninstalling all .NET SDKs from Control Panel and reinstalling cleanly through VS.

**Phase:** Project setup (before Phase 1). Document the chosen installation method in project onboarding notes.

---

### Pitfall 14: Ignoring the `Contacts` Namespace Collision on iOS/macOS

**What goes wrong:** If any code in the project references `Microsoft.Maui.ApplicationModel.Communication.Contacts`, iOS and macOS builds fail with "The type or namespace name 'Default' does not exist in the namespace 'Contacts'". This is a known MAUI issue where iOS/macOS has a system `Contacts` namespace that conflicts.

**Prevention:**
- Avoid `using Microsoft.Maui.ApplicationModel.Communication;` at the file level for files targeting iOS/macOS.
- Use the fully-qualified name: `Microsoft.Maui.ApplicationModel.Communication.Contacts.Default`.
- This app doesn't use the Contacts API, but the pattern applies to any namespace collision with Apple platform namespaces.

**Phase:** N/A for this project (Contacts API not used), but relevant to any future API additions.

---

### Pitfall 15: Markdown Export Naive String Concatenation

**What goes wrong:** The Markdown export requirement produces a formatted character sheet. Naive string concatenation with `+=` in a loop over gear items produces O(n²) string allocations. For a character with 30+ gear items and a long ledger, this is a noticeable pause.

**Prevention:**
- Use `StringBuilder` for any multi-line text assembly.
- Or use an interpolated string handler for smaller outputs.
- The character sheet is not large enough to be a serious performance problem, but establish the right pattern so it doesn't become one with large ledger histories.

**Phase:** Phase 3 (export). A one-time implementation choice.

---

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| Phase 1: Core data model | Rigid save format baked in too early (Pitfall 4) | Define `CharacterSaveData` DTO with version field before first serialize |
| Phase 1: Import | Direct Shadowdarklings JSON coupling (Pitfall 9) | Isolate behind `ShadowdarlingsImportDto` + importer service |
| Phase 1: Save/Load | File picker platform divergence (Pitfall 2) | Define all four platform entries in `FilePickerFileType` on day one |
| Phase 1: Windows | MSIX virtual file system hiding files (Pitfall 3) | Use save dialog, not programmatic path writes |
| Phase 2: Stat block | Calculated properties not updating (Pitfall 1) | Apply `[NotifyPropertyChangedFor]` to every backing field that feeds a computed value |
| Phase 2: Inventory | Gear slot total not updating (Pitfall 8) | Subscribe to `CollectionChanged` and raise `SlotsUsed` PropertyChanged in handler |
| Phase 2: Layout | StackLayout nesting for stat grid (Pitfall 6) | Use `Grid` for stat block from the first implementation |
| Phase 2: Mobile | Keyboard obscuring entry fields on iOS (Pitfall 7) | Wrap all form content in ScrollView; test on iOS simulator |
| Phase 2: Bonus system | Over-engineering toggleable bonuses (Pitfall 5) | Start with `List<BonusEntry>` + `IsActive` flag, no abstractions |
| Phase 3: Export | String concatenation in Markdown export (Pitfall 15) | Use `StringBuilder` |
| All phases: macOS | Mac Catalyst not being treated as a real target (Pitfall 10) | Include Mac Catalyst in every test cycle, add entitlements early |

---

## Sources

- Microsoft Learn — FilePicker in .NET MAUI: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker
- Microsoft Learn — File System Helpers: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers
- Microsoft Learn — Windows Setup / MSIX Packaging: https://learn.microsoft.com/en-us/dotnet/maui/windows/setup
- Microsoft Learn — .NET MAUI Troubleshooting Known Issues: https://learn.microsoft.com/en-us/dotnet/maui/troubleshooting
- Microsoft Learn — CommunityToolkit.Mvvm ObservableObject: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observableobject
- Microsoft Learn — CommunityToolkit.Mvvm ObservableProperty + NotifyPropertyChangedFor: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty
- Microsoft Learn — .NET MAUI Data Binding: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/
- Microsoft Learn — Entry Control Platform Differences: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/entry
- Microsoft Learn — Layouts: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/layouts/
- Confidence: MEDIUM-HIGH. Critical pitfalls verified against official Microsoft docs. Pitfalls 5, 6, 12 draw on established .NET MAUI community patterns and official layout guidance. Pitfall 5 (over-engineering) is assessed from domain experience and corroborated by the simplicity visible in Brim.json's bonus structure.
