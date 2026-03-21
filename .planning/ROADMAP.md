# Roadmap: Shadowdark Character Sheet

## Milestones

- ✅ **v1.0 MVP** — Phases 1–3 (shipped 2026-03-21)
- 🚧 **v1.1 File Management & Talents** — Phases 4–5 (in progress)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1–3) — SHIPPED 2026-03-21</summary>

- [x] Phase 1: Foundation (5/5 plans) — completed 2026-03-14
- [x] Phase 2: Core Sheet (6/6 plans) — completed 2026-03-21
- [x] Phase 3: Export (2/2 plans) — completed 2026-03-21

Full details: `.planning/milestones/v1.0-ROADMAP.md`

</details>

### 🚧 v1.1 File Management & Talents (In Progress)

**Milestone Goal:** Wire existing file services into the UI and add a Talents/Spells text area so the app is fully functional for session play without manual file-path workarounds.

- [ ] **Phase 4: File Menu** — Save, load, and import via File menu items
- [ ] **Phase 5: Talents Editor** — Talents/Spells free-text area above Notes

## Phase Details

### Phase 4: File Menu
**Goal**: Users can save, load, and import characters through the app's File menu without needing OS file manager workarounds
**Depends on**: Phase 3 (v1.0 complete)
**Requirements**: FILE-01, FILE-02, FILE-03
**Success Criteria** (what must be TRUE):
  1. User taps/clicks Save and a native file save dialog opens, producing a .sdchar file on disk
  2. User taps/clicks Load and a native file picker opens, replacing the current character with the loaded one
  3. User taps/clicks Import and a native file picker opens, populating the sheet from a Shadowdarklings JSON export
  4. All three File menu items are visible and functional on all four target platforms (iOS, macOS, Android, Windows)
**Plans:** 1/2 plans executed
Plans:
- [x] 04-01-PLAN.md — Wire SaveCommand, LoadCommand, ImportCommand into CharacterViewModel + AppShell
- [ ] 04-02-PLAN.md — Human verification of file operations on device

### Phase 5: Talents Editor
**Goal**: Users can record and edit their talents and spells directly on the Notes tab
**Depends on**: Phase 4
**Requirements**: TLNT-01
**Success Criteria** (what must be TRUE):
  1. Talents/Spells text area appears above the Notes editor on the Notes tab
  2. Text entered in the Talents/Spells area persists through save/load round-trips
  3. Both Talents/Spells and Notes areas are independently editable and scrollable
**Plans**: TBD

## Progress

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1. Foundation | v1.0 | 5/5 | Complete | 2026-03-14 |
| 2. Core Sheet | v1.0 | 6/6 | Complete | 2026-03-21 |
| 3. Export | v1.0 | 2/2 | Complete | 2026-03-21 |
| 4. File Menu | v1.1 | 1/2 | In Progress|  |
| 5. Talents Editor | v1.1 | 0/? | Not started | - |
