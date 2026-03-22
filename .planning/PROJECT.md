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

- ✓ User can save a character to a .sdchar file via the File menu (FILE-01) — v1.1
- ✓ User can load a character from a .sdchar file via the File menu (FILE-02) — v1.1
- ✓ User can import a Shadowdarklings.net JSON export via the File menu (FILE-03) — v1.1
- ✓ User can view and edit a Talents/Spells text area on the Notes tab (TLNT-01) — v1.1

### Out of Scope

- Character builder (automated talent tables, ancestry bonuses auto-applied) — user enters choices manually
- Online sync or cloud saves — file-based only
- Multi-character roster UI — character switching handled via OS file management
- Shadowdarklings-compatible JSON export — import only
- Spell management beyond free-text entry in Talents/Spells area
- Dice roller — not a character sheet concern
- Automatic attack derivation from weapon stats — attacks are free-form text

## Context

**Shipped v1.1 with ~3,426 LOC C# (+ XAML), 54 xUnit tests passing, 2 milestones over 15 days.**

Tech stack: .NET 10 MAUI, CommunityToolkit.Mvvm, System.Text.Json, xUnit.

Architecture:
- `SdCharacterSheet.Core`: domain models, DTOs, services, MarkdownBuilder (pure/testable)
- `SdCharacterSheet`: MAUI app — ViewModels, Views (3-tab: Sheet/Gear/Notes), platform wiring
- `SdCharacterSheet.Tests`: xUnit test project (54 tests passing)

Key facts:
- Shadowdarklings.net JSON format understood (`examples/Brim.json`): stats have rolled + final values, bonuses are named with sourceType/sourceCategory/bonusTo
- Gear slots = STR score total; items are regular (1 slot), bulky (2 slots), or bundled
- Coin encumbrance: first 100 coins of any denomination free, then ceiling(coins/100) slots per type
- Shadowdark rulebook PDF at `~/Downloads/Shadowdark_RPG_-_V4-9.pdf` for mechanical reference
- .sdchar format is JSON with version field, using CharacterSaveData DTO (not ViewModel)
- Export: MarkdownExportService routes to share sheet (mobile) / save-as dialog (macOS/Windows)
- File menu: Save/Load/Import in AppShell MenuBarItems (desktop) and ToolbarItems overflow (mobile)
- MacFilePickerHelper (commit b2d9977): workaround for MAUI FilePicker null on macOS 15 Sequoia
- Talents field: full-stack free-text area on Notes tab (above Spells, above Notes), exported as `## Talents` section in Markdown

Known gaps (tech debt, not blocking):
- VALIDATION.md Nyquist sign-off checklists not ticked for Phase 04 and 05
- TestFileCommandVM stub omits Talents — FILE-01/02 + TLNT-01 cross-path not covered in unit tests
- MacFilePickerHelper requires re-validation if targeting iOS natively or new macOS versions

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
| MauiImportFileService pattern | MAUI file picker + Core service delegation, mirrors MauiCharacterFileService | ✓ Good — consistent platform service pattern |
| MacFilePickerHelper (UIDocumentPickerViewController direct) | MAUI FilePicker.Default.PickAsync() returns null on macOS 15 Sequoia | ✓ Good — workaround works; needs re-check on iOS native |
| BuildCharacterFromViewModel() on save | Backing field is stale after edits; observable properties are authoritative | ✓ Good — correct data saved |
| GearItemSource enum as canonical discriminator | Type-safe split of GearItem vs MagicItem on save | ✓ Good — clean serialization |
| Talents implemented inline (not via formal phase plan) | Feature was simple; plan overhead not warranted | ✓ Good — shipped faster; test coverage closed by Phase 05 |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-22 after v1.1 milestone complete*
