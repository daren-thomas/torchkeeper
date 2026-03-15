---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: planning
stopped_at: "Checkpoint: 02-05 awaiting human verification of full app"
last_updated: "2026-03-15T10:32:56.101Z"
last_activity: 2026-03-08 — Roadmap created; phases derived from requirements
progress:
  total_phases: 3
  completed_phases: 2
  total_plans: 11
  completed_plans: 11
  percent: 0
---

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
| Phase 01-foundation P01 | 5 | 2 tasks | 35 files |
| Phase 01-foundation P03 | 8 | 2 tasks | 3 files |
| Phase 01-foundation P02 | 12 | 2 tasks | 3 files |
| Phase 01-foundation P04 | 2 | 2 tasks | 4 files |
| Phase 01-foundation P05 | 5 | 1 tasks | 1 files |
| Phase 02-core-sheet P00 | 3 | 2 tasks | 3 files |
| Phase 02-core-sheet P01 | 2 | 2 tasks | 3 files |
| Phase 02-core-sheet P02 | 2 | 2 tasks | 5 files |
| Phase 02-core-sheet P03 | 2 | 2 tasks | 5 files |
| Phase 02-core-sheet P04 | 1 | 1 tasks | 2 files |
| Phase 02-core-sheet P05 | 5 | 1 tasks | 2 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Pre-Phase 1]: Native .sdchar format uses a separate CharacterSaveData DTO with version field — never serialize the ViewModel directly
- [Pre-Phase 1]: Shadowdarklings import reads rolledStats (not stats) to avoid double-counting bonuses already included in final stat values
- [Pre-Phase 1]: CharacterViewModel is a singleton shared across all tabs — gear slots depend on STR stat, so a change on the Stats tab must propagate to the Gear tab immediately
- [Phase 01-foundation]: DTO separation: CharacterSaveData is the versioned save format; CharacterViewModel is never serialized
- [Phase 01-foundation]: Import uses RolledStats (not Stats) from Shadowdarklings JSON to avoid double-counting bonuses
- [Phase 01-foundation]: Files created manually due to .NET 8-only execution environment; .NET 10 + MAUI workload required to build
- [Phase 01-foundation]: SaveOptions uses DefaultIgnoreCondition.WhenWritingDefault (not deprecated IgnoreNullValues) for .NET 10 compatibility
- [Phase 01-foundation]: LoadFromStreamAsync returns null on empty stream or JSON parse failure rather than throwing — callers decide error handling
- [Phase 01-foundation]: Static readonly JsonSerializerOptions avoids per-call allocation in ShadowdarklingsImportService
- [Phase 01-foundation]: All bonuses (stat and AC-prefixed) go to same Bonuses list; BonusTo prefix differentiation deferred to Phase 2 UI
- [Phase 01-foundation]: Currency null-coalescing: top-level Gold/Silver/Copper win over ledger sum fallback in import
- [Phase 01-foundation]: Android uses application/octet-stream MIME for .sdchar file association (no custom MIME registration possible on Android for unknown extensions)
- [Phase 01-foundation]: Windows file type filter at FilePicker call site via FilePickerFileType WinUI entry; no manifest changes for unpackaged Windows apps
- [Phase 01-foundation]: OpenAsync uses fileResult.OpenReadAsync() not FullPath for cross-platform correctness
- [Phase 01-foundation]: NullFileSaver throws NotImplementedException rather than returning FileSaverResult — neither test calls SaveAsync so avoiding FileSaverResult constructor complexity is safe and correct
- [Phase 02-core-sheet]: Test stubs defined inline in test files to avoid TFM mismatch between net10.0 test project and MAUI project
- [Phase 02-core-sheet]: CoinSlots uses integer-floor division: (GP-100)/100 per denomination — 200GP is the first slot boundary
- [Phase 02-core-sheet]: StatRowViewModel uses Action<int> callback constructor parameter for decoupled write-back to CharacterViewModel
- [Phase 02-core-sheet]: LoadCharacter sets backing fields directly to bypass setter equality check, then calls OnPropertyChanged(string.Empty)
- [Phase 02-core-sheet]: GearSlotsUsed reactivity: per-item PropertyChanged subscription managed via CollectionChanged handler
- [Phase 02-core-sheet]: Stats section uses BindableLayout (not CollectionView+Expander) — iOS safe per CommunityToolkit.Maui issues #1557, #1670
- [Phase 02-core-sheet]: MaxHP tap-to-edit implemented in code-behind using x:Name references — MaxHP visibility is transient UI state, not persisted ViewModel state
- [Phase 02-core-sheet]: GearItemPopup uses two constructor overloads (edit vs add) for cleaner call sites in GearPage.xaml.cs
- [Phase 02-core-sheet]: CollectionView used for gear list (BindableLayout reserved for Stats page iOS workaround only)
- [Phase 02-core-sheet]: Grid (not VerticalStackLayout) as NotesPage root so Editor fills full page height via VerticalOptions=Fill; no ScrollView wrapper needed
- [Phase 02-core-sheet]: Tab pages registered as AddTransient — Shell creates one per tab, all share singleton CharacterViewModel via DI
- [Phase 02-core-sheet]: AppShell uses DataTemplate binding pattern so Shell resolves pages from DI on demand

### Pending Todos

None yet.

### Blockers/Concerns

- [Phase 1]: .NET version (9 vs 10 LTS) must be decided before project creation — .NET 10 was expected Nov 2025, verify current LTS status
- [Phase 1]: MSIX vs. unpackaged decision for Windows target affects file I/O strategy — decide before implementing file picker
- [Phase 2]: Attack display format decision needed — Shadowdarklings exports pre-formatted attack strings; decide whether to parse them or require re-entry in a structured form

## Session Continuity

Last session: 2026-03-15T10:32:56.099Z
Stopped at: Checkpoint: 02-05 awaiting human verification of full app
Resume file: None
