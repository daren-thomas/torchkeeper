# Phase 6: Gear & Stats Polish - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-28
**Phase:** 06-gear-stats-polish
**Areas discussed:** Free-carry item mechanism, Stat drill-down display, Talents import decision, Export parity verification

---

## Todo Fold

| Todo | Area | Score | Folded |
|------|------|-------|--------|
| Drill down base stat — include base stat in drill down | ui | 0.9 | ✓ |
| Fix coins and slots discrepancies | ui | 0.9 | ✓ |
| Check Shadowdark rules for backpack gear slots and free-carry items | general | 0.6 | ✓ |
| Check Talents/Shadowdarklings export format — should spells appear in Talents area? | general | 0.6 | ✓ |
| Test coin slots match between export and character sheet | testing | 0.6 | (covered by D-09) |

---

## Free-Carry Item Mechanism

| Option | Description | Selected |
|--------|-------------|----------|
| Hardcoded name list | Match known names: Backpack, Bag of Coins, Thieves Tools. No UI changes. | |
| User-settable checkbox | Add a 'free carry' toggle to GearItemPopup. More flexible. | |
| Hybrid: list + checkbox | Auto-mark known names, also let users override/add their own. | ✓ |

**User's choice:** Hybrid — auto-detect known names + user-settable checkbox

Follow-up — checkbox label:

| Option | Selected |
|--------|----------|
| Free Carry | ✓ |
| No Slots | |
| Slotless | |

Follow-up — display:

| Option | Selected |
|--------|----------|
| Same list, show 0 slots | |
| Same list, labeled 'Free' | |
| Separate section in gear list | ✓ |

Follow-up — section position:

| Option | Selected |
|--------|----------|
| Below regular gear | ✓ |
| Above regular gear | |

Follow-up — Markdown export:

| Option | Selected |
|--------|----------|
| Separate section in export | ✓ |
| Inline, showing 0 slots | |

---

## Stat Drill-Down Display

| Option | Selected |
|--------|----------|
| In the expanded panel only | ✓ |
| Always visible in row header | |

**User's choice:** Expanded panel only. Collapsed row stays clean.

Follow-up — label format:

| Option | Selected |
|--------|----------|
| Base: 14 | ✓ |
| STR (base): 14 | |
| You decide | |

---

## Talents Import Decision

| Option | Selected |
|--------|----------|
| Yes — import Talents from JSON | ✓ |
| No — leave Talents manual-only | |

Follow-up — mapping:

| Option | Selected |
|--------|----------|
| Talents → level talents, Spells → spellsKnown | ✓ |
| Both into Talents, clear Spells | |

**Notes:** User confirmed "go with one" (option 1). Also directed review of all example JSON files in `./examples/` folder.

Follow-up — scope of talent data:

| Option | Selected |
|--------|----------|
| Level talent rolls only | ✓ |
| Level rolls + class/ancestry abilities | |
| You decide | |

**Notes:** User asked "talents might also come from species and stuff, right?" — investigated bonuses array. Ancestry/class abilities that affect stats are already imported via the bonuses system. Level talent roll descriptions are the right data for the Talents text field.

Follow-up — format:

| Option | Selected |
|--------|----------|
| Lv1: [desc] per line | ✓ |
| Plain list, no level label | |
| You decide | |

---

## Export Parity Verification

| Option | Selected |
|--------|----------|
| Yes — add the test | ✓ |
| No — trust the shared property | |

**Notes:** Export already shares `vm.GearSlotsUsed` and `vm.CoinSlots` — parity is automatic once slot calc is fixed. Test added as a safeguard against future divergence.

---

## Claude's Discretion

- Whether `Rolled12ChosenTalentDesc` should be included in Talents import
- Whether free-carry name matching is case-insensitive
- Exact Base stat row position in expanded panel when no bonuses exist

## Deferred Ideas

None surfaced during discussion.
