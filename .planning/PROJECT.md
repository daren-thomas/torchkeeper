# Shadowdark Character Sheet

## What This Is

A .NET MAUI cross-platform character sheet app for Shadowdark RPG, designed for play management rather than character creation. Users hand-enter their stats, talents, and gear, track HP/XP during sessions, and manage inventory with full slot visibility. Supports one-way import from Shadowdarklings.net JSON exports.

## Core Value

A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] User can import a Shadowdarklings.net JSON export to bootstrap a character
- [ ] User can load and save a character in a native app file format
- [ ] User can hand-edit all character fields: name, ancestry, class, level, title, alignment, background, deity, languages
- [ ] User can view and edit the 6 core stats (STR/DEX/CON/INT/WIS/CHA) with modifier display
- [ ] User can see how each stat total is calculated (base + per-source bonuses with drill-down)
- [ ] User can toggle individual bonus sources on/off (equipped items, situational talents, spell effects)
- [ ] User can track current and max HP
- [ ] User can track XP and level
- [ ] User can manage gear inventory with slot counts (items, slots used vs total)
- [ ] User can add, edit, and remove gear items with custom notes per item
- [ ] User can manage magic items separately from mundane gear
- [ ] User can track gold, silver, and copper
- [ ] User can record level advancement notes (what talent was chosen, what stat was bumped, etc.)
- [ ] User can write freeform notes on the character
- [ ] User can export the full character sheet as formatted Markdown for print/reference
- [ ] App displays calculated attack values based on stats and gear

### Out of Scope

- Character builder (automated talent tables, ancestry bonuses auto-applied) — user enters choices manually
- Online sync or cloud saves — file-based only
- Multi-character roster UI — character switching handled via OS file management
- Shadowdarklings-compatible JSON export — import only
- Spell management beyond noting spells known as text

## Context

- Shadowdarklings.net JSON format is understood (see `examples/Brim.json`): stats include both rolled and final values, bonuses are named with sourceType/sourceCategory/bonusTo, gear has typed slots, magic items are separate from mundane gear
- Gear slots in Shadowdark = STR score total; items are regular (1 slot), bulky (2 slots), or bundled
- The Shadowdark rulebook PDF is available at ~/Downloads/Shadowdark_RPG_-_V4-9.pdf for mechanical reference
- Coins occupy 1 slot per 20 coins in Shadowdark

## Constraints

- **Tech stack**: .NET MAUI — targets Windows, macOS (Mac Catalyst), iOS, Android
- **File format**: Native save format is app-defined (not required to match Shadowdarklings JSON)
- **No character builder**: No automated rule lookups or guided creation — manual data entry only

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| .NET MAUI over web | Cross-platform native feel, user's choice | — Pending |
| Native file format separate from Shadowdarklings JSON | Import-only; native format can be optimized for app needs | — Pending |

---
*Last updated: 2026-03-08 after initialization*
