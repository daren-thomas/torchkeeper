---
phase: 02-core-sheet
plan: "01"
subsystem: ui
tags: [communityToolkit-mvvm, viewmodel, observable-property, maui, data-binding]

# Dependency graph
requires:
  - phase: 02-core-sheet
    provides: Wave 0 test stubs (CharacterViewModelTests, GearItemViewModelTests) documenting computed property contracts; CommunityToolkit.Mvvm reference in test project
  - phase: 01-foundation
    provides: TorchKeeper.Core models (Character, BonusSource, GearItem, MagicItem)
provides:
  - Full CharacterViewModel with all 6 stats, computed totals/modifiers, gear/coin slots, LoadCharacter with per-stat write-back lambdas
  - StatRowViewModel with editable BaseStat, TotalScore, ModifierDisplay, BonusSources, ToggleExpandCommand
  - GearItemViewModel unifying GearItem and MagicItem for the gear list
affects: [02-02-stats-tab, 02-03-gear-tab, 02-04-identity-tab, all future phase 2 XAML plans]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "[ObservableProperty] with [NotifyPropertyChangedFor] chains for computed properties"
    - "Action<int> callback delegate pattern for StatRowViewModel → CharacterViewModel write-back"
    - "CollectionChanged + per-item PropertyChanged subscription for GearSlotsUsed reactivity"
    - "LoadCharacter sets backing fields directly (bypasses setter equality check) then calls OnPropertyChanged(string.Empty)"

key-files:
  created:
    - TorchKeeper/ViewModels/StatRowViewModel.cs
    - TorchKeeper/ViewModels/GearItemViewModel.cs
  modified:
    - TorchKeeper/ViewModels/CharacterViewModel.cs

key-decisions:
  - "StatRowViewModel receives per-stat write-back Action<int> in constructor — edits to BaseStat propagate to CharacterViewModel.BaseSTR via delegate, not direct reference"
  - "CharacterViewModel.GearSlotsUsed listens to individual GearItemViewModel.PropertyChanged (Slots) via CollectionChanged handler managing subscriptions, not just collection adds/removes"
  - "CommunityToolkit naming: xP backing field generates XP property; gP→GP, sP→SP, cP→CP — source generator capitalizes first letter"

patterns-established:
  - "Callback pattern: StatRowViewModel(Action<int> onBaseStatChanged) — partial void OnBaseStatChanged calls _onBaseStatChanged(value)"
  - "Backing field direct assignment in LoadCharacter: `baseSTR = character.BaseSTR` bypasses setter equality check so OnPropertyChanged(string.Empty) fires all"
  - "GearItemSource enum distinguishes origin for save-back: { Gear, Magic }"
  - "floor division for Shadowdark modifiers: (int)Math.Floor((score - 10.0) / 2.0) — ensures correct negative values"

requirements-completed: [STAT-01, STAT-02, GEAR-01, GEAR-02, GEAR-03, GEAR-04, HITP-01, HITP-02, IDNT-01, IDNT-02, CURR-01, ATCK-01, NOTE-01]

# Metrics
duration: 2min
completed: 2026-03-15
---

# Phase 02 Plan 01: CharacterViewModel Expansion Summary

**Full observable CharacterViewModel with 6 stats, computed totals/modifiers, gear/coin slots, and LoadCharacter with per-stat write-back lambdas; plus StatRowViewModel and GearItemViewModel as supporting types**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-15T10:18:24Z
- **Completed:** 2026-03-15T10:20:45Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Created StatRowViewModel with per-stat observable BaseStat, Action<int> write-back callback, computed TotalScore and ModifierDisplay, filterable BonusSources ObservableCollection, and ToggleExpandCommand
- Created GearItemViewModel unifying GearItem and MagicItem into a single observable wrapper (ItemType="" for magic items); GearItemSource enum for save-back origin tracking
- Replaced CharacterViewModel stub with full implementation: all 6 stats with [NotifyPropertyChangedFor] chains, computed TotalX/ModX properties, GearSlotTotal/CoinSlots/GearSlotsUsed, complete LoadCharacter with StatRows/GearItems/Attacks rebuild
- All 27 tests remain green (stubs continue to verify computed property contracts)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create StatRowViewModel and GearItemViewModel** - `4d214b2` (feat)
2. **Task 2: Expand CharacterViewModel with all observable properties and computed properties** - `5f77017` (feat)

## Files Created/Modified
- `TorchKeeper/ViewModels/StatRowViewModel.cs` - Per-stat row VM with editable BaseStat, TotalScore, ModifierDisplay, BonusSources, ToggleExpandCommand, and Action<int> write-back callback
- `TorchKeeper/ViewModels/GearItemViewModel.cs` - Unified GearItem/MagicItem wrapper with four observable fields and GearItemSource enum
- `TorchKeeper/ViewModels/CharacterViewModel.cs` - Full observable ViewModel: all identity/stat/HP/currency [ObservableProperty] fields, computed properties with [NotifyPropertyChangedFor] chains, LoadCharacter, per-item PropertyChanged propagation for GearSlotsUsed

## Decisions Made
- `StatRowViewModel` uses `Action<int>` callback constructor parameter rather than a direct reference to `CharacterViewModel` — keeps the row VM decoupled and testable
- `CharacterViewModel.LoadCharacter` sets backing fields directly (e.g. `baseSTR = character.BaseSTR`) rather than using public setters, then fires `OnPropertyChanged(string.Empty)` to refresh all bindings in one shot
- `[NotifyPropertyChangedFor(nameof(TotalScore))]` and `[NotifyPropertyChangedFor(nameof(ModifierDisplay))]` declared on `baseStat` field; `partial void OnBaseStatChanged` only calls the callback delegate (no redundant manual notifications)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- MAUI project build (`dotnet build TorchKeeper/`) cannot be run in this environment — MAUI NuGet packages not cached locally and network access is restricted (same constraint as Phase 1). The test project (net10.0 class library) builds and runs correctly, confirming all logic is sound.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All three ViewModel files are complete and provide all binding targets needed by Phase 2 XAML plans (02-02 through 02-04)
- CharacterViewModel is the singleton data layer; all tab pages can bind to it
- No blockers

---
*Phase: 02-core-sheet*
*Completed: 2026-03-15*
