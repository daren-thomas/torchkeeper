# Project Research Summary

**Project:** Shadowdark Character Sheet (.NET MAUI)
**Domain:** Cross-platform TTRPG character sheet — play management focus (Shadowdark RPG)
**Researched:** 2026-03-08
**Confidence:** MEDIUM-HIGH

## Executive Summary

This is a document-centric single-character-sheet app, not a roster or campaign management tool. The correct mental model is a mobile-first form with derived calculations, file-based persistence, and one-way import from an external builder (Shadowdarklings.net). Experts build this category of app with .NET MAUI Shell + MVVM using CommunityToolkit.Mvvm, a singleton CharacterViewModel shared across all tab pages, and System.Text.Json for a versioned flat-file format. The data surface is well-understood via the `Brim.json` export example: stats, bonuses with structured source metadata, gear with slot math, magic items, per-level history, and attacks with finesse/range flags.

The recommended approach is to build bottom-up: pure domain models first, then services (file I/O, import, export), then ViewModels, then XAML pages. This order allows the core logic — slot calculations, stat modifier derivations, bonus toggle state — to be unit-tested before any UI is written. The most technically sensitive area is the bonus toggle system: the Shadowdarklings data model shows a flat list of named bonus sources each targeting a stat with a numeric value. This is simple addition, not a rule engine. The strongest architectural recommendation is to resist over-engineering this system.

The primary risks are all in Phase 1: file format versioning baked in too late, file picker platform divergence across iOS/Android/Windows/macOS, and MSIX virtual file system surprises on Windows. All three are well-documented, preventable day-one decisions. Secondary risks are UI-layer: computed properties not propagating updates (a MAUI binding engine requirement, not a bug), and layout performance from nested StackLayouts. These are caught early if ViewModels are unit-tested and the stat block UI uses Grid from the start.

---

## Key Findings

### Recommended Stack

The stack is minimal and first-party. Only two NuGet packages are needed: `CommunityToolkit.Mvvm` for source-generator-based MVVM boilerplate elimination, and `CommunityToolkit.Maui` for the `FileSaver` (save-as dialog) and Popup controls. Everything else — JSON serialization, file I/O, file picker, DI container, share sheet, and Essentials APIs — ships in-box with .NET MAUI. No commercial control suites are justified; no third-party persistence layer is warranted. The native save format is a `.sdchar` file containing UTF-8 JSON with a `version` field.

**Important version note:** Research was conducted against training data with an August 2025 cutoff. .NET 10 may have shipped as LTS by March 2026. Verify whether to target .NET 9 or .NET 10 before project creation, and resolve current stable NuGet versions via `dotnet add package`.

**Core technologies:**
- `.NET MAUI 9.x (or 10.x)`: Cross-platform UI framework — mandated by project constraints; targets Windows, macOS, iOS, Android from one codebase
- `CommunityToolkit.Mvvm ~8.3.x`: MVVM infrastructure — source generators eliminate INotifyPropertyChanged boilerplate; zero runtime overhead; Microsoft community project
- `CommunityToolkit.Maui ~9.x`: Extended controls — provides `FileSaver` (save-as dialog) and Popup API for bonus drill-down; no equivalent in built-in MAUI
- `System.Text.Json (built-in)`: Serialization — AOT-safe, zero dependency, correct choice for document-centric file format
- `xUnit ~2.9.x`: Unit testing — test ViewModels and services on host machine; skip device-required Appium/UITest for this scope

### Expected Features

The feature set divides cleanly into three buckets. Table stakes are everything a player needs to run a session. Shadowdark-specific table stakes are rules the system enforces that generic TTRPG apps miss. Differentiators are where this app earns its place alongside Shadowdarklings.net.

