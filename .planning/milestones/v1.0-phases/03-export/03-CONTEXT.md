# Phase 3: Export - Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

<domain>
## Phase Boundary

A player can export their full character sheet as formatted Markdown for print or offline reference. The output is a complete, human-readable document covering all character data. No preview step — export goes directly to the share sheet (mobile) or save-as dialog (desktop). Styling and formatting decisions made in this phase may be revisited in future milestones.

</domain>

<decisions>
## Implementation Decisions

### Export trigger
- **D-01:** Export is reachable from anywhere in the app — a toolbar/nav bar action visible on all three tabs
- **D-02:** On desktop (Windows/macOS), export uses the native menu/toolbar; on mobile (iOS/Android), the same action triggers the native share sheet
- **D-03:** Export button is always tappable, even with no character loaded (exports a blank/default sheet)
- **D-04:** No preview step — tap → share/save immediately

### Filename
- **D-05:** Default filename is `{Name}-{Class}{Level}.md` — e.g. `Brim-Thief4.md`

### Markdown structure
- **D-06:** Section order: Identity → Stats → Attacks → Currency → Gear → Notes
- **D-07:** No horizontal rules between sections
- **D-08:** Stat names in bold; keep Markdown plain and readable
- **D-09:** Character name as a top-level `#` heading; sections use `##`

### Stats section
- **D-10:** Each stat shows total + modifier, followed by an indented bullet list of bonus sources
  ```
  **STR** 16 (+3)
  - Base: 14
  - Ring of Protection: +2
  ```
- **D-11:** AC gets its own subsection using the same bonus-source breakdown pattern (bonus sources with `AC:` prefix)

### Attacks section
- **D-12:** `## Attacks` is its own section — free-form text entries, one per line (bullet list)

### Gear section
- **D-13:** Gear is a Markdown table with exactly `GearSlotTotal` rows (= max(STR, 10))
- **D-14:** Table header includes the slot count: `**Gear** (used / total slots)`
- **D-15:** Two columns: `Slot` (number) and `Item` (name)
- **D-16:** Multi-slot items: first row = item name, subsequent rows = `(cont. {item name})`
- **D-17:** Coin slots appear as a row with `Coins` when they occupy ≥1 slot
- **D-18:** Unused slots are empty numbered rows
- Example:
  ```
  | Slot | Item |
  |------|------|
  | 1 | Chain Mail |
  | 2 | (cont. Chain Mail) |
  | 3 | Rope |
  | 4 | Coins |
  | 5 | |
  ```

### Currency section
- **D-19:** `## Currency` section — one line each for GP, SP, CP

### HP / XP
- **D-20:** HP and XP appear in the Identity section, one line each — e.g. `HP: 8 / 14` and `XP: 3 / 10`

### Spells section
- **D-21:** `## Spells` section included if `SpellsKnown` is non-empty — plain string, no parsing
- **D-22:** `SpellsKnown` needs to be exposed on `CharacterViewModel` (it exists on the `Character` model but is not yet an observable property)

### Platform share/save behavior
- **D-23:** Mobile (iOS/Android): export triggers native share sheet with the `.md` file
- **D-24:** Desktop (Windows/macOS): export triggers save-as dialog; uses existing `IFileSaver` / `CommunityToolkitFileSaverAdapter` infrastructure

### Claude's Discretion
- Exact MIME type for the `.md` file on each platform
- Whether the export service lives in `SdCharacterSheet.Core` or the MAUI project
- Error handling if save/share fails
- Exact Identity section layout (field labels, line breaks vs. table)
- How to handle empty string fields in Identity (omit or include blank)

</decisions>

<specifics>
## Specific Ideas

- The gear slot table mirrors a physical character sheet — the intent is that the exported Markdown is printable and usable at the table without the app. Each slot number corresponds to a physical box on the sheet.
- `(cont. Chain Mail)` keeps the item name visible on overflow rows so the table is readable without mental tracking.
- HP and XP as single lines (not just totals) leave room for the player to write in current values on a printed sheet.

</specifics>

<canonical_refs>
## Canonical References

No external specs — requirements are fully captured in decisions above.

### For reference during planning/implementation
- `.planning/phases/01-foundation/01-CONTEXT.md` — IFileSaver interface, CommunityToolkitFileSaverAdapter, save/share infrastructure decisions
- `.planning/phases/02-core-sheet/02-CONTEXT.md` — CharacterViewModel shape, stat bonus model, gear slot rules, coin slot formula

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `IFileSaver` (`SdCharacterSheet.Core/Storage/IFileSaver.cs`): `SaveAsync(fileName, stream)` — already wired for desktop save-as
- `CommunityToolkitFileSaverAdapter` (`SdCharacterSheet/Storage/CommunityToolkitFileSaverAdapter.cs`): wraps CommunityToolkit.Maui's IFileSaver — use this for desktop export
- `CharacterViewModel` (`SdCharacterSheet/ViewModels/CharacterViewModel.cs`): all computed properties available (`TotalSTR`, `ModSTR`, `GearSlotsUsed`, `GearSlotTotal`, `CoinSlots`, `GearItems`, `Attacks`, `StatRows`) — export service reads from here directly
- `CharacterViewModel.Character`: backing `Character` model accessible for fields not yet exposed as observable properties (e.g. `SpellsKnown`)

### Established Patterns
- `MauiCharacterFileService` (`SdCharacterSheet/Services/MauiCharacterFileService.cs`): pattern for MAUI-layer services that wrap Core services with platform-specific behavior — export service should follow this pattern
- Stats bonus parsing: `BonusTo` field uses `"STAT:+N"` format (e.g. `"STR:+2"`); AC contributors use `"AC:+N"` prefix — same parsing logic applies to both

### Integration Points
- `CharacterViewModel.Character.SpellsKnown` — needs `[ObservableProperty]` added to CharacterViewModel (or export reads directly from `Character` model)
- Export button placement: toolbar/nav bar action registered in `AppShell.xaml` (or per-page toolbar items); needs wiring in all three tab pages or at Shell level
- DI: export service should be registered in `MauiProgram.cs` alongside existing services

</code_context>

<deferred>
## Deferred Ideas

- Markdown styling/formatting improvements — explicitly deferred to future milestones
- PDF export — out of scope for v1
- OS-level file type association for `.md` files — not required

</deferred>

---

*Phase: 03-export*
*Context gathered: 2026-03-21*
