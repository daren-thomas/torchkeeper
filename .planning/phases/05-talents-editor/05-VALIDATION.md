---
phase: 5
slug: talents-editor
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-22
---

# Phase 5 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit |
| **Config file** | SdCharacterSheet.Tests/SdCharacterSheet.Tests.csproj |
| **Quick run command** | `dotnet test SdCharacterSheet.Tests` |
| **Full suite command** | `dotnet test SdCharacterSheet.Tests` |
| **Estimated runtime** | ~5 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test SdCharacterSheet.Tests`
- **After every plan wave:** Run `dotnet test SdCharacterSheet.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 10 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 5-01-01 | 01 | 1 | TLNT-01 | unit | `dotnet test SdCharacterSheet.Tests --filter "RoundTrip"` | ✅ | ⬜ pending |
| 5-01-02 | 01 | 1 | TLNT-01 | unit | `dotnet test SdCharacterSheet.Tests --filter "MarkdownBuilder"` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Talents/Spells textarea visible above Notes on Notes tab | TLNT-01 | UI layout | Launch app, navigate to Notes tab, verify Talents/Spells area appears above Notes |
| Talents text scrollable independently | TLNT-01 | UI behavior | Enter multi-line text, verify scroll |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 10s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
