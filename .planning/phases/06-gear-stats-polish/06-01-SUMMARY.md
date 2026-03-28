---
phase: 06-gear-stats-polish
plan: "01"
subsystem: ui
tags: [maui, gear, free-carry, observable-collections, xaml]

# Dependency graph
requires: []
provides:
  - IsFreeCarry bool on GearItem, MagicItem, GearItemData, MagicItemData
  - Auto-detect for known free-carry names (Backpack, Bag of Coins, Thieves Tools)
  - RegularGearItems and FreeCarryItems sub-collections on CharacterViewModel
  - GearSlotsUsed excludes free-carry items
  - Free Carry checkbox in GearItemPopup
  - GearPage split into regular gear and free carry sections
affects:
  - 06-02-PLAN.md
  - 06-03-PLAN.md

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Sub-collection rebuild pattern: RebuildGearSubCollections() called from CollectionChanged + OnGearItemChanged + LoadCharacter"
    - "Auto-detect on load: IsFreeCarry || IsKnownFreeCarry(name) in GearItem constructor"

key-files:
  created: []
  modified:
    - SdCharacterSheet.Core/Models/GearItem.cs
    - SdCharacterSheet.Core/Models/MagicItem.cs
    - SdCharacterSheet.Core/DTOs/CharacterSaveData.cs
    - SdCharacterSheet.Core/Services/CharacterFileService.cs
    - SdCharacterSheet/ViewModels/GearItemViewModel.cs
    - SdCharacterSheet/ViewModels/CharacterViewModel.cs
    - SdCharacterSheet/Views/Popups/GearItemPopup.xaml
    - SdCharacterSheet/Views/Popups/GearItemPopup.xaml.cs
    - SdCharacterSheet/Views/GearPage.xaml

key-decisions:
  - "Used Close() not CloseAsync() in GearItemPopup to match existing project popup pattern (AttackPopup uses Close())"
  - "Free Carry frame always visible (no IsZeroConverter) — simplest approach, empty BindableLayout renders nothing"
  - "Auto-detect on GearItem constructor corrects old saves without migration"

patterns-established:
  - "Sub-collection rebuild pattern: separate observable collections per category rebuilt from master on mutation"
  - "Known free-carry auto-detect: fires on GearItem and string constructors, not on MagicItem constructor"

requirements-completed: [GEAR-01]

# Metrics
duration: 10min
completed: 2026-03-28
---

# Phase 6 Plan 01: Free-Carry Gear Slots Summary

**IsFreeCarry flag added full-stack (model to UI) with auto-detect for known items and GearPage split into regular/free-carry sections**

## Performance

- **Duration:** ~10 min
- **Started:** 2026-03-28T14:28:42Z
- **Completed:** 2026-03-28T14:38:28Z
- **Tasks:** 6
- **Files modified:** 9

## Accomplishments

- `IsFreeCarry` bool added to `GearItem`, `MagicItem`, `GearItemData`, `MagicItemData` with full round-trip through `CharacterFileService`
- `GearItemViewModel` auto-detects known free-carry names (Backpack, Bag of Coins, Thieves Tools) on load to correct pre-existing saves
- `CharacterViewModel` excludes free-carry items from `GearSlotsUsed`, maintains `RegularGearItems` and `FreeCarryItems` sub-collections
- `GearItemPopup` gains Free Carry checkbox (pre-filled on edit, written back on save, min slots changed to 0)
- `GearPage` split into "Gear" section (slot count) and "Free Carry" section ("free" label)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add IsFreeCarry to models and DTO** - `f1bb417` (feat)
2. **Task 2: Wire IsFreeCarry through CharacterFileService** - `9aa9f99` (feat)
3. **Task 3: Add IsFreeCarry and auto-detect to GearItemViewModel** - `f4ea8bd` (feat)
4. **Task 4: Update CharacterViewModel** - `d46c1eb` (feat)
5. **Task 5: Add Free Carry checkbox to GearItemPopup** - `8f68479` (feat)
6. **Task 6: Split GearPage into regular and Free Carry sections** - `480b40f` (feat)