**Must have (table stakes):**
- All 6 core stats (STR/DEX/CON/INT/WIS/CHA) with modifiers — session-critical
- HP tracking with tap-to-adjust — damage happens constantly at the table
- Gear inventory with per-item slot display and slot total (used/max) — Shadowdark's core survival mechanic
- Gear slot total = STR score (not modifier) — Shadowdark-specific rule players expect automated
- Coin slot calculation (1 slot per 20 coins across GP+SP+CP) — easy to forget; automation removes friction
- Magic items displayed separately from mundane gear, with full properties (benefits, curses, personality)
- Attack display derived from weapon + stats + class bonus — with finesse (higher of STR/DEX) and range flags
- Shadowdarklings JSON import — zero re-entry for existing players; one-way only
- Native file save/load (`.sdchar` format) — persistence between sessions
- Character identity fields (name, class, ancestry, level, title, alignment, background, deity, XP, languages)
- Currency tracking (GP/SP/CP) with coin slot auto-calculation
- Per-level talent and HP roll record — players want a log of what they chose at each level
- Freeform notes field — players always need somewhere to write things
- Formatted Markdown export — explicit project requirement; Shadowdarklings has no export

**Should have (competitive):**
- Stat bonus drill-down with source breakdown — "Why is my DEX 16?" transparency Shadowdarklings lacks
- Bonus source toggle (on/off per bonus) — situational bonuses change depending on equipment and effects; called out explicitly in PROJECT.md
- Per-level advancement history display — full readable log of talent names, descriptions, and HP rolls
- Magic item detail view — expose benefits, curses, and personality traits fully

**Defer (v2+):**
- Currency ledger display — data comes from import; low urgency for play management
- Slot visualization (progress bar) — text "8/9" is functional; visual bar is nice-to-have
- Bonus source category toggles — high UX complexity; validate actual player usage before building

### Architecture Approach

The architecture is standard MAUI MVVM with one critical structural constraint: all tab pages share a single `CharacterViewModel` singleton registered in DI. This is not optional — gear slots are derived from the STR stat, so a change made on the Stats tab must immediately reflect in the Gear tab's slot math. Independent per-tab ViewModels would require synchronization logic that introduces bugs. Below the ViewModel is a service layer (`ICharacterService`, `IImportService`, `IMarkdownExportService`) that the ViewModel calls into. Below services are pure domain model classes with no MAUI dependency.

Navigation uses Shell with a bottom TabBar (5 tabs: Stats, Gear, Level, Notes, Settings). Detail and modal routes (stat bonus drill-down, gear item edit, import flow) are push-navigated from the tab pages. The character file format (`.sdchar`) is app-owned JSON — never the Shadowdarklings format, which is import-only.

**Major components:**
1. `Character` (root aggregate model) — pure data, no MAUI dependency; owns `BaseStats`, `BonusSources`, `Gear`, `MagicItems`, `Levels`, currency, notes
2. `CharacterViewModel` (singleton) — exposes all character state and computed properties (stat totals, modifiers, slot counts); coordinates saves; owns child `StatBlockViewModel` instances and `GearViewModel`
3. `ICharacterService` — serialize/deserialize `.sdchar` via System.Text.Json; uses `FilePicker` and `FileSaver` for user-facing open/save
4. `IImportService` — maps Shadowdarklings JSON (`ShadowdarlingsImportDto`) to internal `Character`; isolated behind a DTO so external format changes don't leak into the domain model
5. `IMarkdownExportService` — renders character as a formatted Markdown string via `StringBuilder`; shares via MAUI Share API on mobile, `FileSaver` on desktop
6. XAML Tab Pages — zero business logic in code-behind; pure bindings to CharacterViewModel

### Critical Pitfalls

1. **Calculated properties not firing PropertyChanged** — MAUI bindings only update when `PropertyChanged` fires for that specific property name. Every computed property (stat modifier, gear slot total) must chain notifications from its inputs using `[NotifyPropertyChangedFor]`. Establish this pattern before any UI is wired. Unit-test every computed property.

