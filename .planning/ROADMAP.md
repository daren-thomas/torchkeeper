# Roadmap: Shadowdark Character Sheet

## Overview

Three phases, bottom-up. Phase 1 lays the domain model, file format, and import pipeline — the riskiest work with no visible UI but the most consequential decisions. Phase 2 builds the full interactive character sheet on top of those stable services — everything a player needs at the table. Phase 3 adds Markdown export and validates the app across all four platform targets. Each phase delivers a coherent, verifiable capability; nothing is built on unstable ground.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Foundation** - Data model, native file I/O, and Shadowdarklings import pipeline (completed 2026-03-14)
- [ ] **Phase 2: Core Sheet** - All interactive character sheet tabs — stats, gear, identity, notes
- [ ] **Phase 3: Export** - Markdown export and cross-platform verification

## Phase Details

### Phase 1: Foundation
**Goal**: The app can load, save, and import a character with all data intact — no UI required
**Depends on**: Nothing (first phase)
**Requirements**: FILE-01, FILE-02, FILE-03
**Success Criteria** (what must be TRUE):
  1. A character imported from a Shadowdarklings JSON export produces a populated Character model with correct base stats, bonus sources, gear, and magic items
  2. A character can be saved to a .sdchar file and loaded back with no data loss
  3. The .sdchar format includes a version field and uses a separate DTO (not the ViewModel class) as the serialization target
  4. File picker works on all four platform targets (Windows, macOS, iOS, Android) using platform-appropriate file type declarations
**Plans**: 4 plans

Plans:
- [x] 01-01-PLAN.md — Scaffold MAUI solution, domain models, DTOs, ViewModel skeleton, xUnit test stubs
- [ ] 01-02-PLAN.md — Implement ShadowdarklingsImportService with 7 passing tests (FILE-01)
- [ ] 01-03-PLAN.md — Implement CharacterFileService save/load JSON layer with 2 passing tests (FILE-02, FILE-03)
- [ ] 01-04-PLAN.md — Android/Windows platform declarations, FilePicker/FileSaver wiring, DI registration

### Phase 2: Core Sheet
**Goal**: A player can open their character and manage everything needed at the table — stats with breakdowns, HP, gear slots, currency, attacks, and notes
**Depends on**: Phase 1
**Requirements**: IDNT-01, IDNT-02, STAT-01, STAT-02, HITP-01, HITP-02, GEAR-01, GEAR-02, GEAR-03, GEAR-04, CURR-01, ATCK-01, NOTE-01
**Success Criteria** (what must be TRUE):
  1. User can view and edit all identity fields (name, class, ancestry, level, title, alignment, background, deity, languages) and track XP
  2. User can view each of the 6 stats with its modifier and a drill-down showing every bonus source contributing to the total
  3. User can increment and decrement current HP and set maximum HP
  4. User can add, edit, and remove gear items; the slot counter updates live and reflects STR score as the slot total
  5. User can track GP, SP, and CP; coin slots are auto-calculated and included in the gear slot total
**Plans**: TBD

### Phase 3: Export
**Goal**: A player can export their full character sheet as formatted Markdown for print or offline reference
**Depends on**: Phase 2
**Requirements**: MRKD-01
**Success Criteria** (what must be TRUE):
  1. User can trigger a Markdown export that produces a complete, formatted character sheet including identity, stats with modifiers, gear list with slot counts, currency, attacks, and notes
  2. On mobile (iOS/Android) the export triggers the native share sheet; on desktop (Windows/macOS) a save-as dialog appears
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 4/4 | Complete   | 2026-03-14 |
| 2. Core Sheet | 0/TBD | Not started | - |
| 3. Export | 0/TBD | Not started | - |
