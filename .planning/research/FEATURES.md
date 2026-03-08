# Feature Landscape

**Domain:** TTRPG character sheet app — Shadowdark RPG, play management focus
**Researched:** 2026-03-08
**Confidence:** MEDIUM (web tools unavailable; based on PROJECT.md, Brim.json data model analysis, and domain knowledge of TTRPG apps and Shadowdark mechanics)

---

## Table Stakes

Features users expect from any digital character sheet. Missing = product feels incomplete or broken at the table.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Display all 6 core stats (STR/DEX/CON/INT/WIS/CHA) | Every TTRPG app shows this | Low | With modifier (+/-) displayed prominently |
| HP tracking (current / max) | Session critical — damage happens constantly | Low | Tap to increment/decrement; not just a text field |
| Armor Class display | Looked up on every attack against character | Low | Derived from gear + DEX mod (Shadowdark: light armor = 11+DEX, medium = 13+DEX, heavy = 15) |
| Character identity fields | Name, class, ancestry, level, title, alignment | Low | Read often, edited rarely |
| Gear inventory list | Core play loop — tracking what you carry | Medium | Names, quantities, slot counts |
| Gear slot count (used / total) | Shadowdark-critical — slot limit governs survival decisions | Medium | Total = STR score; displayed as e.g. "8/9" |
| Attack display | Looked up on every combat turn | Medium | Weapon name, attack bonus, damage die |
| Currency tracking (GP/SP/CP) | Every session involves loot and spending | Low | Three denominations; coins take slots |
| Save / load character | Persistence between sessions | Medium | File-based; app-defined format |
| Freeform notes | Players always need somewhere to write things | Low | Simple text area |
| Spells known display | Casters need this at a glance | Low | Shadowdark scope: text field only; not spell cards |
| Languages display | Occasionally referenced | Low | Text field |

---

## Shadowdark-Specific Table Stakes

These are specific to Shadowdark mechanics. Players of this system expect them; apps without them feel like generic sheet fillers.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Gear slot total = STR score | Core Shadowdark rule — not STR modifier, the full score | Low | Auto-derive from STR stat when STR changes |
| Coin slot calculation | 1 slot per 20 coins (gp+sp+cp combined) | Low | Coins occupy real inventory slots; easy to forget without automation |
| Bulky item slot cost (2 slots) | Players need to know bulky = 2 | Low | Item-level flag; display as "2 slots" |
| Bundled items display | Arrows (20 = 1 slot), torches, rations | Low | Quantity/bundle shown together with single slot cost |
| Per-level talent record | Each level-up involves a talent table roll — players want a log | Medium | See Brim.json: `levels[]` array with talentRolledName, talentRolledDesc per level |
| HP roll per level display | HP = CON mod + per-level die roll; Stout ancestry adds extra roll | Low | Brim.json: `HitPointRoll` and `stoutHitPointRoll` per level entry |
| Stat bonus drill-down | STR/DEX etc. can have multiple named bonus sources | Medium | Brim.json: `bonuses[]` with sourceType (Class/Ancestry/Item), sourceCategory (Talent/Feature), bonusTo (e.g. "DEX:+2") |
| Magic items separate from mundane gear | Shadowdark distinguishes these; found items have unique properties | Medium | Brim.json: `magicItems[]` separate from `gear[]`; magic items have benefits, curses, personality fields |
| Class-derived title | Shadowdark has per-class level titles (e.g. Thief level 4 = "Cutthroat") | Low | Display only; user hand-enters |
| XP tracking | XP-to-level system; 10 XP per level | Low | Simple counter; milestone not relevant here |
| Background display | Chosen at creation; referenced for rulings | Low | Text field |
| Deity display | Clerics and others reference this | Low | Text field |
| Attack bonus derivation | Melee = STR mod + class attack bonus; ranged = DEX mod; Finesse = higher of STR/DEX | High | Brim.json `attacks[]` array shows this complexity: "DAGGER: +3 (N), 1d4 (FIN)" — near/far range indicators, finesse flag |

---

## Differentiators