2. **File picker platform divergence** — `FilePickerFileType` requires a per-platform dictionary (Windows: `.sdchar` extension; Android: MIME type; iOS/macOS: UTI). Define all four entries on day one. Never use `FullPath` from picker results; always use `OpenReadAsync()`. Add Mac Catalyst App Sandbox entitlements before first TestFlight build.

3. **MSIX virtual file system on Windows** — MSIX-packaged apps redirect writes to a virtualized path the user cannot find in Explorer. Use `FileSavePicker` (save dialog) for user-facing saves, not programmatic path writes. Decide packaged vs. unpackaged at project setup.

4. **Rigid save format baked in too early** — serializing the ViewModel class directly ties the file format to internal structure. Any refactor breaks existing saves. Define a separate `CharacterSaveData` DTO with a `version: 1` field as the canonical serialization target from day one.

5. **Over-engineering the bonus system** — Shadowdark bonuses are a flat list of `{TargetStat, Value, IsActive}` entries. Toggle = flip `IsActive`, recompute the sum. Resist building a generic rule engine or abstract `IBonusSource` hierarchy before any UI renders a modifier value.

---

## Implications for Roadmap

Based on the dependency graph from ARCHITECTURE.md and the phase warnings from PITFALLS.md, a three-phase structure is suggested. The build order flows strictly bottom-up: data model, then services, then ViewModel, then UI, then export.

### Phase 1: Foundation — Data Model, Services, and File I/O

**Rationale:** All UI depends on the domain model, all ViewModel logic depends on services, and all five critical pitfalls need to be addressed here before any UI is built. This phase has no visible UI milestone but it is the riskiest phase. Getting the file format, import isolation, and file picker platform handling right now prevents rewrites later.

**Delivers:**
- `Character` model and all sub-models (`BonusSource`, `GearItem`, `MagicItem`, `LevelRecord`) — pure C#, no MAUI
- `CharacterSaveData` DTO with `version: 1` field for the native `.sdchar` format
- `ICharacterService` implementation with cross-platform FilePicker (all 4 platform entries) and FileSaver
- `IImportService` with `ShadowdarlingsImportDto` — maps `rolledStats` (not `stats`) to `baseStats`, parses `bonuses[]` into `BonusSource` records
- `MauiProgram.cs` DI registration — CharacterViewModel singleton, services singletons, pages transient
- xUnit test project scaffolded; import service tested against `Brim.json`

**Addresses:** Shadowdarklings JSON import (FEATURES.md), native file save/load (FEATURES.md)

**Avoids:** Pitfall 4 (rigid save format), Pitfall 9 (import coupling), Pitfall 2 (file picker divergence), Pitfall 3 (MSIX virtual FS), Pitfall 11 (FullPath vs OpenReadAsync)

### Phase 2: Core Character Sheet — Stats, Gear, and Display

**Rationale:** With services established and unit-tested, the ViewModel and tab UI can be built against stable interfaces. This is the main visible milestone — a working, interactive character sheet. The bonus toggle system belongs here at its simplest form: `List<BonusSource>` + `IsActive` flag. Do not generalize until Phase 2 is complete and tested.

**Delivers:**
- `CharacterViewModel` singleton with all computed properties: stat totals, modifiers, gear slot total, coin slots, armor class, HP max
- `StatBlockViewModel` (per-stat) with `BonusSources` collection, `Total`, `Modifier`; `CollectionChanged` wired to propagate `PropertyChanged` for computed values
- Shell + TabBar (5 tabs); all pages receive CharacterViewModel via DI constructor injection
- Stats tab: 6 stat blocks with modifier display, HP tracker (tap-to-adjust), identity fields, XP, attack display
- Gear tab: slot counter (live), gear list with CollectionView (swipe-to-delete), magic items section, coin totals with auto-calculated coin slots
- Level tab: per-level records (HP rolled, talent chosen)
- Notes tab: freeform text, spells known
- Settings tab: Load / Save / Import file actions
- Modal: `StatBonusDetailPage` (stat drill-down with toggle checkboxes)
- Modal: `GearItemEditPage` (add/edit gear item)
- Modal: `ImportPage` (file picker + import flow)
- IsDirty pattern for unsaved-state prompts

