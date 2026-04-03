# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v1.0 — MVP

**Shipped:** 2026-03-21
**Phases:** 3 | **Plans:** 13 | **Timeline:** 13 days (2026-03-08 → 2026-03-21)

### What Was Built

- Full .NET MAUI character sheet app for Shadowdark RPG targeting all 4 platforms (iOS/macOS/Android/Windows)
- 3-tab UI: Sheet (identity, stats with bonus breakdowns, attacks, HP/XP), Gear (slot counter, gear list, currency), Notes
- Shadowdarklings.net JSON import pipeline with RolledStats base extraction and full bonus pass-through
- Native .sdchar file format with versioned DTO serialization, round-trip verified
- Markdown export with platform-appropriate delivery (share sheet / save-as dialog)
- 27+ xUnit tests covering domain logic, ViewModel contracts, and MarkdownBuilder formatting rules

### What Worked

- **Bottom-up phase ordering** — building the domain model (Phase 1) before any UI (Phase 2) prevented rework; no ViewModel had to work around a broken service layer
- **TDD for pure logic** — MarkdownBuilder implemented test-first (RED → GREEN) with 18 tests; caught formatting edge cases early, zero rework at integration time
- **Two-DTO architecture** — separating CharacterSaveData from ShadowdarklingsJson kept the import path isolated from the save path; no coupling issues
- **Plan-execute-summarize cadence** — each plan's SUMMARY.md one-liner made progress visible and made this retrospective easy to write
- **Gap closure as a planned phase step** — 01-05 was explicitly a gap-closure plan; acknowledging compile errors as a discrete task rather than ad-hoc fixing kept phase state clean

### What Was Inefficient

- **ROADMAP.md plan checkbox drift** — Phase 2 plan checkboxes in ROADMAP.md were never updated to `[x]` after execution; gsd-tools summary counts were authoritative but the roadmap looked incomplete. Should update as plans complete.
- **Coin encumbrance bug** — initial implementation used floor division instead of ceiling; caught during Phase 3 wiring (not Phase 2 testing). A GEAR-04 unit test covering the boundary condition would have caught this earlier.
- **Human verification items deferred** — Phase 3 VERIFICATION.md has 8 human_needed items that weren't signed off before milestone completion. Build verification and basic E2E smoke tests should be confirmed during the phase, not deferred.

### Patterns Established

- Pure service layer in `SdCharacterSheet.Core` — no MAUI dependencies, fully unit-testable
- `NullFileSaver` test double pattern — lightweight interface stub for platform services in unit tests
- `StatRowViewModel` with `BonusSources` collection — each stat row owns its own expansion state
- `GearItemViewModel` wrapping both `GearItem` and `MagicItem` — unified gear list with type discrimination
- MarkdownBuilder as pure static class — accepts DTO, returns string, no side effects

### Key Lessons

1. **Verify coin math at the boundary** — Shadowdark encumbrance rules have edge cases (first N free, then per-N rounding) that need explicit boundary tests, not just happy-path coverage
2. **Keep plan checkboxes in ROADMAP.md current** — stale checkboxes create confusion when using `roadmap analyze`; update them immediately when a SUMMARY.md is written
3. **Human verification before milestone close** — don't defer "does the build pass / does the export work" to milestone completion; it should be a hard gate before marking a phase done
4. **RolledStats not FinalStats for base** — Shadowdarklings exports have both; using FinalStats would double-count bonuses. Document this in the import service as a prominent comment.

### Cost Observations

- Model mix: balanced profile (sonnet executor, opus planner)
- Sessions: ~5 estimated
- Notable: TDD plans (01-02, 03-01) were among the cleanest executions — no rework, clear RED/GREEN/REFACTOR structure

---

## Milestone: v1.1 — File Management & Talents

**Shipped:** 2026-03-22
**Phases:** 2 | **Plans:** 3 | **Timeline:** 2 days (2026-03-21 → 2026-03-22)

### What Was Built

