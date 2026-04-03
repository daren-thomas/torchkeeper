# Phase 2: Core Sheet - Context

**Gathered:** 2026-03-14
**Status:** Ready for planning

<domain>
## Phase Boundary

A player can open their character and manage everything needed at the table: identity fields, HP tracking, stats with bonus breakdowns, attacks list, gear inventory with slot counting, currency, and freeform notes. This phase builds the full character sheet UI. Character creation and automated rule lookups are out of scope — manual data entry only.

</domain>

<decisions>
## Implementation Decisions

### Navigation structure
- 3 tabs: **Sheet | Gear | Notes**
- Use MAUI Shell tab navigation
- Sheet tab contains: Identity → HP + XP → Stats → Attacks (top-to-bottom scroll order)
- Gear tab contains the full inventory and currency management
- Notes tab contains the freeform notes field

### Stat block (STAT-01, STAT-02)
- Each stat row (collapsed): stat name | total score | modifier — e.g. "STR  14  +2"
- Tapping a stat row expands an inline sub-list of bonus sources below it; tap again to collapse
- Each bonus source row shows: Label + value — e.g. "Ring of Protection  +1"
- Tapping the score number allows inline editing of the base stat value; total re-derives automatically

### HP tracker (HITP-01, HITP-02)
- Large +/- buttons flanking a large current HP number
- Format: "current / max" — e.g. "8 / 14"
- Tap the max HP number to edit it inline
- Current HP can go below 0 (no floor) — negatives are valid in Shadowdark
- No special visual at 0 — the number speaks for itself

### Gear list (GEAR-01 through GEAR-04)
- Unified item list — no visual distinction between mundane gear and magic items
- All items have: name, slot count, item type (free text), note (free text)
- Slot counter (used / total) displayed as a header above the list
- Tap any item row to open an edit modal with all fields
- "Add Item" button at the bottom of the list opens the same modal for new items
- Delete button inside the edit modal (one extra tap — avoids accidental deletes)
- Slot total = max(STR score, 10); coin slots auto-calculated per GEAR-04 rules

### Gear model unification
- GearItem and MagicItem are treated identically in the UI — a single unified list
- The planner should decide whether to merge the model types or map them to a shared view model; the user has no preference on the internal representation

### Attacks (ATCK-01)
- Editable list of free-form text entries — no structured fields
- Same modal-based pattern as gear: tap to edit, add button, delete inside modal
- Displayed as a section in the Sheet tab below Stats

### Identity fields (IDNT-01, IDNT-02)
- All identity fields shown at the top of the Sheet tab: name, class, ancestry, level, title, alignment, background, deity, languages, XP
- Editing approach: Claude's Discretion (inline tap-to-edit consistent with stat editing, or a separate identity edit form — either is fine)

### Claude's Discretion
- Exact visual styling, typography, colors — follow MAUI default styles unless an obvious improvement exists
- Identity field editing interaction (inline vs. form)
- Currency display placement within Gear tab (top, bottom, or inline with slot counter)
- Notes tab: simple Editor control, no formatting needed
- Loading/empty states

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `CharacterViewModel` (TorchKeeper/ViewModels/CharacterViewModel.cs): ObservableObject singleton stub. Phase 2 adds all observable properties and computed properties (stat totals, slot counts). Already wired as singleton in DI.
- `Character` model (TorchKeeper.Core/Models/Character.cs): All fields already defined — base stats, HP, currency, bonuses, gear, magic items, attacks, notes. No additions needed.
- `BonusSource` model: has Label, BonusTo (e.g. "DEX:+2"), SourceType, GainedAtLevel — all data needed for breakdown display is present.
- `GearItem` and `MagicItem`: separate types with identical field shape (name, slots, note). UI treats them as one list.

### Established Patterns
- CommunityToolkit.Mvvm: already installed, `[ObservableProperty]` and `[RelayCommand]` attributes available
- File operations (load/save/import) complete in Phase 1 — CharacterViewModel.LoadCharacter() is the hook for populating the VM after load
- `AppShell.xaml`: currently single-page Shell. Phase 2 replaces with tabbed Shell structure.
- No existing UI components to reuse — blank slate for all views

### Integration Points
- `CharacterViewModel.LoadCharacter(character)` must fire `OnPropertyChanged` for all bound properties after Phase 2 observable properties are added — this was explicitly called out in the Phase 1 comment
- STR stat change must update gear slot total — CharacterViewModel singleton ensures this propagates automatically if slot count is a computed property derived from STR
- `MauiProgram.cs`: CharacterViewModel registered as singleton — all tab pages should inject it from DI, not create new instances

</code_context>

<specifics>
## Specific Ideas

- HP section is the most-touched during a session — large tap targets are intentional, not just aesthetic
- The Sheet tab is a scrollable vertical layout; no horizontal scrolling or grids for the main sections
- "Allow negative HP" is a deliberate Shadowdark rule choice — characters can be at -HP before death

</specifics>

<deferred>
## Deferred Ideas

- None — discussion stayed within phase scope

</deferred>

---

*Phase: 02-core-sheet*
*Context gathered: 2026-03-14*
