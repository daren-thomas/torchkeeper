---
phase: 2
slug: core-sheet
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-14
---

# Phase 2 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 (already installed in TorchKeeper.Tests) |
| **Config file** | TorchKeeper.Tests/TorchKeeper.Tests.csproj |
| **Quick run command** | `dotnet test TorchKeeper.Tests/ --filter "Category=Unit" -x` |
| **Full suite command** | `dotnet test TorchKeeper.Tests/` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test TorchKeeper.Tests/ --filter "Category=Unit" -x`
- **After every plan wave:** Run `dotnet test TorchKeeper.Tests/`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 2-W0-01 | W0 | 0 | STAT-01, STAT-02, GEAR-03, GEAR-04, HITP-01, IDNT-01 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~CharacterViewModel" -x` | ❌ W0 | ⬜ pending |
| 2-W0-02 | W0 | 0 | GEAR-01 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~GearItemViewModel" -x` | ❌ W0 | ⬜ pending |
| 2-01-01 | 01 | 1 | STAT-01 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~StatModifier" -x` | ✅ W0 | ⬜ pending |
| 2-01-02 | 01 | 1 | STAT-02 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~BonusFilter" -x` | ✅ W0 | ⬜ pending |
| 2-02-01 | 02 | 1 | HITP-01 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~NegativeHP" -x` | ✅ W0 | ⬜ pending |
| 2-02-02 | 02 | 1 | HITP-02 | manual | N/A — UI stepper verified by inspection | N/A | ⬜ pending |
| 2-03-01 | 03 | 1 | GEAR-01 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~GearItemViewModel" -x` | ✅ W0 | ⬜ pending |
| 2-03-02 | 03 | 1 | GEAR-03 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~GearSlotTotal" -x` | ✅ W0 | ⬜ pending |
| 2-03-03 | 03 | 1 | GEAR-04 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~CoinSlots" -x` | ✅ W0 | ⬜ pending |
| 2-03-04 | 03 | 1 | GEAR-02 | manual | N/A — add/edit/remove gear verified by inspection | N/A | ⬜ pending |
| 2-04-01 | 04 | 1 | IDNT-01 | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~LoadCharacter" -x` | ✅ W0 | ⬜ pending |
| 2-04-02 | 04 | 1 | IDNT-02 | manual | N/A — XP field binding verified by inspection | N/A | ⬜ pending |
| 2-05-01 | 05 | 2 | CURR-01 | manual | N/A — coin tracking UI verified by inspection | N/A | ⬜ pending |
| 2-06-01 | 06 | 2 | ATCK-01 | manual | N/A — attack list UI verified by inspection | N/A | ⬜ pending |
| 2-07-01 | 07 | 2 | NOTE-01 | manual | N/A — notes editor verified by inspection | N/A | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `TorchKeeper.Tests/ViewModels/CharacterViewModelTests.cs` — stubs for STAT-01 (modifier math), STAT-02 (bonus filter), GEAR-03 (slot total), GEAR-04 (coin slots), HITP-01 (negative HP), IDNT-01 (LoadCharacter population)
- [ ] `TorchKeeper.Tests/ViewModels/GearItemViewModelTests.cs` — stubs for GEAR-01 (unified GearItem/MagicItem wrapper)
- [ ] Add `CommunityToolkit.Mvvm` NuGet package reference to `TorchKeeper.Tests` project (needed for ViewModel unit tests)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| HP stepper increments/decrements currentHP | HITP-02 | Requires MAUI runtime — headless xUnit targets net10.0 not net10.0-android | Open app, tap +/- on HP widget, verify value changes |
| Stat popup opens on tap showing bonus sources | STAT-02 | MAUI UI event (TapGestureRecognizer + Popup) requires runtime | Tap any stat row, verify popup shows breakdown |
| Add/edit/remove gear updates slot counter live | GEAR-02 | MAUI CollectionView + ObservableCollection requires runtime | Add gear, verify slot counter updates; remove gear, verify counter decreases |
| XP field saves on blur | IDNT-02 | MAUI Entry.Completed binding requires runtime | Edit XP, tap elsewhere, verify persisted on reload |
| GP/SP/CP fields update coin slot total | CURR-01 | Currency bindings require MAUI runtime | Enter currency values, verify coin slots calculated correctly |
| Attack list add/view | ATCK-01 | Attack list UI (CollectionView + sheet) requires runtime | Add attack entry, verify it appears in list |
| Notes editor saves on blur | NOTE-01 | MAUI Editor binding requires runtime | Edit notes, navigate away, verify saved on return |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