- SaveCommand/LoadCommand/ImportCommand wired into CharacterViewModel with MAUI file pickers and AppShell menu items
- MacFilePickerHelper workaround for MAUI FilePicker null on macOS 15 Sequoia (UIDocumentPickerViewController direct)
- Talents/Spells free-text area through full stack — Notes tab now shows Talents → Spells → Notes sections
- 54 xUnit tests passing (net +8 from v1.0)

### What Worked

- **Inline implementation for simple features** — Talents was implemented in a single commit (4239483) outside the formal plan cycle; this was the right call for a simple field addition. Phase 05 then closed the test gap cleanly.
- **Human verification as an explicit plan** — 04-02 was a dedicated human-verification plan; making this a first-class plan step prevented the "deferred verification" problem from v1.0
- **Platform workaround discovery early** — MacFilePickerHelper fix discovered and committed within the same phase; not deferred to a later milestone

### What Was Inefficient

- **VALIDATION.md sign-off checklists not completed** — both Phase 04 and 05 VALIDATION.md files have unchecked sign-off checkboxes (Nyquist compliance not marked). The code works but the audit trail is incomplete.
- **TestFileCommandVM stub omits Talents** — the 6 file command unit tests provide no cross-phase regression safety for the TLNT-01 + FILE-01/02 save/load path. This is a known tech debt item from the audit.

### Patterns Established

- `MauiImportFileService` pattern: MAUI file picker + Core service delegation (mirrors `MauiCharacterFileService`)
- `BuildCharacterFromViewModel()` as the canonical save snapshot builder — backing field is always stale after edits
- `GearItemSource` enum as type discriminator for gear serialization split
- `MacFilePickerHelper` for UIDocumentPickerViewController direct invocation on macOS Catalyst

### Key Lessons

1. **Inline implementation is valid when scope is clear** — but close the test gap in the very next plan, not next milestone
2. **VALIDATION.md sign-off is a paper audit trail** — the actual verification happened; the paperwork didn't. Either tick the boxes or don't create the file. Dangling checklists degrade the audit signal.
3. **Platform quirks need immediate workarounds in the same phase** — MAUI FilePicker on macOS 15 was caught and fixed in Phase 04; deferring it would have blocked all load/import functionality

### Cost Observations

- Model mix: balanced profile (sonnet executor, opus planner)
- Sessions: ~3 estimated
- Notable: Milestone completed in 2 days — short scope, focused execution

---

## Milestone: v1.2 — Gear & Stats Polish

**Shipped:** 2026-04-03
**Phases:** 2 | **Plans:** 4 | **Timeline:** 2 days (2026-03-28 → 2026-03-29)

### What Was Built

