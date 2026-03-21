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

## Cross-Milestone Trends

### Process Evolution

| Milestone | Phases | Plans | Key Change |
|-----------|--------|-------|------------|
| v1.0 | 3 | 13 | First milestone — baseline established |

### Cumulative Quality

| Milestone | Tests | Notes |
|-----------|-------|-------|
| v1.0 | 27+ | Core domain + ViewModel contracts + MarkdownBuilder covered |

### Top Lessons (Verified Across Milestones)

1. Build domain logic bottom-up before UI — prevents ViewModel rework
2. TDD for pure/stateless logic pays off immediately at integration time
