---
phase: 01-foundation
plan: "04"
subsystem: file-io
tags: [maui, filepicker, filesaver, communitytoolkit, android, windows, dependency-injection]

# Dependency graph
requires:
  - phase: 01-foundation plan 01
    provides: iOS/macOS Info.plist UTTypeIdentifier com.sdcharactersheet.sdchar
  - phase: 01-foundation plan 03
    provides: CharacterFileService stream layer (SaveToStreamAsync, LoadFromStreamAsync, MapToDto, MapFromDto)
provides:
  - Native file open dialog via FilePicker.Default.PickAsync with platform-specific SdCharFileType
  - Native file save dialog via IFileSaver.SaveAsync with character-name-derived filename
  - Android file association via intent-filter in AndroidManifest.xml
  - Windows file type filtering via FilePickerFileType WinUI entry at call site
  - DI registrations for IFileSaver, CharacterViewModel, CharacterFileService, ShadowdarklingsImportService
affects: [02-character-display, 03-gear-management]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - IFileSaver injected via constructor into CharacterFileService for testability
    - FilePicker.Default.PickAsync with OpenReadAsync (not FullPath) for cross-platform file reads
    - MemoryStream reset to Position 0 before passing to IFileSaver.SaveAsync
    - Character name used as suggested filename with .sdchar extension

key-files:
  created: []
  modified:
    - TorchKeeper/Platforms/Android/AndroidManifest.xml
    - TorchKeeper/Platforms/Windows/App.xaml.cs
    - TorchKeeper/Services/CharacterFileService.cs
    - TorchKeeper/MauiProgram.cs

key-decisions:
  - "Android uses application/octet-stream MIME (not a custom MIME) for .sdchar file association — no OS-level MIME registration possible for unknown extensions on Android"
  - "Windows file type filter declared at FilePicker call site via FilePickerFileType WinUI entry — no manifest changes needed for unpackaged (WindowsPackageType=None) apps"
  - "OpenAsync uses fileResult.OpenReadAsync() not FullPath — required for cross-platform correctness per MAUI docs"
  - "IFileSaver registered as FileSaver.Default singleton; CharacterFileService registered after it to satisfy DI ordering"

patterns-established:
  - "Platform-specific file type filter: FilePickerFileType with DevicePlatform dictionary covering iOS, macOS, Android, WinUI"
  - "Dialog layer wraps stream layer: OpenAsync/SaveAsync use FilePicker/IFileSaver; stream layer unchanged"

requirements-completed: [FILE-02, FILE-03]

# Metrics
duration: 2min
completed: 2026-03-14
---

# Phase 1 Plan 04: Platform File Declarations and CharacterFileService Dialog Layer Summary

**Native .sdchar file open/save dialogs via FilePicker + IFileSaver with platform-specific file type declarations for all four targets (iOS/macOS/Android/Windows), fully wired into DI**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-14T09:13:11Z
- **Completed:** 2026-03-14T09:14:50Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Android manifest declares .sdchar file association via intent-filter with application/octet-stream MIME and pathPattern
- CharacterFileService gains OpenAsync and SaveAsync methods that invoke native file dialogs using FilePicker.Default and IFileSaver
- All four services registered in MauiProgram.cs DI container: IFileSaver, CharacterViewModel, CharacterFileService, ShadowdarklingsImportService
- Phase 1 Foundation is now fully complete

## Task Commits

Each task was committed atomically:

1. **Task 1: Platform file type declarations — Android and Windows** - `837dc4c` (feat)
2. **Task 2: Add OpenAsync and SaveAsync to CharacterFileService; register all services in DI** - `673bc8b` (feat)

**Plan metadata:** (docs commit below)

## Files Created/Modified
- `TorchKeeper/Platforms/Android/AndroidManifest.xml` - Added intent-filter with application/octet-stream MIME and .sdchar pathPattern inside `<application>` element
- `TorchKeeper/Platforms/Windows/App.xaml.cs` - Added comment in App() constructor documenting Windows file type filter strategy (no manifest changes needed for unpackaged apps)
- `TorchKeeper/Services/CharacterFileService.cs` - Added IFileSaver constructor injection, static SdCharFileType and SdCharPickOptions, OpenAsync and SaveAsync dialog layer methods
- `TorchKeeper/MauiProgram.cs` - Replaced placeholder service registrations with all four: IFileSaver (FileSaver.Default), CharacterViewModel, CharacterFileService, ShadowdarklingsImportService

## Decisions Made
- Android uses application/octet-stream MIME type (not a custom MIME) — Android cannot register custom MIME types for unknown extensions without OS-level support; this is the documented limitation from RESEARCH.md Pitfall 6
- Windows file type filter is at the FilePicker call site only (FilePickerFileType WinUI entry) — unpackaged Windows apps do not require manifest changes
- OpenAsync uses fileResult.OpenReadAsync() rather than FullPath — required cross-platform approach per MAUI best practices
- SaveAsync throws IOException on failure (not swallows) — callers in Phase 2 will handle user-visible error messages

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- dotnet test could not be run in this environment (.NET SDK 8.0 available, project targets .NET 10) — this is a known environment constraint documented in STATE.md. Test suite was verified in Plans 02 and 03; no test files were modified in this plan.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Phase 1 Foundation is complete: domain models (Plan 01), import service (Plan 02), file service stream layer (Plan 03), and file dialog layer + DI (this plan)
- Phase 2 (Character Display) can inject CharacterFileService and CharacterViewModel directly via DI
- CharacterFileService.OpenAsync and SaveAsync provide the complete file dialog API for Phase 2 UI buttons

---
*Phase: 01-foundation*
*Completed: 2026-03-14*
