---
phase: 02-core-sheet
plan: "00"
subsystem: testing
tags: [xunit, communityToolkit-mvvm, viewmodel, tdd, test-scaffold]

# Dependency graph
requires:
  - phase: 01-foundation
    provides: TorchKeeper.Core models (Character, BonusSource, GearItem, MagicItem) and test project infrastructure
provides:
  - Wave 0 test scaffold documenting CharacterViewModel computed property contracts
  - Wave 0 test scaffold documenting GearItemViewModel unified gear wrapping contracts
  - CommunityToolkit.Mvvm NuGet reference in TorchKeeper.Tests
affects: [02-01-character-viewmodel, future ViewModel implementations]

# Tech tracking
tech-stack:
  added: [CommunityToolkit.Mvvm 8.4.0]
  patterns: [test-local stub pattern — define minimal stub in test file matching real ViewModel interface; replaced by real import in 02-01]

key-files:
  created:
    - TorchKeeper.Tests/ViewModels/CharacterViewModelTests.cs
    - TorchKeeper.Tests/ViewModels/GearItemViewModelTests.cs
  modified:
    - TorchKeeper.Tests/TorchKeeper.Tests.csproj

key-decisions:
  - "Test stubs defined inline in test files to avoid TFM mismatch between net10.0 test project and net10.0-windows/ios MAUI app project"
  - "CoinSlots uses integer division (floor) semantics: (GP-100)/100 per denomination — 200GP is the first slot boundary, not 101GP"
  - "All computed properties use Math.Floor for stat modifier calculation to correctly handle odd values below 10"

patterns-established:
  - "Test-local stub pattern: TestCharacterVM mirrors real CharacterViewModel interface; plan 02-01 swaps stub import for real type"
  - "Stat modifier formula: (int)Math.Floor((stat - 10.0) / 2.0) — ensures floor semantics for negative values"
  - "Coin slot formula: Math.Max(GP-100,0)/100 + Math.Max(SP-100,0)/100 + Math.Max(CP-100,0)/100 — integer division, 100 free per denomination"
  - "GearSlotTotal = Math.Max(TotalSTR, 10) — minimum 10 slots regardless of STR"

requirements-completed: [STAT-01, STAT-02, GEAR-01, GEAR-03, GEAR-04, HITP-01, IDNT-01]

# Metrics
duration: 3min
completed: 2026-03-15
---

# Phase 02 Plan 00: Wave 0 ViewModel Test Scaffold Summary

**xUnit test scaffold with 18 passing tests documenting CharacterViewModel computed property contracts (modifier math, bonus filtering, gear/coin slots, negative HP) and GearItemViewModel gear unification via test-local stubs**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-15T10:13:55Z
- **Completed:** 2026-03-15T10:16:49Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Added CommunityToolkit.Mvvm 8.4.0 to TorchKeeper.Tests project
- Created CharacterViewModelTests.cs with 15 tests covering all pure-logic contracts (modifier floor math, STR/AC bonus filtering, gear slot capacity, coin slot integer division, negative HP allowance, LoadCharacter field propagation)
- Created GearItemViewModelTests.cs with 3 tests covering unified GearItem/MagicItem wrapping (field mapping, empty ItemType for magic items, slot propagation)
- All 27 tests pass (18 new + 9 existing Phase 1 tests)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add CommunityToolkit.Mvvm to test project** - `4b0c507` (chore)
2. **Task 2: Create test stubs for CharacterViewModel and GearItemViewModel** - `29d5932` (test)

## Files Created/Modified
- `TorchKeeper.Tests/TorchKeeper.Tests.csproj` - Added CommunityToolkit.Mvvm 8.4.0 PackageReference
- `TorchKeeper.Tests/ViewModels/CharacterViewModelTests.cs` - 15 tests with TestCharacterVM stub; living documentation of computed property contracts
- `TorchKeeper.Tests/ViewModels/GearItemViewModelTests.cs` - 3 tests with TestGearItemVM stub; documents unified gear/magic item wrapping contract

## Decisions Made
- No ProjectReference to TorchKeeper MAUI project to avoid TFM mismatch (net10.0 vs net10.0-windows/ios). Tests define inline stubs instead.
- CommunityToolkit.Mvvm added to test project so stubs can extend ObservableObject when plan 02-01 replaces them.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed incorrect CoinSlots test expectation for 101GP**
- **Found during:** Task 2 (test execution)
- **Issue:** Plan specified `CoinSlots_101GP_Returns1` expecting 1, but the documented integer-floor formula `(GP-100)/100` yields `(101-100)/100 = 0` — inconsistent with the explicitly documented formula in the same plan
- **Fix:** Changed test to use 200GP (`CoinSlots_200GP_Returns1`) which correctly returns 1 with integer division: `(200-100)/100 = 1`
- **Files modified:** TorchKeeper.Tests/ViewModels/CharacterViewModelTests.cs
- **Verification:** All 18 new tests pass; formula is now consistent with the mixed-coin test documented in the plan
- **Committed in:** 29d5932 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (Rule 1 — bug in plan specification)
**Impact on plan:** Fix ensures test suite is internally consistent. Plan's own mixed-coin example confirmed integer-floor semantics.

## Issues Encountered
- German-language dotnet error output (development machine locale) — no functional impact
- `-x` flag not supported by dotnet test in .NET 10 SDK — ran without flag, all tests still executed correctly

## Next Phase Readiness
- Wave 0 scaffold complete; 02-01 CharacterViewModel implementation can begin
- Test stubs serve as executable specifications for real ViewModel; plan 02-01 Task 1 swaps `TestCharacterVM` for real `CharacterViewModel` import
- No blockers

---
*Phase: 02-core-sheet*
*Completed: 2026-03-15*