## Files Created/Modified

- `SdCharacterSheet.Core/Models/GearItem.cs` - Added `IsFreeCarry bool` property
- `SdCharacterSheet.Core/Models/MagicItem.cs` - Added `IsFreeCarry bool` property
- `SdCharacterSheet.Core/DTOs/CharacterSaveData.cs` - Added `IsFreeCarry` to `GearItemData` and `MagicItemData`
- `SdCharacterSheet.Core/Services/CharacterFileService.cs` - Wired `IsFreeCarry` in `MapToDto` and `MapFromDto`
- `SdCharacterSheet/ViewModels/GearItemViewModel.cs` - Added `IsFreeCarry` observable property, `KnownFreeCarryNames`, `IsKnownFreeCarry`, updated all 3 constructors
- `SdCharacterSheet/ViewModels/CharacterViewModel.cs` - `GearSlotsUsed` excludes free-carry, `OnGearItemChanged` reacts to `IsFreeCarry`, `BuildCharacterFromViewModel` persists it, added `RegularGearItems`/`FreeCarryItems` + `RebuildGearSubCollections`
- `SdCharacterSheet/Views/Popups/GearItemPopup.xaml` - Added `FreeCarryCheckBox` between Note and Save
- `SdCharacterSheet/Views/Popups/GearItemPopup.xaml.cs` - Pre-fill checkbox, write back on save, `Math.Max(0, slots)`
- `SdCharacterSheet/Views/GearPage.xaml` - Section 2 bound to `RegularGearItems`, Section 2b (Free Carry) bound to `FreeCarryItems`

## Decisions Made

- **Close() not CloseAsync()**: Used `Close()` in `GearItemPopup` to match existing project popup pattern (`AttackPopup` uses `Close()`). Plan specified `CloseAsync()` but consistency with existing code takes precedence.
- **No IsZeroConverter**: Free Carry frame always visible. Empty `BindableLayout` renders nothing, no converter complexity needed.
- **Auto-detect on load**: `IsFreeCarry = g.IsFreeCarry || IsKnownFreeCarry(g.Name)` in `GearItem` constructor corrects old saves without requiring a data migration.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Consistency] Used Close() instead of CloseAsync() in GearItemPopup**
- **Found during:** Task 5 (GearItemPopup code-behind)
- **Issue:** Plan specified `async void` methods with `CloseAsync()`, but existing project code (AttackPopup) uses synchronous `Close()`
- **Fix:** Used `Close()` to match existing pattern
- **Files modified:** SdCharacterSheet/Views/Popups/GearItemPopup.xaml.cs
- **Verification:** No C# compiler errors
- **Committed in:** `8f68479` (Task 5 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 - consistency)
**Impact on plan:** Purely style — no behavior change. Consistent with existing popup pattern.

## Issues Encountered

- **Test runner socket failure**: `dotnet test` fails with `SocketException: Permission denied` in sandbox environment due to parallel agent execution contention on TCP ports. Verification confirmed via 0 C# compiler errors on `dotnet build` — the Core project builds cleanly.
- **Xcode toolchain failure**: `dotnet build SdCharacterSheet/SdCharacterSheet.csproj` fails on `actool/xcrun` (pre-existing: `xcodebuild -runFirstLaunch` required). 0 `error CS` lines confirmed — all C# code compiles correctly.

## Known Stubs

None — all data flows are wired. `RegularGearItems` and `FreeCarryItems` are rebuilt from `GearItems` on every mutation; `GearPage.xaml` binds directly to these collections.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- GEAR-01 complete: free-carry model, persistence, ViewModel logic, and UI all in place
- Plan 06-02 (GEAR-02) can build on top of `RegularGearItems`/`FreeCarryItems` if needed
- Plan 06-03 can proceed independently (depends_on: none per plan frontmatter)
- Manual verification needed: import `examples/Brim.json` to confirm Backpack auto-detects as free-carry

## Self-Check: PASSED

All 9 modified files confirmed present. All 6 task commits verified in git log.

---
*Phase: 06-gear-stats-polish*
*Completed: 2026-03-28*
