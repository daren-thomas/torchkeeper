---
phase: 02-core-sheet
plan: "03"
subsystem: ui
tags: [maui, xaml, gear, inventory, currency, popup, collectionview]

# Dependency graph
requires:
  - phase: 02-core-sheet-01
    provides: CharacterViewModel with GearItems, GearSlotsUsed, GearSlotTotal, CoinSlots, GP/SP/CP; GearItemViewModel with GearItem/MagicItem constructors
provides:
  - GearPage.xaml/cs: inventory tab with slot header, unified gear list, add/edit via modal, currency section
  - GearItemPopup.xaml/cs: reusable modal for adding and editing gear items
  - GearItemViewModel third constructor for new user-created items
affects: [02-04, 02-05, app-shell, maui-navigation]

# Tech tracking
tech-stack:
  added: []
  patterns: [ShowPopupAsync modal pattern for add/edit workflows, CollectionView with TapGestureRecognizer for list item editing]

key-files:
  created:
    - SdCharacterSheet/Views/GearPage.xaml
    - SdCharacterSheet/Views/GearPage.xaml.cs
    - SdCharacterSheet/Views/Popups/GearItemPopup.xaml
    - SdCharacterSheet/Views/Popups/GearItemPopup.xaml.cs
  modified:
    - SdCharacterSheet/ViewModels/GearItemViewModel.cs

key-decisions:
  - "GearItemPopup uses two constructor overloads (edit vs add) rather than a single constructor with nullable parameter — cleaner call sites in GearPage.xaml.cs"
  - "Coin slots display shown as secondary label below the main slot counter for transparency without cluttering the header"
  - "CollectionView used for gear list (not BindableLayout) as no expand/collapse needed — iOS Expander/CollectionView bug does not apply here"

patterns-established:
  - "ShowPopupAsync modal pattern: code-behind calls this.ShowPopupAsync(new XxxPopup(_vm, item)) for tap-to-edit and this.ShowPopupAsync(new XxxPopup(_vm)) for add"
  - "Popup constructors accept CharacterViewModel + optional item; mutations happen in the popup via direct property assignment and collection Add/Remove"

requirements-completed: [GEAR-01, GEAR-02, GEAR-03, GEAR-04, CURR-01]

# Metrics
duration: 2min
completed: 2026-03-15
---

# Phase 02 Plan 03: GearPage Summary

**GearPage with live slot counter (GearSlotsUsed/GearSlotTotal), tap-to-edit gear list via GearItemPopup modal, and GP/SP/CP currency section**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-15T10:26:25Z
- **Completed:** 2026-03-15T10:27:42Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Slot header displaying live GearSlotsUsed / GearSlotTotal with coin slots sub-label
- CollectionView gear list with tap-to-edit and Add Item button wired to GearItemPopup
- Currency section with GP/SP/CP two-way bound to CharacterViewModel
- GearItemPopup with Name/Slots/Type/Note fields, Save and Delete handlers
- GearItemViewModel third constructor enabling new user-created items

## Task Commits

Each task was committed atomically:

1. **Task 1: Create GearItemPopup for add/edit modal** - `846be65` (feat)
2. **Task 2: Build GearPage with slot header, gear list, and currency section** - `813c357` (feat)

**Plan metadata:** (docs commit pending)

## Files Created/Modified
- `SdCharacterSheet/ViewModels/GearItemViewModel.cs` - Added third plain-values constructor for user-created items
- `SdCharacterSheet/Views/Popups/GearItemPopup.xaml` - Modal UI with Name, Slots, Type, Note fields and Save/Delete buttons
- `SdCharacterSheet/Views/Popups/GearItemPopup.xaml.cs` - Two constructors (edit/add), OnSave updates existing or adds new, OnDelete removes from GearItems
- `SdCharacterSheet/Views/GearPage.xaml` - Slot header Frame, CollectionView gear list, Currency Grid with GP/SP/CP entries
- `SdCharacterSheet/Views/GearPage.xaml.cs` - CharacterViewModel injection, OnItemTapped and OnAddItemClicked handlers via ShowPopupAsync

## Decisions Made
- GearItemPopup uses two constructor overloads (edit vs add) rather than a nullable parameter — cleaner call sites
- Coin slots shown as secondary label below main slot counter for transparency
- CollectionView used for gear list (BindableLayout reserved for Stats page iOS workaround only)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- MAUI project build cannot be verified in this environment (no MAUI workload installed — pre-existing environment constraint per Phase 1 decisions). Core project builds cleanly. XAML correctness confirmed by review against plan specification.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- GearPage complete; ready for integration into AppShell tab navigation (plan 02-05 or app shell wiring)
- GearItemPopup establishes the modal add/edit pattern used consistently across the app
- Attack display format decision still pending (see blockers in STATE.md)

## Self-Check: PASSED

All files present and commits verified:
- FOUND: SdCharacterSheet/Views/GearPage.xaml
- FOUND: SdCharacterSheet/Views/GearPage.xaml.cs
- FOUND: SdCharacterSheet/Views/Popups/GearItemPopup.xaml
- FOUND: SdCharacterSheet/Views/Popups/GearItemPopup.xaml.cs
- FOUND: 846be65 (Task 1 commit)
- FOUND: 813c357 (Task 2 commit)
- FOUND: c577c9b (docs/metadata commit)

---
*Phase: 02-core-sheet*
*Completed: 2026-03-15*
