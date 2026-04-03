---
gsd_state_version: 1.0
milestone: v1.2
milestone_name: Gear & Stats Polish
status: Archived — planning next milestone
last_updated: "2026-04-03T00:00:00.000Z"
progress:
  total_phases: 2
  completed_phases: 2
  total_plans: 4
  completed_plans: 4
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-04-03)

**Core value:** A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.
**Current focus:** v1.2 archived — ready for next milestone planning

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Key context carried forward:

- GearItemSource enum is canonical discriminator for gear vs magic item split on save
- MacFilePickerHelper (commit b2d9977): workaround for MAUI FilePicker null on macOS 15 Sequoia — needs re-test for iOS native
- BuildCharacterFromViewModel() is the canonical save snapshot builder — backing field is stale after edits
- Gear slots = STR score total; coin encumbrance = ceiling(coins/100) with first 100 free per type
- MAUI-local shadow types (CS0436 pattern): Core changes must be manually propagated to TorchKeeper/Models/ and TorchKeeper/DTOs/
- Free Carry frame always visible — no converter needed, empty BindableLayout renders nothing

### Blockers/Concerns

None — v1.2 archived cleanly. Tech debt documented in PROJECT.md.
