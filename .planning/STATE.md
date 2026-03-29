---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: Gear & Stats Polish
status: Milestone complete
last_updated: "2026-03-29T15:07:15.447Z"
progress:
  total_phases: 2
  completed_phases: 2
  total_plans: 4
  completed_plans: 4
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-22)

**Core value:** A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.
**Current focus:** Planning next milestone (v1.2 Gear & Stats Polish)

## Current Position

Phase: 07
Plan: Not started
Last session stopped at: Completed 07-01-PLAN.md

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Key context carried forward:

- GearItemSource enum is canonical discriminator for gear vs magic item split on save
- MacFilePickerHelper (commit b2d9977): workaround for MAUI FilePicker null on macOS 15 Sequoia — needs re-test for iOS native
- BuildCharacterFromViewModel() is the canonical save snapshot builder — backing field is stale after edits
- Gear slots = STR score total; coin encumbrance = ceiling(coins/100) with first 100 free per type
- [Phase 06-gear-stats-polish]: Used Close() not CloseAsync() in GearItemPopup to match existing AttackPopup pattern
- [Phase 06-gear-stats-polish]: Free Carry frame always visible — no converter needed, empty BindableLayout renders nothing
- [Phase 07-maui-isfreecarry-fix]: MAUI-local shadow types (CS0436): Core changes must be manually propagated to MAUI-local equivalents

### Decisions

- Rolled12ChosenTalentDesc labeled with "(chosen)" suffix to disambiguate from rolled talents in Talents field

### Pending Todos

- None — phase 07 complete (07-01 MAUI IsFreeCarry fix). All plans in all phases complete.

### Blockers/Concerns

None — v1.1 shipped cleanly. Tech debt documented in audit.
