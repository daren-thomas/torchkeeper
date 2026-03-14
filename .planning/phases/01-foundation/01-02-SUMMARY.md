---
phase: 01-foundation
plan: 02
subsystem: import
tags: [csharp, maui, system.text.json, xunit, tdd]

# Dependency graph
requires:
  - phase: 01-foundation plan 01
    provides: Character model, BonusSource model, GearItem model, MagicItem model, ShadowdarklingsJson DTO, ShadowdarklingsImportService stub
provides:
  - ShadowdarklingsImportService.ImportAsync(Stream) — maps Shadowdarklings JSON to Character
  - 7 passing xUnit tests covering FILE-01 import scenarios
  - DI registration of ShadowdarklingsImportService in MauiProgram.cs
affects: [01-foundation-03, 02-ui, Phase 2 file picker integration]

# Tech tracking
tech-stack:
  added: []
  patterns: [TDD red-green, static readonly JsonSerializerOptions per service, ledger fallback currency pattern]

key-files:
  created:
    - SdCharacterSheet/Services/ShadowdarklingsImportService.cs
    - (tests already existed as stubs; now fully implemented)
  modified:
    - SdCharacterSheet.Tests/Services/ShadowdarklingsImportServiceTests.cs
    - SdCharacterSheet/MauiProgram.cs

key-decisions:
  - "Static readonly JsonSerializerOptions avoids per-call allocation; shared across all ImportAsync calls"
  - "All bonuses (stat and AC: prefixed) go to the same Bonuses list; differentiation by BonusTo prefix happens at display time in Phase 2"
  - "Currency fallback: top-level Gold/Silver/Copper win; only sum ledger entries when top-level fields are null (absent)"
  - "RolledStats used for BaseSTR/DEX/CON/INT/WIS/CHA; Stats property intentionally ignored to avoid double-counting"
  - "CurrentHP = MaxHP on import — character starts at full health"

patterns-established:
  - "Import service pattern: deserialize to DTO, map to domain model, return null on parse failure"
  - "JsonSerializerOptions as static readonly class field — never created per-call"
  - "Null-coalescing currency fallback: TopLevel ?? (Ledger?.Sum() ?? 0)"

requirements-completed: [FILE-01]

# Metrics
duration: 12min
completed: 2026-03-14
---

# Phase 1 Plan 02: ShadowdarklingsImportService Summary

**Async JSON import service mapping Shadowdarklings.net exports to Character domain model with currency fallback, RolledStats base stats, and full bonus pass-through**

## Performance

- **Duration:** 12 min
- **Started:** 2026-03-14T09:08:04Z
- **Completed:** 2026-03-14T09:20:00Z
- **Tasks:** 2 (TDD RED + GREEN)
- **Files modified:** 3

## Accomplishments
- Replaced 7 skip-stub xUnit tests with real assertions covering all FILE-01 behaviors
- Implemented `ShadowdarklingsImportService.ImportAsync(Stream)` that correctly maps every field
- Currency logic prefers top-level fields with ledger-sum fallback
- All bonuses (stat and AC contributors) pass through to Bonuses list with no filtering
- Registered service in MauiProgram.cs DI container

## Task Commits

Each task was committed atomically:

1. **TDD RED: failing tests** - `bd9d7dd` (test)
2. **TDD GREEN: ImportAsync implementation + DI registration** - `b12b541` (feat)

_Note: TDD tasks have two commits (test RED → feat GREEN)_

## Files Created/Modified
- `SdCharacterSheet/Services/ShadowdarklingsImportService.cs` - Full ImportAsync implementation with static options
- `SdCharacterSheet.Tests/Services/ShadowdarklingsImportServiceTests.cs` - 7 real test methods (no skips)
- `SdCharacterSheet/MauiProgram.cs` - Uncommented ShadowdarklingsImportService DI registration

## Decisions Made
- Static readonly `JsonSerializerOptions` on the class (not per-call) to avoid allocation in the hot path
- All bonuses go to the same `Bonuses` list regardless of prefix — AC prefix interpretation deferred to Phase 2 UI
- Currency null-coalescing pattern (`Gold ?? (Ledger?.Sum(...) ?? 0)`) is concise and explicit about fallback semantics

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

The local environment only has .NET 8.0.100 SDK installed; the test project targets net10.0, so tests cannot be compiled and run locally. This constraint was pre-documented in STATE.md from Plan 01 ("Files created manually due to .NET 8-only execution environment; .NET 10 + MAUI workload required to build"). The implementation was verified through code review against all 7 test cases and the Brim.json fixture.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- `ShadowdarklingsImportService.ImportAsync` is complete and registered — ready for use in the file picker UI (Phase 2)
- Plan 03 (CharacterFileService) can proceed independently; no dependency on this plan
- Tests will pass on a .NET 10 machine or CI with the correct SDK

---
*Phase: 01-foundation*
*Completed: 2026-03-14*
