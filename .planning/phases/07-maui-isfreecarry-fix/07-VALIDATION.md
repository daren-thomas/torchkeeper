---
phase: 7
slug: maui-isfreecarry-fix
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-03-29
---

# Phase 7 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit (SdCharacterSheet.Tests) |
| **Config file** | `SdCharacterSheet.Tests/SdCharacterSheet.Tests.csproj` |
| **Quick run command** | `dotnet test SdCharacterSheet.Tests` |
| **Full suite command** | `dotnet test SdCharacterSheet.Tests && dotnet build SdCharacterSheet -f net10.0-maccatalyst` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test SdCharacterSheet.Tests`
- **After every plan wave:** Run `dotnet test SdCharacterSheet.Tests && dotnet build SdCharacterSheet -f net10.0-maccatalyst`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 7-01-01 | 01 | 1 | GEAR-01 | build | `dotnet build SdCharacterSheet -f net10.0-maccatalyst` | ✅ | ⬜ pending |
| 7-01-02 | 01 | 1 | GEAR-01 | build | `dotnet build SdCharacterSheet -f net10.0-maccatalyst` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

None required. Both tasks use build verification as the primary automated gate. Existing Core-layer round-trip tests (`RoundTrip_GearItem_IsFreeCarry_Persists`) already cover serialization correctness; this phase only adds the MAUI-local property/mapping parity needed for compilation.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Free-carry flag retained after save+reload in running MAUI app | GEAR-01 | MAUI platform incompatible with xUnit test runner | 1. Open app, flag a gear item as free carry. 2. Save character. 3. Reload. 4. Verify item still shows IsFreeCarry=true. |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 15s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
