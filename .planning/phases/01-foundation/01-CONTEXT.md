# Phase 1: Foundation - Context

**Gathered:** 2026-03-08
**Status:** Ready for planning

<domain>
## Phase Boundary

The app can load, save, and import a character with all data intact — no UI required. This phase delivers the domain model (Character, BonusSource, GearItem, AcComponent), the .sdchar native file format, and the Shadowdarklings JSON import pipeline.

</domain>

<decisions>
## Implementation Decisions

### .NET Version & Project Setup
- **Target: .NET 10** (LTS, released ~Nov 2025, supported Nov 2025–Nov 2028)
- **All four platforms from day one**: net10.0-windows, net10.0-maccatalyst, net10.0-ios, net10.0-android
- **Single MAUI app project** — no separate class library; domain model lives inside the MAUI project

### Windows Deployment Model
- **Unpackaged** (no MSIX) — simpler development, direct filesystem access, no sandbox
- **Distribution: self-contained executable / zip** — xcopy deploy, no installer required for personal use
- **OS file type association (.sdchar → app): nice-to-have, not required for Phase 1** — file picker is sufficient

### Import Scope
Fields imported from Shadowdarklings JSON:
- Identity: `name`, `class`, `ancestry`, `level`, `title`, `alignment`, `background`, `deity`, `languages`
- Stats: `rolledStats` (NOT `stats` — avoids double-counting bonuses)
- Bonuses: `bonuses[]` array (each item has bonusName, bonusTo, sourceType, sourceCategory, gainedAtLevel, sourceName)
- HP: `maxHitPoints`
- XP: `XP`
- Gear: `gear[]` and `magicItems[]`
- Spells: `spellsKnown` (plain string, imported as-is)

Fields NOT imported (silently discarded):
- `stats` — use `rolledStats` instead
- `levels[]` — per-level talent/HP roll history; app doesn't model level advancement history
- `armorClass` — app calculates AC from user-maintained contributor list
- `gearSlotsTotal`, `gearSlotsUsed` — app calculates from STR and item list
- `ledger[]` transaction history — derive final GP/SP/CP totals from it, then discard history
- `edits[]`, `ambitionTalentLevel`, `creationMethod`, `coreRulesOnly`, `activeSources` — Shadowdarklings metadata, not needed
- Unknown/future fields: **silently ignored** (use JsonIgnoreCondition in System.Text.Json)

### Currency Import
- Check the JSON for a dedicated current-coins field first
- Fall back to summing `ledger[]` `goldChange`/`silverChange`/`copperChange` entries to derive final GP/SP/CP totals
- Discard the ledger transaction history — app only stores totals (v1 has no ledger view)

### Initial HP on Import
- **currentHP = maxHP** on import — assume full health when bootstrapping from Shadowdarklings
- Player adjusts currentHP during play

### Armor Class Model
- **AC is NOT derived from typed gear items** — gear remains untyped
- AC is modeled as a calculated total with a **free-form contributor list** (same pattern as stats):
  - Each contributor has a label (e.g. "Leather Armor") and a value/formula description (e.g. "11 + DEX mod")
  - User manually maintains the AC breakdown
  - Total is computed from the contributors
- The Shadowdarklings `armorClass` value is discarded on import — user rebuilds the AC breakdown manually or from `bonuses[]` entries with `bonusTo` starting with `"AC:"`

### Native Save Format (.sdchar)
- Serialization: JSON (using `System.Text.Json`)
- Target: separate `CharacterSaveData` DTO with a version field — never serialize the ViewModel
- File extension: `.sdchar`

### Gear Slots Rule (from rulebook)
- Gear slots = **max(STR stat, 10)** — not just STR score
- Coins: **100 coins = 1 slot, first 100 free to carry** — applies per denomination (GP, SP, CP tracked separately)
  - NOTE: REQUIREMENTS.md GEAR-03 and GEAR-04 need updating to match these rules

### Claude's Discretion
- Internal DTO schema design (property names, nesting)
- Null handling for optional fields in the Shadowdarklings JSON
- JSON serialization options (indented vs compact for .sdchar files)
- Error handling approach for malformed import files

</decisions>

<specifics>
## Specific Ideas

- The bonus drill-down model applies to both stats (STR/DEX/etc.) and AC — same underlying `BonusSource[]` pattern
- AC contributor labels are free-form text so users can write "Leather Armor: 11+DEX mod" or "Ring of Protection: +1" without the app needing to understand armor types
- Shadowdarklings `bonuses[]` with `bonusTo` of `"DEX:+2"` format map cleanly to the stat bonus model; AC contributors from `bonuses[]` (if any have `bonusTo` starting with `"AC:"`) should also be imported

</specifics>

<code_context>
## Existing Code Insights

### Reusable Assets
- None — this is a greenfield project. Phase 1 creates the foundational assets.

### Established Patterns
- None yet — Phase 1 establishes the patterns that Phase 2 will extend.

### Integration Points
- Phase 1 produces: `CharacterViewModel` (singleton), `CharacterSaveData` DTO, `ShadowdarklingsImportService`, `CharacterFileService`
- Phase 2 will bind MAUI views directly to `CharacterViewModel`

</code_context>

<deferred>
## Deferred Ideas

- OS-level file type association (.sdchar double-click to open) — future phase or post-v1
- Ledger/transaction history view — v2 requirement (LEDG-01, LEDG-02), explicitly deferred
- Auto-save / IsDirty pattern — v2 requirement (PLSH-02), deferred

</deferred>

---

*Phase: 01-foundation*
*Context gathered: 2026-03-08*
