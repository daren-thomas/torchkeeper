---
phase: 6
plan: 3
subsystem: stats-drill-down, shadowdarklings-import
tags: [ui, import, talents, stats, xunit]
dependency_graph:
  requires: []
  provides: [stat-base-label-in-drill-down, talents-from-levels-import]
  affects: [ShadowdarklingsImportService, SheetPage.xaml, ShadowdarklingsImportServiceTests]
tech_stack:
  added: []
  patterns: [VerticalStackLayout nesting for mixed data-context bindings, SelectMany for flattened per-level talent entries]
key_files:
  created: []
  modified:
    - TorchKeeper/Views/SheetPage.xaml
    - TorchKeeper.Core/DTOs/ShadowdarklingsJson.cs
    - TorchKeeper.Core/Services/ShadowdarklingsImportService.cs
    - TorchKeeper.Tests/Services/ShadowdarklingsImportServiceTests.cs
decisions:
  - Include Rolled12ChosenTalentDesc with "(chosen)" label when non-empty — label disambiguates from rolled talents
metrics:
  duration: ~8 minutes
  completed: 2026-03-28
  tasks: 3
  files_changed: 4
  tests_added: 3
  tests_total: 57
---

# Phase 6 Plan 3: Stat Drill-Down + Talents Import Summary

**One-liner:** Base stat label in expanded stat panel using `StatRowViewModel.BaseStat`; Talents populated from `levels[].talentRolledDesc` with `Lv{N}: {desc}` format during Shadowdarklings import.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add "Base: N" label to SheetPage.xaml expanded stat panel | b005be5 | TorchKeeper/Views/SheetPage.xaml |
| 2 | Add Levels and SdLevelEntry to ShadowdarklingsJson | a49b709 | TorchKeeper.Core/DTOs/ShadowdarklingsJson.cs |
| 3 | Populate Talents in ShadowdarklingsImportService + tests | 31fe0a2 | TorchKeeper.Core/Services/ShadowdarklingsImportService.cs, TorchKeeper.Tests/Services/ShadowdarklingsImportServiceTests.cs |

## What Was Built

**Task 1 — Stat Drill-Down Base Label (D-10, D-11, D-12)**

Wrapped the existing `BindableLayout`-driven `VerticalStackLayout` in an outer `VerticalStackLayout` that carries `IsVisible="{Binding IsExpanded}"`. The outer layout now has `Spacing="4"` and contains:
1. A `Label` bound to `BaseStat` with `StringFormat='Base: {0}'` — always first
2. The inner `VerticalStackLayout` with `BindableLayout.ItemsSource="{Binding BonusSources}"` — bonus sources below

`StatRowViewModel.BaseStat` was already an `[ObservableProperty]` — no ViewModel changes needed. The XAML data context at the outer layout level is `StatRowViewModel`, so `BaseStat` binding is correct.

**Task 2 — ShadowdarklingsJson.Levels (D-15)**

Added `public List<SdLevelEntry>? Levels { get; set; }` to `ShadowdarklingsJson`. Added `SdLevelEntry` class with:
- `Level` (int)
- `TalentRolledDesc` (string, defaults to "")
- `Rolled12ChosenTalentDesc` (string, defaults to "")

`PropertyNameCaseInsensitive = true` already set in `ImportOptions` handles the camelCase/PascalCase mix in Shadowdarklings JSON.

**Task 3 — Talents Population (D-13)**

Added talent-building logic to `ShadowdarklingsImportService.ImportAsync`:
- Iterates `sdJson.Levels` ordered by level
- Per level: adds `"Lv{N}: {desc}"` if `TalentRolledDesc` is non-empty
- Per level: adds `"Lv{N} (chosen): {desc}"` if `Rolled12ChosenTalentDesc` is non-empty
- Joins entries with `\n`, assigns to `Character.Talents`
- Three xUnit tests cover: basic population, empty desc skip, and chosen talent inclusion

## Validation

- `TorchKeeper.Core` build: passed (0 errors, 0 warnings)
- `TorchKeeper.Tests` full suite: 57/57 passed (3 new tests added)
- MAUI project build: platform toolchain (actool/xcrun Xcode plugin errors) prevents full compilation on this machine — pre-existing infrastructure issue unrelated to XAML changes

## Deviations from Plan

None — plan executed exactly as written. The `Rolled12ChosenTalentDesc` inclusion with `"(chosen)"` label was specified as "Claude's discretion" in the plan and matches the implemented test expectations.

## Known Stubs

None — all fields are wired to real data sources.

## Self-Check: PASSED

Files exist:
- TorchKeeper/Views/SheetPage.xaml: modified (Base label added at line 193)
- TorchKeeper.Core/DTOs/ShadowdarklingsJson.cs: modified (Levels + SdLevelEntry added)
- TorchKeeper.Core/Services/ShadowdarklingsImportService.cs: modified (Talents assignment)
- TorchKeeper.Tests/Services/ShadowdarklingsImportServiceTests.cs: modified (3 tests added)

Commits verified:
- a49b709: feat(06-03): add SdLevelEntry and Levels property to ShadowdarklingsJson
- b005be5: feat(06-03): add Base stat label to expanded stat drill-down panel
- 31fe0a2: feat(06-03): populate Talents from levels[] during Shadowdarklings import
