---
gsd_state_version: 1.0
milestone: v1.1
milestone_name: File Management & Talents
status: unknown
last_updated: "2026-03-22T15:15:25.926Z"
progress:
  total_phases: 2
  completed_phases: 2
  total_plans: 3
  completed_plans: 3
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-21)

**Core value:** A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.
**Current focus:** Phase 05 — talents-editor

## Current Position

Phase: 05 (talents-editor) — EXECUTING
Plan: 1 of 1

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

Key context for v1.1:

- [Phase 04-file-menu]: MauiCharacterFileService resolved via cast from CharacterFileService DI key — concrete type needed for OpenAsync
- [Phase 04-file-menu]: BuildCharacterFromViewModel() builds from observable properties not backing field — backing field is stale after edits
- [Phase 04-file-menu]: GearItemSource enum is canonical discriminator for gear vs magic item split on save
- [Phase 04-file-menu]: MAUI FilePicker.Default.PickAsync() silently returns null on macOS 15 (Sequoia) — fixed by MacFilePickerHelper using ConnectedScenes + UIDocumentPickerViewController directly (commit b2d9977)
- [Phase 04-file-menu]: MacCatalyst Info.plist added to register com.sdcharactersheet.sdchar UTType (commit 0fc857c)
- [Phase 05-talents-editor]: Implemented inline (not via formal plan). Added Talents free-text field through full stack (Character model, CharacterSaveData DTO, CharacterFileService save/load, CharacterExportData, MarkdownBuilder, MarkdownExportService, CharacterViewModel). NotesPage now shows Talents → Spells → Notes labeled sections. Committed 4239483.
- [Phase 05-talents-editor]: Talents test coverage added as GREEN-only TDD — implementation already existed in commit 4239483; MinimalData() helper extended with talents parameter following same pattern as spellsKnown

### Pending Todos

- Phase 04 Plan 02 (human verification): needs re-test of Open and Import after MacFilePickerHelper fix. Rebuild with `dotnet build -t:Run -f net10.0-maccatalyst`.
- Phase 05 (Talents Editor): done inline — ROADMAP.md shows it as not started; should be marked complete or the phase removed.

### Blockers/Concerns

- Phase 3 VERIFICATION.md has 8 human_needed items (export E2E, test pass confirmation) — may need resolution before v1.1 ships
