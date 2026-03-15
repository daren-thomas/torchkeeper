---
phase: 02-core-sheet
plan: "02"
subsystem: ui
tags: [maui, xaml, data-binding, community-toolkit-maui, bindable-layout, popup, observable-property]

# Dependency graph
requires:
  - phase: 02-core-sheet
    provides: CharacterViewModel with all observable properties, StatRowViewModel with Action<int> callback, GearItemViewModel, ObservableCollections for StatRows/Attacks/GearItems
  - phase: 01-foundation
    provides: SdCharacterSheet.Core models (BonusSource, Character, GearItem, MagicItem)
provides:
  - SheetPage.xaml: 4-section ScrollView tab — Identity Grid, HP/XP tracker with MaxHP tap-to-edit, Stats BindableLayout with score tap-to-edit, Attacks CollectionView
  - AttackPopup.xaml: CommunityToolkit.Maui Popup for adding and editing attack strings
  - StatRowViewModel: IsEditingBase, BeginEditBaseCommand, CommitBaseEditCommand for inline stat editing
affects: [AppShell wiring, MauiProgram DI registration, 02-03-gear-tab, 02-04-identity-tab]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "BindableLayout.ItemsSource on VerticalStackLayout instead of CollectionView+Expander — avoids iOS resize bug (CommunityToolkit.Maui #1557, #1670)"
    - "Tap-to-edit toggle: Label+Entry pair with IsVisible bound to IsEditingBase / InverseBoolConverter"
    - "MaxHP tap-to-edit via code-behind: TapGestureRecognizer → OnMaxHpLabelTapped toggles x:Name'd Label/Entry visibility"
    - "CommunityToolkit.Maui InvertedBoolConverter in ContentPage.Resources keyed as InverseBoolConverter"
    - "AttackPopup dual constructors: AttackPopup(vm) for new, AttackPopup(vm, text, index) for edit"

key-files:
  created:
    - SdCharacterSheet/Views/SheetPage.xaml
    - SdCharacterSheet/Views/SheetPage.xaml.cs
    - SdCharacterSheet/Views/Popups/AttackPopup.xaml
    - SdCharacterSheet/Views/Popups/AttackPopup.xaml.cs
  modified:
    - SdCharacterSheet/ViewModels/StatRowViewModel.cs

key-decisions:
  - "Stats section uses BindableLayout (not CollectionView+Expander) — iOS safe per CommunityToolkit.Maui issues #1557 and #1670"
  - "MaxHP tap-to-edit implemented in code-behind (OnMaxHpLabelTapped) using x:Name references rather than ViewModel state — keeps HP section logic simple and MaxHP IsVisible is not a persisted state"
  - "Attack CollectionView omits x:DataType on string items — no compiled bindings benefit on plain strings, avoids xmlns complexity"

patterns-established:
  - "Label/Entry tap-to-edit pair: IsVisible='{Binding IsEditingBase}' on Entry, IsVisible='{Binding IsEditingBase, Converter={StaticResource InverseBoolConverter}}' on Label — toggle via [RelayCommand] methods on ViewModel"
  - "Popup wiring: this.ShowPopupAsync(new AttackPopup(...)) called from code-behind async void handlers"

requirements-completed: [IDNT-01, IDNT-02, STAT-01, STAT-02, HITP-01, HITP-02, ATCK-01]

# Metrics
duration: 2min
completed: 2026-03-15
---

# Phase 02 Plan 02: SheetPage Summary

**SheetPage XAML with 4-section tab — identity fields, HP/XP tracker (MaxHP tap-to-edit), stats BindableLayout with score tap-to-edit and bonus expansion, attacks CollectionView with AttackPopup**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-15T10:22:54Z
- **Completed:** 2026-03-15T10:24:43Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- Built SheetPage.xaml with all 4 sections in a ScrollView: Identity (10-field Grid), HP/XP (MaxHP tap-to-edit toggle with code-behind), Stats (BindableLayout with InverseBoolConverter for score inline edit), Attacks (CollectionView with tap handler)
- Created AttackPopup (CommunityToolkit.Maui Popup) with dual constructors for new/edit mode, Save and Delete handlers
- Extended StatRowViewModel with IsEditingBase, BeginEditBaseCommand, CommitBaseEditCommand enabling the score tap-to-edit pattern in XAML

## Task Commits

Each task was committed atomically:

1. **Task 1: Build SheetPage XAML — identity, HP/XP, and stats sections** - `54ec538` (feat)
2. **Task 2: Add attacks section to SheetPage and create AttackPopup** - `175d171` (feat)

## Files Created/Modified
- `SdCharacterSheet/Views/SheetPage.xaml` - 4-section sheet tab: Identity grid, HP/XP tracker with MaxHP tap-to-edit, Stats BindableLayout with tap-to-edit score and expandable bonus sources, Attacks CollectionView
- `SdCharacterSheet/Views/SheetPage.xaml.cs` - Code-behind: CharacterViewModel injection, HP +/- handlers, MaxHP Label/Entry toggle, OnAttackTapped and OnAddAttackClicked popup wiring
- `SdCharacterSheet/Views/Popups/AttackPopup.xaml` - CommunityToolkit.Maui Popup with Entry for attack text, Save and Delete buttons
- `SdCharacterSheet/Views/Popups/AttackPopup.xaml.cs` - Dual constructors (new/edit), OnSave writes to Attacks collection, OnDelete removes by index
- `SdCharacterSheet/ViewModels/StatRowViewModel.cs` - Added IsEditingBase observable property, BeginEditBaseCommand and CommitBaseEditCommand relay commands

## Decisions Made
- Stats section uses BindableLayout on VerticalStackLayout rather than CollectionView+Expander (plan requirement — avoids CommunityToolkit.Maui iOS resize bugs #1557, #1670)
- MaxHP tap-to-edit toggle implemented via code-behind OnMaxHpLabelTapped/OnMaxHpEntryCompleted/OnMaxHpEntryUnfocused rather than ViewModel state — MaxHP visibility is transient UI state, not persisted
- Attack CollectionView DataTemplate omits x:DataType — binding to plain string with `{Binding .}` doesn't benefit from compile-time binding, avoids type syntax complexity

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- MAUI project build (`dotnet build SdCharacterSheet/`) cannot run in this environment — MAUI workload and NuGet packages require network or local cache. Test project (net10.0 class library) builds with 0 errors, confirming shared C# code compiles correctly. XAML validation requires a MAUI build environment.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- SheetPage is the primary player tab; all 4 sections (Identity, HP/XP, Stats, Attacks) are implemented
- StatRowViewModel now has all commands needed for XAML binding (BeginEditBase, CommitBaseEdit, ToggleExpand)
- AttackPopup is ready for wiring into AppShell/MauiProgram DI once SheetPage is registered as a tab
- Plans 02-03 (GearPage) and 02-04 can proceed independently; both use CharacterViewModel as the shared data layer

---
*Phase: 02-core-sheet*
*Completed: 2026-03-15*

## Self-Check: PASSED

- SheetPage.xaml: FOUND
- SheetPage.xaml.cs: FOUND
- AttackPopup.xaml: FOUND
- AttackPopup.xaml.cs: FOUND
- 02-02-SUMMARY.md: FOUND
- Commit 54ec538: FOUND
- Commit 175d171: FOUND
