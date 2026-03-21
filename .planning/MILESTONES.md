# Milestones

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
