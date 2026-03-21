---
phase: 4
slug: file-menu
status: draft
nyquist_compliant: false
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
| **Config file** | SdCharacterSheet.Tests/SdCharacterSheet.Tests.csproj |
| **Quick run command** | `dotnet test SdCharacterSheet.Tests` |
| **Full suite command** | `dotnet test SdCharacterSheet.Tests` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test SdCharacterSheet.Tests`
- **After every plan wave:** Run `dotnet test SdCharacterSheet.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 4-01-01 | 01 | 1 | FILE-01 | unit | `dotnet test SdCharacterSheet.Tests --filter "Save"` | ✅ | ⬜ pending |
| 4-01-02 | 01 | 1 | FILE-02 | unit | `dotnet test SdCharacterSheet.Tests --filter "Load"` | ✅ | ⬜ pending |
| 4-01-03 | 01 | 1 | FILE-03 | unit | `dotnet test SdCharacterSheet.Tests --filter "Import"` | ✅ | ⬜ pending |
| 4-02-01 | 02 | 2 | FILE-01 | manual | See Manual-Only Verifications | N/A | ⬜ pending |
| 4-02-02 | 02 | 2 | FILE-02 | manual | See Manual-Only Verifications | N/A | ⬜ pending |
| 4-02-03 | 02 | 2 | FILE-03 | manual | See Manual-Only Verifications | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements.*

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

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
