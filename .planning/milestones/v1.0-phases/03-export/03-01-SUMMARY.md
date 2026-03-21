---
phase: 03-export
plan: 01
subsystem: export
tags: [tdd, markdown, core-library, pure-functions]
dependency_graph:
  requires: []
  provides: [CharacterExportData, MarkdownBuilder.BuildMarkdown, MarkdownBuilder.BuildFileName]
  affects: [03-02-PLAN (consumes CharacterExportData and MarkdownBuilder)]
tech_stack:
  added: []
  patterns: [TDD RED-GREEN-REFACTOR, pure static builder, plain CLR record DTO]
key_files:
  created:
    - SdCharacterSheet.Core/Export/CharacterExportData.cs
    - SdCharacterSheet.Core/Export/MarkdownBuilder.cs
    - SdCharacterSheet.Tests/Export/MarkdownBuilderTests.cs
  modified: []
decisions:
  - CharacterExportData is a plain CLR record in SdCharacterSheet.Core — zero MAUI dependencies, fully testable from net10.0 test project
  - BuildMarkdown and BuildFileName are pure static methods — no I/O, no state, deterministic output
  - Gear slot table built as a List<string> of exactly GearSlotTotal entries before rendering — prevents row count drift (per RESEARCH.md Pitfall 3)
  - HP and XP always appear in identity section in plain format (HP: n / n, XP: n / n) regardless of other optional fields
  - Identity fields are omitted when empty/whitespace — only Name, HP, XP are always included
metrics:
  duration_minutes: 2
  completed_date: "2026-03-21"
  tasks_completed: 1
  files_created: 3
  files_modified: 0
---

# Phase 03 Plan 01: Markdown Export Builder (TDD) Summary

**One-liner:** Pure static MarkdownBuilder with CharacterExportData DTO — 18 tests covering all section formatting decisions D-05 through D-21, zero MAUI dependencies.

## What Was Built

A complete, unit-tested Markdown generation layer in `SdCharacterSheet.Core`:

- `CharacterExportData` — plain CLR record holding all data needed for Markdown export. Carries pre-computed stat totals and modifiers (avoiding ViewModel coupling), AC bonuses, gear slot counts, coin slots, and all character fields. Used as the single data boundary between ViewModel and builder.

- `MarkdownBuilder.BuildMarkdown(CharacterExportData)` — pure static method using `StringBuilder`. Renders 7 sections in order: Identity (with HP/XP always included), Stats (with indented bonus bullets and AC subsection), Attacks (bullet list), Currency (GP/SP/CP), Gear (Markdown table with exactly GearSlotTotal rows, multi-slot expansion, coin rows, empty slot padding), optional Spells (when SpellsKnown non-empty), and Notes.

- `MarkdownBuilder.BuildFileName(CharacterExportData)` — pure static method. Produces `{Name}-{Class}{Level}.md`, falls back to `{Name}.md` when Class is empty, falls back to `Character.md` when Name is empty. Strips all `Path.GetInvalidFileNameChars()` characters.

- `MarkdownBuilderTests` — 18 xUnit tests covering every formatting rule. Tests run in ~300ms on the `net10.0` test project with no MAUI dependency.

## TDD Phases

**RED:** Created `CharacterExportData.cs`, stub `MarkdownBuilder.cs` (both methods throwing `NotImplementedException`), and `MarkdownBuilderTests.cs` with all 18 tests. All 18 failed as expected.

**GREEN:** Implemented `BuildMarkdown` and `BuildFileName`. All 18 tests passed on first run.

**REFACTOR:** Removed duplicate XP line from identity section (labeled `**XP:**` format was redundant alongside the D-20 plain `XP: n / n` format). Tests remained green.

## Acceptance Criteria Status

| Criterion | Status |
|-----------|--------|
| CharacterExportData.cs contains `public record CharacterExportData` | PASS |
| CharacterExportData.cs contains `public record StatExportData` | PASS |
| CharacterExportData.cs contains `public record BonusExportData` | PASS |
| CharacterExportData.cs contains `public record GearExportItem` | PASS |
| MarkdownBuilder.cs contains `public static string BuildMarkdown(CharacterExportData` | PASS |
| MarkdownBuilder.cs contains `public static string BuildFileName(CharacterExportData` | PASS |
| MarkdownBuilderTests.cs exists with at least 14 test methods | PASS (18 tests) |
| All tests pass | PASS (18/18) |
| `dotnet build SdCharacterSheet.Core` exits 0 | PASS |
| MarkdownBuilder.cs has no `using Microsoft.Maui` | PASS |
| MarkdownBuilder.cs output has no `---` horizontal rules | PASS |

## Deviations from Plan

None — plan executed exactly as written. RED-GREEN-REFACTOR phases completed in sequence.

## Known Stubs

None. All methods are fully implemented and tested.

## Self-Check: PASSED

Files exist:
- SdCharacterSheet.Core/Export/CharacterExportData.cs: FOUND
- SdCharacterSheet.Core/Export/MarkdownBuilder.cs: FOUND
- SdCharacterSheet.Tests/Export/MarkdownBuilderTests.cs: FOUND

Commits exist:
- 9be7b78 (RED phase — tests + stubs): FOUND
- 032f71a (GREEN phase — implementation): FOUND
- e2cfa5e (REFACTOR — cleanup): FOUND
