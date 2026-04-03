---
phase: 3
slug: export
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-21
---

# Phase 3 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xunit (TorchKeeper.Tests) |
| **Config file** | `TorchKeeper.Tests/TorchKeeper.Tests.csproj` |
| **Quick run command** | `dotnet test TorchKeeper.Tests` |
| **Full suite command** | `dotnet test TorchKeeper.Tests` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test TorchKeeper.Tests`
- **After every plan wave:** Run `dotnet test TorchKeeper.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 3-01-01 | 01 | 1 | MRKD-01 | unit | `dotnet test TorchKeeper.Tests --filter MarkdownExport` | ❌ W0 | ⬜ pending |
| 3-01-02 | 01 | 1 | MRKD-01 | unit | `dotnet test TorchKeeper.Tests --filter MarkdownExport` | ❌ W0 | ⬜ pending |
| 3-02-01 | 02 | 2 | MRKD-01 | manual | share sheet / save dialog on device | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `TorchKeeper.Tests/MarkdownExportServiceTests.cs` — stubs for MRKD-01 (BuildMarkdown output tests)

*Existing xunit infrastructure detected — only test stubs need adding.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Mobile share sheet appears | MRKD-01 | Requires physical/emulated device with OS share API | Tap export button on iOS/Android; verify native share sheet opens with .md file |
| Desktop save-as dialog appears | MRKD-01 | Requires desktop UI interaction | Click export on Windows/macOS; verify save dialog opens and file writes correctly |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
