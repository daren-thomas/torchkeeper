---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: Gear & Stats Polish
status: in_progress
last_updated: "2026-03-28T14:37:00.000Z"
progress:
  total_phases: 1
  completed_phases: 0
  total_plans: 3
  completed_plans: 1
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-22)

**Core value:** A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.
**Current focus:** Planning next milestone (v1.2 Gear & Stats Polish)

## Current Position

Phase: 6 — Gear & Stats Polish
Plan: 06-03 complete (stat drill-down base label + Shadowdarklings talents import)
Last session stopped at: Completed 06-03-PLAN.md

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Key context carried forward:

- GearItemSource enum is canonical discriminator for gear vs magic item split on save
- MacFilePickerHelper (commit b2d9977): workaround for MAUI FilePicker null on macOS 15 Sequoia — needs re-test for iOS native
- BuildCharacterFromViewModel() is the canonical save snapshot builder — backing field is stale after edits
- Gear slots = STR score total; coin encumbrance = ceiling(coins/100) with first 100 free per type

### Decisions

- Rolled12ChosenTalentDesc labeled with "(chosen)" suffix to disambiguate from rolled talents in Talents field

### Pending Todos

- Plans 06-01 and 06-02 also running in parallel (gear slots free-carry, stat drill-down UI polish)

### Blockers/Concerns

None — v1.1 shipped cleanly. Tech debt documented in audit.
