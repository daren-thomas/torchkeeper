# Phase 6: Gear & Stats Polish - Context

**Gathered:** 2026-03-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Fix gear slot calculation to match Shadowdark rules (backpack/free-carry items are slotless), verify Markdown export reflects the same counts as the UI, add the base stat value to the stat drill-down, and populate the Talents field during Shadowdarklings import.

New capabilities (dice roller, multi-character roster, character builder) belong in other phases.

</domain>

<decisions>
## Implementation Decisions

### Free-Carry Item Mechanism (GEAR-01)
- **D-01:** Use a hybrid approach — auto-mark known Shadowdark free-carry items by name AND provide a user-settable "Free Carry" checkbox in the GearItemPopup for any item.
- **D-02:** Known free-carry names to auto-mark: Backpack, Bag of Coins, Thieves Tools. (Verify exact names against Shadowdark rulebook PDF before hardcoding.)
- **D-03:** Free-carry items appear in a **separate "Free Carry" section** in the gear list, positioned **below regular gear items**.
- **D-04:** The checkbox/flag label is **"Free Carry"** (matches Shadowdarklings terminology).
- **D-05:** Free-carry items are excluded from `GearSlotsUsed` — they consume 0 slots.

### Gear List Display (GEAR-01)
- **D-06:** The gear list splits into two sections: regular gear (with slot counts) above, then a "Free Carry" section below for slotless items.

### Markdown Export (GEAR-02)
- **D-07:** Markdown export mirrors the UI layout — regular gear table first, then a separate "Free Carry" section below.
- **D-08:** `MarkdownExportService` already reads `vm.GearSlotsUsed` and `vm.CoinSlots` — export parity is automatic once the slot calc is fixed. No separate fix needed.
- **D-09:** Add a test verifying that coin slot count in `CharacterViewModel.GearSlotsUsed` matches the slot count parsed from `MarkdownBuilder` output (covers the gap in the "test coin slots match" todo).

### Stat Drill-Down (STAT-01)
- **D-10:** Base stat value appears **in the expanded panel only** — not in the collapsed row header. Collapsed row remains clean (stat name, total score, modifier).
- **D-11:** Base stat is labeled **"Base: 14"** in the expanded panel (not the stat name, just the label "Base:" followed by the value).
- **D-12:** Base stat row appears **first** in the expanded panel, above the bonus source rows.

### Talents Import
- **D-13:** Shadowdarklings import populates the **Talents** free-text field from `levels[].talentRolledDesc` in the JSON, formatted as one line per talent: `Lv{N}: {description}`. Empty `talentRolledDesc` entries are skipped.
- **D-14:** `SpellsKnown` from the top-level JSON field continues to map to the **Spells** free-text field (existing behavior, no change).
- **D-15:** `ShadowdarklingsJson` DTO needs a `Levels` list property to deserialize the levels array. Add a `SdLevelEntry` class with at minimum `Level` (int) and `TalentRolledDesc` (string) fields.

### Claude's Discretion
- Exact ordering of the "Base: N" row relative to bonus sources if the panel is empty (no bonuses) — Claude decides.
- Whether `Rolled12ChosenTalentDesc` (level-12 talent choice) should also be included in the Talents import — Claude reads the example data and includes it if non-empty.
- Whether free-carry auto-detection is case-insensitive — Claude decides (recommend: yes).

### Folded Todos
- **Drill down base stat** — STAT-01 directly. Implemented in `StatRowViewModel` expanded panel.
- **Fix coins and slots discrepancies** — GEAR-01/02. Investigate and fix slot calc mismatch.
- **Check Shadowdark rules for backpack/free-carry items** — prereq research for GEAR-01. Researcher must read `~/Downloads/Shadowdark_RPG_-_V4-9.pdf` encumbrance section to confirm free-carry item list and rules before implementation.
- **Talents import behavior** — D-13 through D-15. Import levels[].talentRolledDesc into Talents field.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Shadowdark Rules
- `~/Downloads/Shadowdark_RPG_-_V4-9.pdf` — Official rulebook. Read the gear/encumbrance section to confirm: (1) which items are free to carry, (2) coin weight rules (first 100 coins free, etc.). Required before implementing GEAR-01.

