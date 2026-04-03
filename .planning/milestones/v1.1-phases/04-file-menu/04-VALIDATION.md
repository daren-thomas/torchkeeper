---
phase: 4
slug: file-menu
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-03-21
---

# Phase 4 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit (.NET) |
| **Config file** | TorchKeeper.Tests/TorchKeeper.Tests.csproj |
| **Quick run command** | `dotnet test TorchKeeper.Tests --filter "FullyQualifiedName~FileCommand"` |
| **Full suite command** | `dotnet test TorchKeeper.Tests` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test TorchKeeper.Tests --filter "FullyQualifiedName~FileCommand"`
- **After every plan wave:** Run `dotnet test TorchKeeper.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 4-01-00 | 01 | 0 | FILE-01,02,03 | unit | `dotnet test TorchKeeper.Tests --filter "FullyQualifiedName~FileCommand"` | Wave 0 creates it | ⬜ pending |
| 4-01-01 | 01 | 1 | FILE-01 | unit+build | `dotnet build TorchKeeper/TorchKeeper.csproj --no-restore -v q` | N/A (build only) | ⬜ pending |
| 4-01-02 | 01 | 1 | FILE-01,02,03 | unit+build | `dotnet build TorchKeeper/TorchKeeper.csproj --no-restore -v q && dotnet test TorchKeeper.Tests --filter "FullyQualifiedName~FileCommand"` | Created in 4-01-00 | ⬜ pending |
| 4-02-01 | 02 | 2 | FILE-01,02,03 | manual | See Manual-Only Verifications | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Task 0 in Plan 04-01 creates the test file before implementation begins:

- [ ] `TorchKeeper.Tests/ViewModels/CharacterViewModelFileCommandTests.cs` — test-local fakes (FakeFileSaver, TestFileCommandVM) + 6 tests covering SaveCommand, LoadCommand, ImportCommand behavior

**Note:** The test project references only `TorchKeeper.Core`, not the MAUI head project. Tests use test-local stubs mirroring the command logic (same pattern as existing `TestCharacterVM` in `CharacterViewModelTests.cs`). This validates the core command behavior (BuildCharacterFromViewModel, gear splitting by Source, LoadCharacter delegation) without requiring MAUI runtime dependencies.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Native file save dialog opens on macOS/Windows | FILE-01 | Platform UI dialogs cannot be automated in unit tests | Tap Save → verify native OS dialog appears → confirm .sdchar written to chosen path |
| Native file picker opens on iOS/Android | FILE-02 | Platform UI dialogs cannot be automated in unit tests | Tap Load → verify native OS picker appears → confirm character replaces current sheet |
| Import from Shadowdarklings JSON | FILE-03 | End-to-end import requires device + real JSON file | Tap Import → pick a .json export → confirm sheet populates correctly |
| File menu visible on all 4 platforms | FILE-01,02,03 | Requires physical/emulated devices per platform | Verify menu items appear: macOS (menu bar), Windows (menu bar), iOS (toolbar overflow), Android (toolbar overflow) |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