Features that go beyond what Shadowdarklings.net provides and set this app apart. Not universally expected, but highly valued by players who try them.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Bonus source toggle (on/off) | Equipped items, situational talents, spell effects change situationally; player needs to see "what if I unequip this?" | High | Per-bonus enable/disable; recalculates derived stats live. PROJECT.md explicitly calls this out. |
| Stat total breakdown (drill-down) | "Why is my STR 11?" — shows base + each named bonus | Medium | Shadowdarklings shows final values only; no transparency |
| Slot-by-slot inventory visualization | Seeing 8/9 slots filled as a visual bar or grid is more intuitive than a number | Medium | Could be simple progress bar or drag-reorder list |
| Formatted Markdown export | Print-ready reference sheet; shareable with DM | Medium | PROJECT.md requirement; Shadowdarklings has no export |
| Per-level advancement history | Full log of what was chosen at each level (talent name + description + HP roll) | Low | Brim.json has this data; just needs a readable display |
| Magic item detail view | Benefits, curses, and personality traits are rich data — expose them fully | Low | Brim.json has full magic item fields; just needs a display |
| Shadowdarklings JSON import | Bootstrap from existing character — zero re-entry for existing players | Medium | One-way import; data mapping from Brim.json format to app format |
| Currency ledger | Brim.json has a full `ledger[]` of every transaction — displaying this history lets players audit their gold | Medium | Shadowdarklings tracks this internally; exposing it is a differentiator |
| Toggle bonus source categories | Situational: e.g. "apply poison bonus" as toggle so attack shows correct value only when poisoned | High | Requires UX thinking — which bonuses are toggleable vs always-on |
| Coin slot auto-calculation | Auto-computes slots consumed by coins across GP/SP/CP; updates slot total live | Low-Med | Users always forget this rule; automation removes friction |

---

## Anti-Features

Features to explicitly NOT build. Building these would waste scope, add complexity, or undermine the play-management positioning.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| Character builder / guided creation | Shadowdarklings.net does this well; this app is for play management | Import from Shadowdarklings JSON; hand-entry for the few fields needed |
| Automated talent table application | Requires embedding full rulebook data; maintenance burden; out of scope | User records what they rolled and chose; app stores the text description |
| Ancestry/class bonus auto-apply | Same problem as talent tables; creates dependency on embedded rules data | User enters base stats and records bonuses from their sheet manually |
| Online sync / cloud saves | Adds auth, server, security surface; not the value prop here | File-based saves; OS file sharing (iCloud Drive, etc.) handles sync |
| Spell cards / spell management | Shadowdark casters use simple spell lists; full spell management is a separate product | Single text field for spells known; notes field for spell details |
| Multi-character roster UI | Complicates file management, adds nav complexity | File-based; OS "Open File" is the roster |
| Shadowdarklings-compatible JSON export | Re-importing back to Shadowdarklings has no clear user value | Import only; app's native format is not required to round-trip |
| Dice roller | Every TTRPG app has one; physical dice are preferred at the table by most OSR players; adds scope for little differentiation | Leave out or treat as explicit out-of-scope |
| Combat tracker / initiative | Session management product, not character sheet | Out of scope |
| DM tools | This is a player-facing character sheet | Out of scope |
| Condition tracking | Shadowdark has minimal formal conditions; too rules-specific to implement cleanly | Notes field handles this |

---

## Feature Dependencies

```
Gear slot total display → STR stat value (gear slots = STR score)
Coin slot calculation → Currency totals (GP + SP + CP)
Slot used total → Gear list (sum of item slots) + Coin slots
Attack display → Weapon type + STR/DEX modifier + class attack bonus (complex derivation)
Bonus toggle → Stat breakdown display (toggling a bonus reruns the drill-down)
Stat breakdown → Bonuses list with sourceType/sourceCategory/bonusTo
HP max → Per-level HP rolls + CON modifier (sum of HitPointRoll values + CON mod per level)
Shadowdarklings import → All other fields (import populates everything)
```

---

## MVP Recommendation

Based on the PROJECT.md requirements and the play-management positioning, the MVP must include all Table Stakes features plus the highest-value Shadowdark differentiators. The product fails without them.

**Prioritize for MVP:**

1. Character identity display (name, class, ancestry, level, title, alignment, background, deity, languages)
2. 6 core stats with modifiers and bonus breakdown per stat
3. HP tracking with tap-to-adjust
4. Gear inventory with per-item slot display and slot total (used/max)
5. Shadowdark slot rules: total = STR score, coins = 1 per 20, bulky = 2 slots
6. Magic items separate from mundane gear with full property display
7. Currency tracking (GP/SP/CP) with coin slot auto-calc
8. Attack display derived from gear + stats
9. Shadowdarklings JSON import
10. Native file save/load
11. Freeform notes
12. Formatted Markdown export

**Defer post-MVP:**

- Bonus source toggles: High complexity; validate that players actually use situational toggles before building
- Currency ledger display: The data comes from import; displaying it is low complexity but low urgency
- Slot visualization (progress bar): Nice to have; text "8/9" is functional

---

## Sources

- PROJECT.md — project requirements, explicit scope and anti-features (HIGH confidence)
- examples/Brim.json — actual Shadowdarklings.net export format; reveals data model for stats, bonuses, gear, magic items, levels, attacks, ledger (HIGH confidence)
- Shadowdark RPG domain knowledge — gear slot rules (STR score), coin slots (1/20), bulky items (2 slots), talent table per level, HP roll per level, Stout ancestry extra roll, class titles, XP system (MEDIUM confidence — based on training data; verify against rulebook PDF)
- TTRPG character sheet app patterns — table stakes derived from common digital sheet apps (Roll20, DnDBeyond, Pathbuilder) applied to OSR context (MEDIUM confidence — web research unavailable; based on training data)
