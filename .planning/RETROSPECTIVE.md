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

## Cross-Milestone Trends

### Process Evolution

| Milestone | Phases | Plans | Key Change |
|-----------|--------|-------|------------|
| v1.0 | 3 | 13 | First milestone — baseline established |
| v1.1 | 2 | 3 | Inline implementation + test-gap closure pattern; human verification as first-class plan |

### Cumulative Quality

| Milestone | Tests | Notes |
|-----------|-------|-------|
| v1.0 | 27+ | Core domain + ViewModel contracts + MarkdownBuilder covered |
| v1.1 | 54 | +8 tests: file commands (unit), Talents save/load + export (integration) |

### Top Lessons (Verified Across Milestones)

1. Build domain logic bottom-up before UI — prevents ViewModel rework
2. TDD for pure/stateless logic pays off immediately at integration time
3. Human verification must be a first-class plan step, not deferred — made explicit in v1.1
4. Close test gaps on inline implementations in the very next plan, not next milestone
