---
phase: 03-export
plan: 02
subsystem: export
tags: [maui, export, markdown, share-sheet, toolbar, coin-encumbrance]
dependency_graph:
  requires: [03-01]
  provides: [export-feature-end-to-end, corrected-coin-encumbrance]
  affects: [AppShell, CharacterViewModel, MauiProgram]
tech_stack:
  added: [Share.Default (MAUI), ShareFileRequest, FileSystem.CacheDirectory]
  patterns: [platform-routing-service, relay-command-injection, shell-toolbar-binding]
key_files:
  created:
    - TorchKeeper/Services/MarkdownExportService.cs
  modified:
    - TorchKeeper/ViewModels/CharacterViewModel.cs
    - TorchKeeper/AppShell.xaml
    - TorchKeeper/AppShell.xaml.cs
    - TorchKeeper/MauiProgram.cs
    - TorchKeeper/App.xaml.cs
    - TorchKeeper.Tests/ViewModels/CharacterViewModelTests.cs
decisions:
  - "AppShell registered as singleton in DI; App.xaml.cs receives it via constructor injection"
  - "CharacterViewModel uses two-constructor pattern: parameterless for tests, with MarkdownExportService for DI"
  - "MarkdownExportService platform check: DevicePlatform.iOS and Android use Share.Default; all others use IFileSaver"
  - "SpellsKnown added as [ObservableProperty] with direct backing field assignment in LoadCharacter per existing pattern"
  - "Coin encumbrance uses ceiling division: first 100 coins per denomination free, every additional 100 (or part thereof) costs 1 slot — formula: coins > 100 ? (coins - 1) / 100 : 0"
requirements-completed: [MRKD-01]
metrics:
  duration_minutes: 45
  completed_date: 2026-03-21
  tasks_completed: 2
  tasks_total: 2
  files_created: 1
  files_modified: 6
---

# Phase 3 Plan 2: MAUI Export Wiring Summary

**MAUI export layer wired end-to-end: MarkdownExportService routes to share sheet (mobile) or save-as dialog (desktop), triggered by a Shell ToolbarItem; coin encumbrance bug fixed to ceiling division (101+ coins = 1 slot)**

## Status

COMPLETE. Human-verified. Bug fix applied and committed.

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
| Bug fix | Correct coin encumbrance formula (ceiling per denomination) | 1a1d250 |

## Deviations from Plan

**1. [Rule 3 - Blocking] Updated App.xaml.cs to use DI for AppShell**

- **Found during:** Task 1 (step 6)
- **Issue:** The plan mentioned updating App.xaml.cs but the plan's action text focused on MauiProgram changes. App.xaml.cs originally called `new AppShell()` directly, which bypasses DI and means AppShell can't receive the CharacterViewModel constructor parameter.
- **Fix:** Changed `App` constructor signature to `App(AppShell shell)` so MAUI's DI resolves AppShell (and its CharacterViewModel dependency) from the container.
- **Files modified:** TorchKeeper/App.xaml.cs
- **Commit:** 8d8359b (included in Task 1 commit)

**2. [Rule 1 - Bug] Coin encumbrance used floor instead of ceiling division**

- **Found during:** Task 2 (human verification)
- **Issue:** Formula `Math.Max(GP - 100, 0) / 100` gives 0 slots for 101-199 coins (floor division), but the Shadowdark rule requires 1 slot for any coins over 100. Old code meant you needed 200 coins before the first slot was consumed.
- **Fix:** Changed to `coins > 100 ? (coins - 1) / 100 : 0` (integer ceiling) for GP, SP, CP separately; updated `TestCharacterVM` stub to match; renamed `CoinSlots_201GP_Returns1` -> `CoinSlots_201GP_Returns2` with corrected assertion; added `CoinSlots_101GP_Returns1` boundary test.
- **Files modified:** `TorchKeeper/ViewModels/CharacterViewModel.cs`, `TorchKeeper.Tests/ViewModels/CharacterViewModelTests.cs`
- **Verification:** All 46 tests pass including all 7 CoinSlots tests
- **Commit:** 1a1d250

---

**Total deviations:** 2 auto-fixed (1 blocking / DI wiring, 1 bug / coin formula)
**Impact on plan:** Both fixes required for correct behavior. No scope creep.

## Build Notes

The MAUI project build reports `Build FAILED` due to `MSB4216` — a pre-existing ILLink task host environment error unrelated to these changes. The C# compilation succeeds (DLL produced) and TorchKeeper.Core + TorchKeeper.Tests build cleanly with 0 errors. This ILLink error was present before these changes (verified via `git stash`).

## Known Stubs

None — all data is wired from CharacterViewModel to CharacterExportData to MarkdownBuilder.

## Self-Check

- [x] TorchKeeper/Services/MarkdownExportService.cs exists and contains `class MarkdownExportService`
- [x] TorchKeeper/ViewModels/CharacterViewModel.cs contains `private string spellsKnown`
- [x] TorchKeeper/ViewModels/CharacterViewModel.cs contains `spellsKnown = character.SpellsKnown`
- [x] TorchKeeper/ViewModels/CharacterViewModel.cs contains `ExportAsync`
- [x] TorchKeeper/ViewModels/CharacterViewModel.cs contains corrected CoinSlots formula
- [x] TorchKeeper/AppShell.xaml contains `ToolbarItem` and `ExportCommand`
- [x] TorchKeeper/AppShell.xaml.cs contains `BindingContext`
- [x] TorchKeeper/MauiProgram.cs contains `MarkdownExportService`
- [x] Commit 8d8359b verified in git log
- [x] Commit 1a1d250 verified in git log
- [x] All 46 tests pass

## Self-Check: PASSED
