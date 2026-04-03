---
phase: 1
slug: foundation
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-08
---

# Phase 1 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.x |
| **Config file** | `TorchKeeper.Tests/TorchKeeper.Tests.csproj` (Wave 0 creates) |
| **Quick run command** | `dotnet test TorchKeeper.Tests/ --filter "Category=Unit" --no-build` |
| **Full suite command** | `dotnet test TorchKeeper.Tests/` |
| **Estimated runtime** | ~5 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test TorchKeeper.Tests/ --filter "Category=Unit" --no-build`
- **After every plan wave:** Run `dotnet test TorchKeeper.Tests/`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 10 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 1-01-01 | 01 | 0 | FILE-01, FILE-02, FILE-03 | setup | `dotnet test TorchKeeper.Tests/` | ❌ W0 | ⬜ pending |
| 1-02-01 | 02 | 1 | FILE-01 | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests"` | ❌ W0 | ⬜ pending |
| 1-02-02 | 02 | 1 | FILE-01 | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_UsesRolledStats"` | ❌ W0 | ⬜ pending |
| 1-02-03 | 02 | 1 | FILE-01 | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_Currency_TopLevelFields"` | ❌ W0 | ⬜ pending |
| 1-02-04 | 02 | 1 | FILE-01 | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_Currency_LedgerFallback"` | ❌ W0 | ⬜ pending |
| 1-02-05 | 02 | 1 | FILE-01 | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_AcBonuses"` | ❌ W0 | ⬜ pending |
| 1-02-06 | 02 | 1 | FILE-01 | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_UnknownFields_Ignored"` | ❌ W0 | ⬜ pending |
| 1-02-07 | 02 | 1 | FILE-01 | unit | `dotnet test --filter "FullyQualifiedName~ShadowdarklingsImportServiceTests.Import_CurrentHP_EqualsMaxHP"` | ❌ W0 | ⬜ pending |
| 1-03-01 | 03 | 2 | FILE-02, FILE-03 | unit | `dotnet test --filter "FullyQualifiedName~CharacterFileServiceTests.RoundTrip_SaveLoad_NoDataLoss"` | ❌ W0 | ⬜ pending |
| 1-03-02 | 03 | 2 | FILE-03 | unit | `dotnet test --filter "FullyQualifiedName~CharacterFileServiceTests.Save_ContainsVersionField"` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `TorchKeeper.Tests/TorchKeeper.Tests.csproj` — xUnit test project (targets `net10.0`, not a platform TFM)
- [ ] `TorchKeeper.Tests/Services/ShadowdarklingsImportServiceTests.cs` — stubs for FILE-01
- [ ] `TorchKeeper.Tests/Services/CharacterFileServiceTests.cs` — stubs for FILE-02, FILE-03
- [ ] `TorchKeeper.Tests/TestData/Brim.json` — copy of `examples/Brim.json` for test fixture
- [ ] `dotnet add TorchKeeper.Tests package xunit` — xUnit framework
- [ ] `dotnet add TorchKeeper.Tests package Microsoft.NET.Test.Sdk` — test runner host
- [ ] `dotnet add TorchKeeper.Tests package xunit.runner.visualstudio` — VS integration

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| FilePicker open dialog appears on Windows | FILE-02 | Requires native file dialog | Run app, invoke open, verify dialog opens and `.sdchar` filter works |
| FileSaver save dialog appears on Windows | FILE-03 | Requires native file dialog | Run app, invoke save, verify dialog opens and file is created |
| FilePicker opens on macOS Catalyst | FILE-02 | Requires macOS device | Run on Mac, verify dialog opens, verify `.sdchar` files visible |
| FileSaver saves on macOS Catalyst | FILE-03 | Requires macOS device | Run on Mac, invoke save, verify file written to chosen location |
| FilePicker opens on iOS device (not simulator) | FILE-02 | iOS UTType required on device | Install on device, copy `.sdchar` to Files app, verify picker shows it |
| FileSaver saves on iOS | FILE-03 | Requires iOS device | Run on device, invoke save, verify file appears in Files |
| FilePicker opens on Android | FILE-02 | Requires Android device/emulator | Run on Android, invoke open, verify user can navigate to `.sdchar` |
| FileSaver saves on Android | FILE-03 | Requires Android device/emulator | Run on Android, invoke save, verify file is created |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 10s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