**Uses:** CommunityToolkit.Mvvm source generators (`[ObservableProperty]`, `[RelayCommand]`, `[NotifyPropertyChangedFor]`); Grid layout for stat block (not nested StackLayout); ScrollView wrapping all form content for iOS keyboard avoidance

**Implements:** CharacterViewModel, StatBlockViewModel, GearViewModel (ARCHITECTURE.md component boundaries)

**Avoids:** Pitfall 1 (stale computed properties), Pitfall 5 (over-engineered bonus system), Pitfall 6 (StackLayout nesting), Pitfall 7 (iOS keyboard), Pitfall 8 (ObservableCollection slot totals)

### Phase 3: Export and Polish

**Rationale:** Markdown export is isolated from the core data model and can be deferred without blocking any other feature. It is the least risky phase and a natural finishing milestone before the app is considered shippable.

**Delivers:**
- `IMarkdownExportService` implementation using `StringBuilder` (not string concatenation in loops)
- Share sheet integration via MAUI `Share.RequestAsync()` on iOS/Android
- `FileSaver` integration for Windows/macOS Markdown save-as
- Mac Catalyst testing pass: entitlements, file picker behavior, keyboard shortcuts
- Windows unpackaged vs. packaged decision finalized; file path UX verified
- Full cross-platform smoke test on all 4 targets

**Addresses:** Formatted Markdown export (FEATURES.md)

**Avoids:** Pitfall 15 (naive string concatenation), Pitfall 10 (Mac Catalyst treated as free)

### Phase Ordering Rationale

- Phase 1 must precede Phase 2 because all ViewModel logic depends on the domain model and services being stable. Reversing this order forces ViewModel rewrites when the data model changes.
- The bonus toggle system is placed in Phase 2 at its simplest form deliberately. PITFALLS.md warns that the structured bonus metadata in `Brim.json` looks more complex than it is. Building the flat `List<BonusSource> + IsActive` version and validating it with real use before adding categorization or cascading logic prevents the most likely over-engineering failure mode.
- Import (Phase 1) is decoupled from display (Phase 2) by design: the import service hands a fully-populated `Character` model to the ViewModel, and the ViewModel doesn't know or care that it came from a Shadowdarklings file vs. a native `.sdchar`.
- Export (Phase 3) has no upstream dependencies except the final `Character` model shape, making it safely deferrable and testable in isolation.

### Research Flags

Phases with well-documented patterns (skip `/gsd:research-phase` during planning):
- **Phase 1 (file I/O, DI, MAUI project setup):** Official Microsoft docs are authoritative; patterns are established
- **Phase 2 (MVVM, Shell TabBar, CommunityToolkit.Mvvm):** High-confidence, first-party documentation
- **Phase 3 (MAUI Share API, FileSaver):** Official CommunityToolkit.Maui docs cover these

