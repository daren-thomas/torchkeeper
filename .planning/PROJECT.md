# Shadowdark Character Sheet

## What This Is

A .NET MAUI cross-platform character sheet app for Shadowdark RPG, designed for play management rather than character creation. Users hand-enter their stats, talents, and gear, track HP/XP during sessions, and manage inventory with full slot visibility. Supports one-way import from Shadowdarklings.net JSON exports and Markdown export for offline reference.

## Core Value

A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.

## Requirements

### Validated

- ✓ User can import a Shadowdarklings.net JSON export to bootstrap a character — v1.0
- ✓ User can load a character from a native .sdchar file — v1.0
- ✓ User can save a character to a .sdchar file — v1.0
- ✓ User can view and edit character identity fields (name, class, ancestry, level, title, alignment, background, deity, languages) — v1.0
- ✓ User can track XP — v1.0
- ✓ User can view and edit the 6 core stats with automatic modifier display — v1.0
- ✓ User can see a breakdown of all bonus sources contributing to each stat total — v1.0
- ✓ User can track current HP with increment/decrement controls — v1.0
- ✓ User can set maximum HP — v1.0
- ✓ User can add, edit, and remove gear items (mundane and magic) — v1.0
- ✓ Each gear item has name, slot count, type, and optional free-text note — v1.0
- ✓ App displays total gear slots used vs total available (slots = max(STR score, 10)) — v1.0
- ✓ App auto-calculates coin slots consumed by GP/SP/CP totals (first 100 coins free, ceiling per 100 after) — v1.0
- ✓ User can track GP, SP, and CP totals — v1.0
- ✓ User can maintain an editable list of free-form attack entries — v1.0
- ✓ User can write and edit freeform notes on the character — v1.0
- ✓ User can export the full character sheet as formatted Markdown for print/reference — v1.0

### Active

- [ ] User can toggle individual bonus sources on/off per stat (BNUS-01, BNUS-02)
- [ ] User can view full GP/SP/CP transaction history (LEDG-01, LEDG-02)
- [ ] Gear slot utilization shown as visual progress bar (PLSH-01)
- [ ] Character sheet auto-saves on every change (PLSH-02)

### Out of Scope

- Character builder (automated talent tables, ancestry bonuses auto-applied) — user enters choices manually
- Online sync or cloud saves — file-based only
- Multi-character roster UI — character switching handled via OS file management
- Shadowdarklings-compatible JSON export — import only
- Spell management beyond noting spells known as text
- Dice roller — not a character sheet concern
- Automatic attack derivation from weapon stats — attacks are free-form text

## Context

**Shipped v1.0 with ~3,756 LOC (C# + XAML), 134 files, 69 commits over 13 days.**

Tech stack: .NET 10 MAUI, CommunityToolkit.Mvvm, System.Text.Json, xUnit.

Architecture:
- `SdCharacterSheet.Core`: domain models, DTOs, services, MarkdownBuilder (pure/testable)
- `SdCharacterSheet`: MAUI app — ViewModels, Views (3-tab: Sheet/Gear/Notes), platform wiring
- `SdCharacterSheet.Tests`: xUnit test project (27+ tests passing)

Key facts:
- Shadowdarklings.net JSON format understood (`examples/Brim.json`): stats have rolled + final values, bonuses are named with sourceType/sourceCategory/bonusTo
- Gear slots = STR score total; items are regular (1 slot), bulky (2 slots), or bundled
- Coin encumbrance: first 100 coins of any denomination free, then ceiling(coins/100) slots per type
- Shadowdark rulebook PDF at `~/Downloads/Shadowdark_RPG_-_V4-9.pdf` for mechanical reference
- .sdchar format is JSON with version field, using CharacterSaveData DTO (not ViewModel)
- Export: MarkdownExportService routes to share sheet (mobile) / save-as dialog (macOS/Windows)

Known gaps (v1.0, human verification pending):
- Phase 3 VERIFICATION.md has 8 human_needed items (export E2E, test pass confirmation)

## Constraints

- **Tech stack**: .NET MAUI — targets Windows, macOS (Mac Catalyst), iOS, Android
- **File format**: Native save format is .sdchar (JSON, app-defined)
- **No character builder**: No automated rule lookups or guided creation — manual data entry only

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| .NET MAUI over web | Cross-platform native feel, user's choice | ✓ Good — all 4 platforms supported |
| Native .sdchar format separate from Shadowdarklings JSON | Import-only; native format optimized for app needs | ✓ Good — clean separation, DTO round-trip verified |
| Two-DTO architecture (CharacterSaveData + ShadowdarklingsJson) | Avoid coupling save format to import format | ✓ Good — kept domain model stable |
| RolledStats for base stats (not FinalStats) | Final values include bonuses; rolled values are the true base | ✓ Good — bonus pass-through works correctly |
| CommunityToolkit.Mvvm for observable properties | Reduces boilerplate vs manual INotifyPropertyChanged | ✓ Good — clean ViewModel code |
| MarkdownBuilder as pure static methods | Enables unit testing without DI or platform dependencies | ✓ Good — 18 tests pass |
| Coin encumbrance: ceiling(coins/100) with first 100 free | Matches Shadowdark rules; corrected during Phase 3 | ✓ Good — bug caught and fixed (coin bug: first impl used floor) |

---
*Last updated: 2026-03-21 after v1.0 milestone*
