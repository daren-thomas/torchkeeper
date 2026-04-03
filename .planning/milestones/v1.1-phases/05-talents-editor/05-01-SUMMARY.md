---
phase: 05-talents-editor
plan: 01
subsystem: tests
tags: [tests, talents, round-trip, markdown-export, coverage]
dependency_graph:
  requires: []
  provides: [TLNT-01 test coverage for save/load and export]
  affects: [TorchKeeper.Tests]
tech_stack:
  added: []
  patterns: [xUnit Fact, TDD green-path, MinimalData helper extension]
key_files:
  created: []
  modified:
    - TorchKeeper.Tests/Services/CharacterFileServiceTests.cs
    - TorchKeeper.Tests/Export/MarkdownBuilderTests.cs
decisions:
  - "Talents test coverage added as GREEN-only TDD (implementation already existed in commit 4239483)"
  - "MinimalData() helper extended with talents parameter following exact same pattern as spellsKnown"
metrics:
  duration_minutes: 4
  completed_date: "2026-03-22"
  tasks_completed: 2
  files_modified: 2
---

# Phase 5 Plan 1: Talents Test Coverage Summary

Targeted xUnit test assertions covering the Talents field's save/load round-trip and Markdown export rendering — closing the coverage gap left by the inline Talents implementation (commit 4239483).

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add Talents round-trip coverage to CharacterFileServiceTests | 2b00ffc | TorchKeeper.Tests/Services/CharacterFileServiceTests.cs |
| 2 | Add Talents section tests to MarkdownBuilderTests | 4f9ad3a | TorchKeeper.Tests/Export/MarkdownBuilderTests.cs |

## What Was Built

**Task 1 — CharacterFileServiceTests round-trip:**
- Added `Talents = "Backstab +1"` to the `Character` fixture in `RoundTrip_SaveLoad_NoDataLoss()`
- Added `Assert.Equal("Backstab +1", loaded.Talents)` assertion after `SpellsKnown` assertion
- Confirms `Talents` survives a `MapToDto` → JSON serialize → `LoadFromStreamAsync` round-trip

**Task 2 — MarkdownBuilderTests Talents section:**
- Extended `MinimalData()` helper with `string talents = ""` parameter (after `spellsKnown`)
- Added `Talents = talents` to `CharacterExportData` initializer inside `MinimalData()`
- Added Test 19: `BuildMarkdown_TalentsSection_AppearsWhenTalentsNonEmpty` — asserts `"## Talents"` heading and content appear when Talents is non-empty
- Added Test 20: `BuildMarkdown_TalentsSection_OmittedWhenTalentsEmpty` — asserts `"## Talents"` is absent when Talents is empty

## Test Results

Full suite: **54 tests passed, 0 failed, 0 skipped**

The 54 total includes:
- 2 new Talents tests (Tests 19 and 20 in MarkdownBuilderTests)
- All pre-existing tests including the updated RoundTrip test

## Deviations from Plan

None — plan executed exactly as written.

Note: `dotnet build` failed during execution due to macOS `com.apple.provenance` extended attributes on cached build artifacts preventing MSBuild's `WriteLinesToFile` task from reading existing files. Worked around by using `dotnet msbuild` (which succeeded) and `dotnet test --no-build`. This is an environment-specific issue, not a code issue.

## Known Stubs

None.

## Self-Check: PASSED
