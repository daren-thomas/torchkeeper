# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-08)

**Core value:** A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.
**Current focus:** Phase 1 — Foundation

## Current Position

Phase: 1 of 3 (Foundation)
Plan: 0 of TBD in current phase
Status: Ready to plan
Last activity: 2026-03-08 — Roadmap created; phases derived from requirements

Progress: [░░░░░░░░░░] 0%

## Performance Metrics

**Velocity:**
- Total plans completed: 0
- Average duration: -
- Total execution time: 0 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| - | - | - | - |

**Recent Trend:**
- Last 5 plans: none yet
- Trend: -

*Updated after each plan completion*

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Pre-Phase 1]: Native .sdchar format uses a separate CharacterSaveData DTO with version field — never serialize the ViewModel directly
- [Pre-Phase 1]: Shadowdarklings import reads rolledStats (not stats) to avoid double-counting bonuses already included in final stat values
- [Pre-Phase 1]: CharacterViewModel is a singleton shared across all tabs — gear slots depend on STR stat, so a change on the Stats tab must propagate to the Gear tab immediately

### Pending Todos

None yet.

### Blockers/Concerns

- [Phase 1]: .NET version (9 vs 10 LTS) must be decided before project creation — .NET 10 was expected Nov 2025, verify current LTS status
- [Phase 1]: MSIX vs. unpackaged decision for Windows target affects file I/O strategy — decide before implementing file picker
- [Phase 2]: Attack display format decision needed — Shadowdarklings exports pre-formatted attack strings; decide whether to parse them or require re-entry in a structured form

## Session Continuity

Last session: 2026-03-08
Stopped at: Roadmap written; STATE.md initialized; REQUIREMENTS.md traceability already present from requirements definition
Resume file: None