- IsFreeCarry flag through full Core layer with auto-detect (Backpack, Bag of Coins, Thieves Tools) and manual checkbox in GearItemPopup
- GearPage split into "Regular Gear" + "Free Carry" sections with live slot exclusion (free-carry items don't count toward gear slots)
- Markdown export mirrors UI layout: separate `### Free Carry` section; 5 new unit tests verify GEAR-01/GEAR-02 parity (67 tests total)
- Stat drill-down expanded panel now shows "Base: N" as first row above bonus sources
- Shadowdarklings import now populates Talents field from `levels[].talentRolledDesc` (Phase 6)
- MAUI-local shadow types (GearItem, MagicItem, GearItemData, MagicItemData, CharacterFileService) patched to propagate IsFreeCarry — closes GEAR-01 save/load data loss (Phase 7)

### What Worked

- **Milestone audit as gap detector** — `gsd:audit-milestone` identified the MAUI-local shadow type gap before shipping. Without the audit, Phase 6 would have shipped with a silent data-loss bug (manually-flagged free-carry items lose their flag on save/load).
- **Phase 7 as a targeted gap-closure plan** — the fix was 3 minutes of execution: 4 files, 8 property additions. Having a dedicated phase with clear success criteria made the fix clean and verifiable.
- **CS0436 shadow type pattern documented** — the audit forced documentation of the architectural fact that Core changes must be manually propagated to MAUI-local copies. This is now in PROJECT.md and STATE.md context.
- **Export parity unit tests** — 06-02 added explicit GEAR-01/GEAR-02 unit tests. These provide regression protection if the export pipeline changes.

### What Was Inefficient

- **Phase 6 scope missed MAUI-local types** — the plan executor applied IsFreeCarry to Core correctly but didn't check whether MAUI-local shadow files also needed updates. The CS0436 architectural pattern was not visible in the plan context. The audit caught it, but a better plan would have included a `grep -r IsFreeCarry SdCharacterSheet/` check step.
- **Xcode toolchain noise** — MAUI `dotnet build` fails on this machine due to unrelated Xcode plugin/actool errors. This masked whether C# compilation succeeded or failed during Phase 6 plan execution. The workaround (check for `error CS` lines separately) is effective but adds friction to every build verification.
- **06-02 and 06-03 SUMMARY.md frontmatter missing `requirements-completed`** — GEAR-02 and STAT-01 weren't declared in plan frontmatter, only GEAR-01 was. The `gsd-tools summary-extract` then couldn't find one-liners for those plans. Minor paperwork gap, but it broke the automated MILESTONES.md generation.

### Patterns Established

- **Audit-then-gap-phase pattern** — run `gsd:audit-milestone`, let it find gaps, add a targeted gap-closure phase, ship. Fast and clean.
- **MAUI-local shadow propagation checklist** — whenever a Core model/DTO changes, check `SdCharacterSheet/Models/`, `SdCharacterSheet/DTOs/`, `SdCharacterSheet/Services/` for shadow copies that need the same change.
- **Free Carry frame always visible** — empty `BindableLayout` renders nothing; no need for a visibility converter on sections that may be empty.

### Key Lessons

1. **Audit before archive, not after** — the audit is most valuable when there's still time to fix gaps. Running it as a hard pre-requisite to milestone completion is the right workflow.
2. **Shadow types need explicit plan steps** — for any architecture with CS0436 shadow copies, include a "propagate to MAUI-local" step explicitly in the plan. Don't rely on the executor to know about the duplication.
3. **Always declare `requirements-completed` in SUMMARY.md frontmatter** — even for "obvious" requirements. The automation depends on it.
4. **3-minute gap closures are worth a full phase** — Phase 7 was tiny, but having a proper PLAN.md + SUMMARY.md means the fix is traceable, the gap is formally closed, and the audit trail is clean.

### Cost Observations

- Model mix: balanced profile (sonnet executor, opus planner)
- Sessions: ~2 estimated
- Notable: Entire milestone executed in 2 days; Phase 7 gap-closure was 3 minutes of actual execution time

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Phases | Plans | Key Change |
|-----------|--------|-------|------------|
| v1.0 | 3 | 13 | First milestone — baseline established |
| v1.1 | 2 | 3 | Inline implementation + test-gap closure pattern; human verification as first-class plan |
| v1.2 | 2 | 4 | Audit-then-gap-phase pattern; shadow type propagation discipline established |

### Cumulative Quality

| Milestone | Tests | Notes |
|-----------|-------|-------|
| v1.0 | 27+ | Core domain + ViewModel contracts + MarkdownBuilder covered |
| v1.1 | 54 | +8 tests: file commands (unit), Talents save/load + export (integration) |
| v1.2 | 67 | +13 tests: GEAR-01/GEAR-02 slot exclusion and export parity |

### Top Lessons (Verified Across Milestones)

1. Build domain logic bottom-up before UI — prevents ViewModel rework
2. TDD for pure/stateless logic pays off immediately at integration time
3. Human verification must be a first-class plan step, not deferred — made explicit in v1.1
4. Close test gaps on inline implementations in the very next plan, not next milestone
5. Run `gsd:audit-milestone` before archiving — it catches cross-layer gaps the executor can miss (confirmed v1.2: MAUI shadow type data loss caught and fixed)
6. Architecture with duplicate shadow types requires explicit propagation steps in every plan that changes a model
