# Milestones

## v1.2 Gear & Stats Polish (Shipped: 2026-04-03)

**Phases completed:** 2 phases, 4 plans, 11 tasks

**Key accomplishments:**

- IsFreeCarry flag added through Core + MAUI layers with auto-detect for Backpack/Bag of Coins/Thieves Tools; GearPage split into regular gear + "Free Carry" sections with live slot exclusion
- Markdown export mirrors UI gear layout: separate "### Free Carry" section added to MarkdownBuilder; 5 unit tests verify GEAR-01/GEAR-02 parity (67 tests total)
- Stat drill-down expanded panel now shows "Base: N" as first row above bonus sources; Shadowdarklings import populates Talents field from levels[].talentRolledDesc
- IsFreeCarry propagated to all four MAUI-local shadow types (GearItem, MagicItem, GearItemData, MagicItemData) and both mapping directions in CharacterFileService, closing GEAR-01 save/load data loss

---

## v1.1 File Management & Talents (Shipped: 2026-03-22)

**Phases completed:** 2 phases, 3 plans, 4 tasks

**Key accomplishments:**

- SaveCommand/LoadCommand/ImportCommand wired into CharacterViewModel with MAUI file pickers, menu items in AppShell, and 6 unit tests using Core-only test fakes
- All three file operations (Save, Open, Import) verified working on macOS Catalyst — no crashes on cancellation, menu items correctly placed
- MacFilePickerHelper workaround for MAUI FilePicker.Default.PickAsync() silently returning null on macOS 15 Sequoia (UIDocumentPickerViewController direct wiring)
- Talents/Spells free-text field added through full stack (model, DTO, save/load, export, ViewModel, NotesPage) — TLNT-01 delivered via inline implementation
- 54 xUnit tests passing — net +8 tests (6 file command, 2 Talents coverage) protecting all critical paths

---

## v1.0 MVP (Shipped: 2026-03-21)

**Phases completed:** 3 phases, 13 plans, 19 tasks

**Key accomplishments:**

- .NET 10 MAUI solution scaffolded with Character domain model, two-DTO architecture (save + import), CharacterViewModel skeleton, and xUnit Wave 0 stub tests targeting net10.0
- Async JSON import service mapping Shadowdarklings.net exports to Character domain model with currency fallback, RolledStats base stats, and full bonus pass-through
- Stream-based JSON save/load for Character via System.Text.Json with full Character<->CharacterSaveData mapping and 2 xUnit TDD tests covering FILE-02 and FILE-03
- Native .sdchar file open/save dialogs via FilePicker + IFileSaver with platform-specific file type declarations for all four targets (iOS/macOS/Android/Windows), fully wired into DI
- NullFileSaver test double unblocks CharacterFileServiceTests compilation, enabling 9 tests passing including FILE-02 round-trip and FILE-03 version-field verification
- xUnit test scaffold with 18 passing tests documenting CharacterViewModel computed property contracts (modifier math, bonus filtering, gear/coin slots, negative HP) and GearItemViewModel gear unification via test-local stubs
- Full observable CharacterViewModel with 6 stats, computed totals/modifiers, gear/coin slots, and LoadCharacter with per-stat write-back lambdas; plus StatRowViewModel and GearItemViewModel as supporting types
- SheetPage XAML with 4-section tab — identity fields, HP/XP tracker (MaxHP tap-to-edit), stats BindableLayout with score tap-to-edit and bonus expansion, attacks CollectionView with AttackPopup
- GearPage with live slot counter (GearSlotsUsed/GearSlotTotal), tap-to-edit gear list via GearItemPopup modal, and GP/SP/CP currency section
- NotesPage with a single full-height MAUI Editor bound TwoWay to CharacterViewModel.Notes via Grid fill layout
- AppShell replaced with 3-tab TabBar (Sheet, Gear, Notes) wiring all character sheet views into a single DI-connected app
- One-liner:
- MAUI export layer wired end-to-end: MarkdownExportService routes to share sheet (mobile) or save-as dialog (desktop), triggered by a Shell ToolbarItem; coin encumbrance bug fixed to ceiling division (101+ coins = 1 slot)

---