### Example Import Files
- `examples/Brim.json` — Thief/Dwarf, level 3. Has 2 level talents. spellsKnown is 'None'.
- `examples/Ghellence Pnidd.json` — Wizard/Risen, level 3. Has 2 level talents + comma-separated spellsKnown.
- `examples/Tham.json` — Warlock/Tiefling, level 1. Has 1 level talent.
- `examples/Zaraevra.json` — Wizard/Dragonborn, level 1. Has 1 level talent + spellsKnown.
- `examples/Zaraevra (1).json` — Same character, slightly different spells.
- `examples/M'liox.json` — Additional example.

### Codebase Files
- `SdCharacterSheet.Core/Models/GearItem.cs` — Current GearItem model (no FreeCarry flag)
- `SdCharacterSheet/ViewModels/CharacterViewModel.cs` — GearSlotTotal, GearSlotsUsed, CoinSlots calculations
- `SdCharacterSheet/ViewModels/StatRowViewModel.cs` — BaseStat, TotalScore, ModifierDisplay, BonusSources
- `SdCharacterSheet/ViewModels/GearItemViewModel.cs` — Per-item ViewModel
- `SdCharacterSheet/Views/Popups/GearItemPopup.xaml` — Gear item edit popup (add Free Carry checkbox here)
- `SdCharacterSheet/Views/GearPage.xaml` — Gear list view (add separate Free Carry section)
- `SdCharacterSheet/Views/SheetPage.xaml` — Stat rows with expand/collapse (add Base row here)
- `SdCharacterSheet.Core/Export/MarkdownBuilder.cs` — Gear section export (add Free Carry section)
- `SdCharacterSheet.Core/Export/CharacterExportData.cs` — Export data record (may need FreeCarry items list)
- `SdCharacterSheet/Services/MarkdownExportService.cs` — Builds CharacterExportData from ViewModel
- `SdCharacterSheet.Core/DTOs/ShadowdarklingsJson.cs` — Import DTO (add Levels list + SdLevelEntry class)
- `SdCharacterSheet.Core/Services/ShadowdarklingsImportService.cs` — Import logic (populate Talents)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `GearItemViewModel.cs` — Has `Slots` observable property; needs `IsFreeCarry` bool added
- `StatRowViewModel.cs` — Has `BaseStat`, `BonusSources`; needs base stat row in expanded view
- `CharacterViewModel.GearSlotsUsed` — Currently `GearItems.Sum(g => g.Slots) + CoinSlots`; needs to filter out free-carry items
- `MarkdownBuilder.BuildMarkdown()` — Gear section at line 70; currently one table, needs split

### Established Patterns
- `[ObservableProperty]` + `[NotifyPropertyChangedFor(...)]` for reactive properties in ViewModels (CommunityToolkit.Mvvm)
- `GearItemSource` enum as discriminator for gear vs magic items on save
- `CharacterSaveData` is the save DTO — any new fields need to be added here for persistence
- `MauiCharacterFileService` / `CharacterFileService` round-trip through `CharacterSaveData` — new fields need DTO + round-trip wiring
- Free-carry items already in gear list (Backpack, Bag of Coins, Thieves Tools) will be auto-detected by name on load

### Integration Points
- `GearItem` model → `CharacterSaveData` → round-trip: add `IsFreeCarry` to both `GearItem` and `GearItemSaveData`
- `StatRowViewModel.BonusSources` collection → SheetPage XAML `BindableLayout.ItemsSource` — add Base row as first item or as a separate label above the collection
- `ShadowdarklingsJson.Levels` (new) → `ShadowdarklingsImportService` → `CharacterSaveData.Talents`

</code_context>

<specifics>
## Specific Ideas

- User confirmed: free-carry section goes **below** regular gear in both UI and Markdown export
- User confirmed: base stat in expanded panel only — no changes to collapsed row
- User confirmed: `Lv{N}: {desc}` format for imported talent descriptions
- User wants researcher to verify the exact free-carry item list against the official Shadowdark PDF before implementing

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

### Reviewed Todos (not folded)
- **Test coin slots match between export and character sheet** — Covered by D-09 (add export parity test). Considered folded via D-09 even though not explicitly selected in the todo fold prompt (UI limit of 4 options).

</deferred>

---

*Phase: 06-gear-stats-polish*
*Context gathered: 2026-03-28*
