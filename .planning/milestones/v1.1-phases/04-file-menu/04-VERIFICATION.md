---
phase: 04-file-menu
verified: 2026-03-22T00:00:00Z
status: human_needed
score: 4/4 must-haves verified
human_verification:
  - test: "Verify Save, Open, Import on a device (macOS, iOS, Android, or Windows)"
    expected: "Save produces .sdchar file with correct data; Open loads it and replaces sheet; Import populates sheet from Shadowdarklings JSON; Cancel does not crash"
    why_human: "File picker dialogs and platform-native save dialogs cannot be exercised by unit tests or static analysis. The MacFilePickerHelper bypass for macOS Catalyst (commit b2d9977) also requires runtime confirmation."
  - test: "Confirm all three menu items are visible on all four target platforms"
    expected: "Save/Open/Import appear in the File menu bar on desktop (macOS, Windows) and in the toolbar overflow menu on mobile (iOS, Android)"
    why_human: "XAML ToolbarItem Order=Secondary behavior and MenuBarItem rendering must be observed at runtime per platform"
---

# Phase 4: File Menu Verification Report

**Phase Goal:** Wire Save, Load, and Import file operations into the app UI with platform-native file dialogs
**Verified:** 2026-03-22
**Status:** human_needed
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | User can tap Save in the File menu and a native save dialog opens, producing a .sdchar file | ? HUMAN NEEDED | SaveCommand wired in CharacterViewModel (line 129-135), calls `_fileService.SaveAsync(BuildCharacterFromViewModel())`. AppShell has `Command="{Binding SaveCommand}"` in both MenuFlyoutItem and ToolbarItem. Runtime behavior cannot be verified statically. |
| 2 | User can tap Open/Load in the File menu and a native file picker opens, replacing the current character | ? HUMAN NEEDED | LoadCommand wired (lines 137-143), calls `_fileService.OpenAsync()` then `LoadCharacter(character)`. AppShell has `Command="{Binding LoadCommand}"`. Runtime dialog behavior requires human. |
| 3 | User can tap Import in the File menu and a native file picker opens for JSON, populating the sheet from Shadowdarklings export | ? HUMAN NEEDED | ImportCommand wired (lines 146-153), calls `_importFileService.ImportAsync()` then `LoadCharacter(character)`. MauiImportFileService uses MacFilePickerHelper on macOS Catalyst (commit b2d9977). Runtime dialog behavior requires human. |
| 4 | All three menu items are visible on desktop (MenuBarItems) and mobile (ToolbarItem overflow) | ? HUMAN NEEDED | AppShell.xaml has all three commands in both `Shell.MenuBarItems` and `Shell.ToolbarItems`. ToolbarItems use `Order="Secondary"` (overflow). XAML is correct but per-platform rendering requires human. |

**Score:** All 4 truths have full automated implementation evidence. Runtime confirmation needed.

---

## Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `TorchKeeper.Tests/ViewModels/CharacterViewModelFileCommandTests.cs` | 6 unit tests for SaveCommand, LoadCommand, ImportCommand using test-local fakes | VERIFIED | File exists (327 lines). Contains `class CharacterViewModelFileCommandTests`, `FakeFileSaver`, `TestFileCommandVM`, 6 `[Fact]` test methods. Created in commit 34b8796. |
| `TorchKeeper/ViewModels/CharacterViewModel.cs` | SaveCommand, LoadCommand, ImportCommand via [RelayCommand]; BuildCharacterFromViewModel; new constructor accepting file + import services | VERIFIED | Contains `[RelayCommand]` before all three async methods (lines 129, 137, 146). `BuildCharacterFromViewModel()` at line 200 uses `GearItemSource.Gear` discriminator. Three-arg constructor at line 186. Fields `_fileService` and `_importFileService` at lines 125-126. |
| `TorchKeeper/Services/MauiImportFileService.cs` | MAUI file picker + ShadowdarklingsImportService delegation for JSON import | VERIFIED | File exists (50 lines). Contains `class MauiImportFileService`, `private readonly ShadowdarklingsImportService _importService`, `await using var stream`, null-check on picker result. MacCatalyst branch uses `MacFilePickerHelper.PickAsync` (commit b2d9977). |
| `TorchKeeper/MauiProgram.cs` | DI registration for MauiImportFileService and updated CharacterViewModel constructor | VERIFIED | Line 28: `builder.Services.AddSingleton<MauiImportFileService>()`. Lines 30-34: CharacterViewModel factory passes all three services using `(MauiCharacterFileService)sp.GetRequiredService<CharacterFileService>()` cast. |
| `TorchKeeper/AppShell.xaml` | Save, Open, Import menu items (MenuFlyoutItem + ToolbarItem) | VERIFIED | Lines 9-11: three ToolbarItems with `Order="Secondary"`. Lines 18-23: three MenuFlyoutItems in the File MenuBarItem, followed by `<MenuFlyoutSeparator />` before Export As... |

---

## Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `CharacterViewModel.SaveAsync` | `CharacterFileService.SaveAsync` | `_fileService.SaveAsync(BuildCharacterFromViewModel())` | WIRED | Line 134: `await _fileService.SaveAsync(character)` where character comes from `BuildCharacterFromViewModel()` on line 133. |
| `CharacterViewModel.LoadAsync` | `MauiCharacterFileService.OpenAsync` | `_fileService.OpenAsync()` | WIRED | Line 141: `var character = await _fileService.OpenAsync()`. `_fileService` typed as `MauiCharacterFileService?` which exposes `OpenAsync()`. |
| `CharacterViewModel.ImportAsync` | `MauiImportFileService.ImportAsync` | `_importFileService.ImportAsync()` | WIRED | Line 150: `var character = await _importFileService.ImportAsync()`. |
| `AppShell.xaml MenuFlyoutItem` | `CharacterViewModel commands` | `Command={Binding SaveCommand/LoadCommand/ImportCommand}` | WIRED | All three commands bound in both MenuFlyoutItem and ToolbarItem sections. Confirmed by grep. |
| `MauiProgram.cs DI` | `CharacterViewModel constructor` | cast `(MauiCharacterFileService)sp.GetRequiredService<CharacterFileService>()` | WIRED | Line 33: exact cast resolves concrete type despite DI registration being keyed by base type. Pattern differs from PLAN spec but is semantically correct and explicitly documented as the intended approach. |

---

## Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| FILE-01 | 04-01-PLAN.md | User can save the current character to a .sdchar file via the File menu | SATISFIED | `SaveCommand` wired end-to-end through CharacterViewModel → CharacterFileService → IFileSaver. 3 unit tests cover save behavior (builds from VM state, suggests filename, splits gear by Source). AppShell Save menu item bound. |
| FILE-02 | 04-01-PLAN.md | User can load a character from a .sdchar file via the File menu | SATISFIED | `LoadCommand` wired: `_fileService.OpenAsync()` → `LoadCharacter(character)`. Unit test `LoadCommand_ReplacesVMState` verifies state replacement. AppShell Open menu item bound. |
| FILE-03 | 04-01-PLAN.md | User can import a Shadowdarklings.net JSON export via the File menu | SATISFIED | `ImportCommand` wired: `_importFileService.ImportAsync()` → `LoadCharacter(character)`. `MauiImportFileService` delegates to `ShadowdarklingsImportService`. Unit test `ImportCommand_ReplacesVMState` verifies. AppShell Import menu item bound. |

**Orphaned requirements check:** REQUIREMENTS.md maps FILE-01, FILE-02, FILE-03 to Phase 4. All three appear in plan frontmatter. No orphaned requirements.

---

## Anti-Patterns Found

None. No TODOs, FIXMEs, placeholders, or stub implementations found in any phase 4 modified files.

---

## Human Verification Required

### 1. End-to-End Save/Load/Import on macOS Catalyst

**Test:** Build and run on macOS Catalyst (`dotnet build -t:Run -f net10.0-maccatalyst`). Perform:
1. Edit character (set Name, Level, add gear), File menu -> Save... — confirm native save dialog appears with suggested `<Name>.sdchar` filename; verify file exists on disk and is valid JSON
2. Change character name, File menu -> Open... — confirm file picker filtered to .sdchar; load the saved file; verify sheet shows original Name/Level/gear
3. File menu -> Import from Shadowdarklings... — confirm file picker filtered to .json; select `examples/Brim.json`; verify sheet populates with imported character data
4. Cancel each dialog — confirm no crash, character state unchanged

**Expected:** All three operations work. File picker uses `MacFilePickerHelper` (UIDocumentPickerViewController bypass, commit b2d9977) on macOS Catalyst. Cancel returns cleanly.

**Why human:** File picker dialogs (especially the macOS Catalyst UIDocumentPickerViewController bypass) require a running app and user interaction.

### 2. Menu Item Visibility and Placement

**Test:** On macOS/Windows: inspect the File menu bar. On iOS/Android (if available): tap the toolbar overflow (three-dot) menu.

**Expected:** Save, Open, Import appear in the File menu (desktop) with a separator before Export As... On mobile, all three appear in the overflow menu; Export remains in the primary toolbar.

**Why human:** XAML `MenuBarItem` and `ToolbarItem Order="Secondary"` behavior is platform-specific and requires runtime observation.

---

## Notes

The MacFilePickerHelper bypass (commit b2d9977) is a significant deviation from the original plan's `FilePicker.Default.PickAsync()` approach. It was introduced after initial Plan 01 execution to address macOS 15 Sequoia compatibility. The bypass is present in both `MauiImportFileService.cs` (for Import) and presumably in `MauiCharacterFileService.cs` (for Open). The 04-02-SUMMARY.md confirms human approval of all 5 verification groups including this fix, but independent re-confirmation is recommended given the platform-specific nature.

STATE.md notes: "Phase 04 Plan 02 (human verification): needs re-test of Open and Import after MacFilePickerHelper fix." The 04-02-SUMMARY.md reports this was completed ("Human approved — all 5 verification groups passed"), dated 2026-03-22.

---

_Verified: 2026-03-22_
_Verifier: Claude (gsd-verifier)_
