---
phase: 03-export
plan: 02
subsystem: export
tags: [maui, export, markdown, share-sheet, toolbar]
dependency_graph:
  requires: [03-01]
  provides: [export-feature-end-to-end]
  affects: [AppShell, CharacterViewModel, MauiProgram]
tech_stack:
  added: [Share.Default (MAUI), ShareFileRequest, FileSystem.CacheDirectory]
  patterns: [platform-routing-service, relay-command-injection, shell-toolbar-binding]
key_files:
  created:
    - SdCharacterSheet/Services/MarkdownExportService.cs
  modified:
    - SdCharacterSheet/ViewModels/CharacterViewModel.cs
    - SdCharacterSheet/AppShell.xaml
    - SdCharacterSheet/AppShell.xaml.cs
    - SdCharacterSheet/MauiProgram.cs
    - SdCharacterSheet/App.xaml.cs
decisions:
  - "AppShell registered as singleton in DI; App.xaml.cs receives it via constructor injection"
  - "CharacterViewModel uses two-constructor pattern: parameterless for tests, with MarkdownExportService for DI"
  - "MarkdownExportService platform check: DevicePlatform.iOS and Android use Share.Default; all others use IFileSaver"
  - "SpellsKnown added as [ObservableProperty] with direct backing field assignment in LoadCharacter per existing pattern"
metrics:
  duration_minutes: 18
  completed_date: 2026-03-21
  tasks_completed: 1
  tasks_total: 2
  files_created: 1
  files_modified: 5
---

# Phase 3 Plan 2: MAUI Export Wiring Summary

**One-liner:** MAUI export layer wired end-to-end: MarkdownExportService routes to share sheet (mobile) or save-as dialog (desktop), triggered by a Shell ToolbarItem bound to ExportCommand on CharacterViewModel.

## Status

Paused at human verification checkpoint (Task 2). Task 1 completed and committed.

## What Was Built

Complete export feature integration:

1. **`MarkdownExportService`** — platform-routing service that:
   - Calls `MarkdownBuilder.BuildMarkdown` and `BuildFileName` (from Plan 01)
   - On iOS/Android: writes `.md` file to `FileSystem.CacheDirectory` and triggers `Share.Default.RequestAsync`
   - On desktop: encodes UTF-8 bytes and calls `IFileSaver.SaveAsync`
   - Maps `CharacterViewModel` to `CharacterExportData` including all stats, gear, AC bonuses, spells, and notes

2. **`CharacterViewModel` changes** — three additions:
   - `[ObservableProperty] private string spellsKnown = ""` (D-22)
   - `spellsKnown = character.SpellsKnown` in `LoadCharacter()`
   - `[RelayCommand] private async Task ExportAsync()` backed by `MarkdownExportService?` field
   - Second constructor `CharacterViewModel(MarkdownExportService)` for DI injection

3. **`AppShell.xaml`** — added `Shell.ToolbarItems` with `ToolbarItem` bound to `ExportCommand` (D-01)

4. **`AppShell.xaml.cs`** — updated to accept `CharacterViewModel` via constructor and set `BindingContext = vm` (required for toolbar binding)

5. **`App.xaml.cs`** — updated to accept `AppShell` via constructor (DI resolves it)

6. **`MauiProgram.cs`** — three registrations added:
   - `AddSingleton<MarkdownExportService>()`
   - `AddSingleton<CharacterViewModel>(sp => new CharacterViewModel(sp.GetRequiredService<MarkdownExportService>()))`
   - `AddSingleton<AppShell>()`

## Commits

| Task | Description | Commit |
|------|-------------|--------|
| Task 1 | Wire Markdown export into MAUI app | 8d8359b |

## Deviations from Plan

**1. [Rule 3 - Blocking] Updated App.xaml.cs to use DI for AppShell**

- **Found during:** Task 1 (step 6)
- **Issue:** The plan mentioned updating App.xaml.cs but the plan's action text focused on MauiProgram changes. App.xaml.cs originally called `new AppShell()` directly, which bypasses DI and means AppShell can't receive the CharacterViewModel constructor parameter.
- **Fix:** Changed `App` constructor signature to `App(AppShell shell)` so MAUI's DI resolves AppShell (and its CharacterViewModel dependency) from the container.
- **Files modified:** SdCharacterSheet/App.xaml.cs
- **Commit:** 8d8359b (included in Task 1 commit)

## Build Notes

The MAUI project build reports `Build FAILED` due to `MSB4216` — a pre-existing ILLink task host environment error unrelated to these changes. The C# compilation succeeds (DLL produced) and SdCharacterSheet.Core + SdCharacterSheet.Tests build cleanly with 0 errors. This ILLink error was present before these changes (verified via `git stash`).

## Awaiting Human Verification

Task 2 is a human verification checkpoint. The tester should:
1. Launch the app
2. Load a character
3. Verify Export button appears in toolbar on all tabs
4. Tap Export and confirm share sheet (mobile) or save-as dialog (desktop)
5. Open the .md file and verify content structure

## Known Stubs

None — all data is wired from CharacterViewModel to CharacterExportData to MarkdownBuilder.

## Self-Check

- [x] SdCharacterSheet/Services/MarkdownExportService.cs exists and contains `class MarkdownExportService`
- [x] SdCharacterSheet/ViewModels/CharacterViewModel.cs contains `private string spellsKnown`
- [x] SdCharacterSheet/ViewModels/CharacterViewModel.cs contains `spellsKnown = character.SpellsKnown`
- [x] SdCharacterSheet/ViewModels/CharacterViewModel.cs contains `ExportAsync`
- [x] SdCharacterSheet/AppShell.xaml contains `ToolbarItem` and `ExportCommand`
- [x] SdCharacterSheet/AppShell.xaml.cs contains `BindingContext`
- [x] SdCharacterSheet/MauiProgram.cs contains `MarkdownExportService`
- [x] Commit 8d8359b verified in git log
