# Requirements: Shadowdark Character Sheet

**Defined:** 2026-03-08
**Core Value:** A player can open their character, see all their stats with full bonus breakdowns, and manage their inventory slot-by-slot — everything needed at the table that Shadowdarklings doesn't provide.

## v1 Requirements

### Identity

- [ ] **IDNT-01**: User can view and edit character identity fields (name, class, ancestry, level, title, alignment, background, deity, languages)
- [ ] **IDNT-02**: User can track XP

### Stats

- [ ] **STAT-01**: User can view and edit the 6 core stats (STR, DEX, CON, INT, WIS, CHA) with automatic modifier display
- [ ] **STAT-02**: User can see a breakdown of all bonus sources contributing to each stat total (display only — no toggle in v1)

### Hit Points

- [ ] **HITP-01**: User can track current HP with increment/decrement controls
- [ ] **HITP-02**: User can set maximum HP

### Gear / Inventory

- [ ] **GEAR-01**: User can add, edit, and remove gear items (mundane and magic)
- [ ] **GEAR-02**: Each gear item has name, slot count, type, and an optional free-text note; magic items can additionally store benefits, curses, and personality traits in their note
- [ ] **GEAR-03**: App displays total gear slots used vs total available (slots = max(STR score, 10))
- [ ] **GEAR-04**: App auto-calculates coin slots consumed by current GP/SP/CP totals (100 coins per slot per denomination; first 100 coins of any type are free)

### Currency

- [ ] **CURR-01**: User can track current GP, SP, and CP totals

### Attacks

- [ ] **ATCK-01**: User can maintain an editable list of free-form attack text entries (e.g. "DAGGER: +3 (N), 1d4 (FIN)")

### Notes

- [ ] **NOTE-01**: User can write and edit freeform notes on the character

### File Operations

- [ ] **FILE-01**: User can import a Shadowdarklings.net JSON export to bootstrap a character
- [ ] **FILE-02**: User can load a character from a native .sdchar file
- [ ] **FILE-03**: User can save a character to a .sdchar file

### Markdown Export

- [ ] **MRKD-01**: User can export the full character sheet as formatted Markdown for print/reference

## v2 Requirements

### Bonus Toggles

- **BNUS-01**: User can toggle individual bonus sources on/off per stat (equipped items, situational talents, spell effects)
- **BNUS-02**: Stat totals and modifiers update immediately when a bonus source is toggled

### Currency Ledger

- **LEDG-01**: User can view full GP/SP/CP transaction history with descriptions
- **LEDG-02**: User can add ledger entries manually

### Polish

- **PLSH-01**: Gear slot utilization shown as a visual progress bar
- **PLSH-02**: Character sheet auto-saves on every change (IsDirty pattern)

## Out of Scope

| Feature | Reason |
|---------|--------|
| Character builder (automated talent tables, ancestry auto-apply) | User enters choices manually — design philosophy |
| Online sync / cloud saves | File-based only |
| Multi-character roster UI | Character switching via OS file management |
| Shadowdarklings-compatible JSON export | Import only |
| Spell management beyond free-text notes | Out of Shadowdark scope for v1 |
| Dice roller | Not a character sheet concern |
| Automatic attack derivation from weapon stats | Attacks are free-form text, not calculated |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| FILE-01 | Phase 1 | Pending |
| FILE-02 | Phase 1 | Pending |
| FILE-03 | Phase 1 | Pending |
| IDNT-01 | Phase 2 | Pending |
| IDNT-02 | Phase 2 | Pending |
| STAT-01 | Phase 2 | Pending |
| STAT-02 | Phase 2 | Pending |
| HITP-01 | Phase 2 | Pending |
| HITP-02 | Phase 2 | Pending |
| GEAR-01 | Phase 2 | Pending |
| GEAR-02 | Phase 2 | Pending |
| GEAR-03 | Phase 2 | Pending |
| GEAR-04 | Phase 2 | Pending |
| CURR-01 | Phase 2 | Pending |
| ATCK-01 | Phase 2 | Pending |
| NOTE-01 | Phase 2 | Pending |
| MRKD-01 | Phase 3 | Pending |

**Coverage:**
- v1 requirements: 17 total
- Mapped to phases: 17
- Unmapped: 0 ✓

---
*Requirements defined: 2026-03-08*
*Last updated: 2026-03-08 after initial definition*
