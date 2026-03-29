# Roadmap: Shadowdark Character Sheet

## Milestones

- ✅ **v1.0 MVP** — Phases 1–3 (shipped 2026-03-21)
- ✅ **v1.1 File Management & Talents** — Phases 4–5 (shipped 2026-03-22)
- 📋 **v1.2 Gear & Stats Polish** — Phase 6 (planned)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1–3) — SHIPPED 2026-03-21</summary>

- [x] Phase 1: Foundation (5/5 plans) — completed 2026-03-14
- [x] Phase 2: Core Sheet (6/6 plans) — completed 2026-03-21
- [x] Phase 3: Export (2/2 plans) — completed 2026-03-21

Full details: `.planning/milestones/v1.0-ROADMAP.md`

</details>

<details>
<summary>✅ v1.1 File Management & Talents (Phases 4–5) — SHIPPED 2026-03-22</summary>

- [x] Phase 4: File Menu (2/2 plans) — completed 2026-03-22
- [x] Phase 5: Talents Editor (1/1 plan) — completed 2026-03-22

Full details: `.planning/milestones/v1.1-ROADMAP.md`

</details>

### 📋 v1.2 Gear & Stats Polish (Planned)

**Milestone Goal:** Gear slot counting is fully rules-accurate per Shadowdark RPG, export and display are in sync, and the stat drill-down shows the full picture including the raw base stat.

- [x] **Phase 6: Gear & Stats Polish** — Fix slot accuracy, verify export parity, and add base stat to drill-down (completed 2026-03-28)
- [ ] **Phase 7: MAUI Layer IsFreeCarry Fix** — Propagate IsFreeCarry to MAUI-local models, DTOs, and service; close GEAR-01 save/load gap

## Phase Details

### Phase 6: Gear & Stats Polish
**Goal**: Gear slot calculation matches Shadowdark rules (backpack/free-carry items are slotless, coin weight is correct), the Markdown export reflects the same slot count as the UI, and the stat drill-down shows the base stat value alongside the modifier
**Depends on**: Phase 5
**Requirements**: GEAR-01, GEAR-02, STAT-01
**Success Criteria** (what must be TRUE):
  1. Items defined as free to carry (backpack, bag of coins, thieves tools) do not consume gear slots
  2. Coin weight slot calculation matches the Shadowdark RPG rules
  3. Exported Markdown gear section slot counts match what is shown in the character sheet UI
  4. Tapping a stat shows the raw base stat value (e.g. STR 14) as well as the modifier and bonus sources
**Plans**: 06-01 (Free-Carry Gear Slots), 06-02 (Export Parity + Tests), 06-03 (Stat Drill-Down + Talents Import)

### Phase 7: MAUI Layer IsFreeCarry Fix
**Goal:** Propagate `IsFreeCarry` to the MAUI-local model, DTO, and service layer so that free-carry status round-trips correctly through save/load in the running app. Closes the critical GEAR-01 gap where manually-flagged items silently lose their flag on reload.
**Depends on:** Phase 6
**Requirements:** GEAR-01
**Gap Closure:** Closes gaps from v1.2 audit (MILESTONE-AUDIT.md)
**Success Criteria:**
  1. `SdCharacterSheet/Models/GearItem.cs` and `MagicItem.cs` have `IsFreeCarry` property
  2. `SdCharacterSheet/DTOs/CharacterSaveData.cs` `GearItemData` and `MagicItemData` include `IsFreeCarry`
  3. `SdCharacterSheet/Services/CharacterFileService.cs` `MapToDto`/`MapFromDto` project `IsFreeCarry` correctly
  4. MAUI app compiles without CS0117 errors
  5. A manually-flagged free-carry item retains its flag after save and reload

## Progress

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1. Foundation | v1.0 | 5/5 | Complete | 2026-03-14 |
| 2. Core Sheet | v1.0 | 6/6 | Complete | 2026-03-21 |
| 3. Export | v1.0 | 2/2 | Complete | 2026-03-21 |
| 4. File Menu | v1.1 | 2/2 | Complete | 2026-03-22 |
| 5. Talents Editor | v1.1 | 1/1 | Complete | 2026-03-22 |
| 6. Gear & Stats Polish | v1.2 | 3/3 | Complete   | 2026-03-28 |
| 7. MAUI Layer IsFreeCarry Fix | v1.2 | 0/1 | Pending | — |