Phases that may benefit from targeted research during planning:
- **Phase 1 (import mapping):** The `Brim.json` format is the only source; verify that `rolledStats` vs `stats` interpretation is correct by examining additional Shadowdarklings export examples if available
- **Phase 2 (attack derivation):** The `attacks[]` array in `Brim.json` is pre-formatted strings (`"DAGGER: +3 (N), 1d4 (FIN)"`). Decide whether to parse these strings or have users re-enter attack data in a structured form. This is marked HIGH complexity in FEATURES.md and needs a concrete decision before building the Stats tab attack display.
- **Phase 2 (bonus toggle UX):** No existing app reference for this exact UX pattern. Consider a short design spike on the `StatBonusDetailPage` toggle interaction before committing to the implementation.

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | MEDIUM | Core choices (CommunityToolkit.Mvvm, System.Text.Json, FileSaver) are HIGH confidence. Version numbers require NuGet verification — all from training data cutoff Aug 2025. .NET 10 LTS status needs verification. |
| Features | MEDIUM | Table stakes derived from PROJECT.md (HIGH) and Brim.json data model analysis (HIGH). Shadowdark-specific rules (slot math, coin slots, attack derivation) from training data — verify against rulebook PDF before implementation. |
| Architecture | HIGH | Shell TabBar, singleton CharacterViewModel, MVVM with CommunityToolkit, file I/O patterns all from official Microsoft docs. Bonus toggle modeling is MEDIUM — derived from Brim.json structure, no existing app to reference. |
| Pitfalls | MEDIUM-HIGH | Critical pitfalls (file picker divergence, MSIX VFS, computed property propagation, import coupling) verified against official docs. Pitfall 5 (over-engineering) assessed from domain experience and Brim.json simplicity. |

**Overall confidence:** MEDIUM-HIGH

### Gaps to Address

- **.NET version:** Verify whether .NET 10 has shipped as LTS (due Nov 2025; project date is March 2026). If so, evaluate upgrading from the .NET 9 baseline before project creation.
- **Attack string parsing:** `Brim.json`'s `attacks[]` array contains pre-formatted strings like `"DAGGER: +3 (N), 1d4 (FIN)"`. The app needs a concrete decision: parse these strings into structured data during import, or require users to enter attacks in a structured form. This affects the `GearItem` model design and must be resolved in Phase 1.
- **Shadowdark rulebook accuracy:** Coin slot rule (1 per 20), gear slots = STR score, bulky = 2 slots, class attack bonus values — all from training data. Verify against the Shadowdark RPG PDF before building derived calculations.
- **Bonus toggle UX:** No app reference exists for the toggle-per-bonus-source pattern. A short design spike during Phase 2 planning is recommended before committing to the `StatBonusDetailPage` interaction model.
- **Windows distribution:** The MSIX vs. unpackaged decision (Pitfall 3) affects the file I/O strategy. Decide before Phase 1 file I/O implementation whether the Windows target is App Store / sideloaded MSIX or a plain installer.

---

## Sources

### Primary (HIGH confidence)
- `examples/Brim.json` (project file) — Shadowdarklings.net export format; ground truth for data model shape, bonus structure, gear, magic items, levels, attacks, ledger
- `PROJECT.md` (project file) — explicit requirements, scope, anti-features, platform targets
- [.NET MAUI MVVM Fundamentals](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/mvvm) — official Microsoft docs
- [Enterprise App Patterns: MVVM](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm) — Microsoft architecture guide
- [CommunityToolkit.Mvvm Introduction](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) — official CommunityToolkit docs
- [Shell Navigation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation) — official .NET MAUI docs
- [File System Helpers](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers) — official .NET MAUI docs
- [FilePicker in .NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker) — official docs
- [FileSaver API](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/essentials/file-saver) — CommunityToolkit.Maui docs
- [CommunityToolkit.Mvvm ObservableProperty + NotifyPropertyChangedFor](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty) — official docs

### Secondary (MEDIUM confidence)
- Shadowdark RPG domain knowledge (training data, cutoff Aug 2025) — slot rules, coin math, attack derivation, talent table system, HP per level — verify against rulebook PDF
- TTRPG character sheet app patterns (Roll20, DnDBeyond, Pathbuilder) — table stakes derivation; applied to OSR context

### Tertiary (LOW confidence)
- NuGet version numbers for CommunityToolkit.Mvvm and CommunityToolkit.Maui — from training data only; must be verified via `dotnet add package` before project creation
- .NET 10 LTS status — expected Nov 2025; not confirmed in training data; verify before project setup

---
*Research completed: 2026-03-08*
*Ready for roadmap: yes*
