# Phase 2: Core Sheet - Research

**Researched:** 2026-03-14
**Domain:** .NET MAUI UI — Shell tabs, MVVM bindings, CollectionView, Popup, CommunityToolkit
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Navigation structure**
- 3 tabs: Sheet | Gear | Notes
- Use MAUI Shell tab navigation
- Sheet tab contains: Identity → HP + XP → Stats → Attacks (top-to-bottom scroll order)
- Gear tab contains the full inventory and currency management
- Notes tab contains the freeform notes field

**Stat block (STAT-01, STAT-02)**
- Each stat row (collapsed): stat name | total score | modifier — e.g. "STR  14  +2"
- Tapping a stat row expands an inline sub-list of bonus sources below it; tap again to collapse
- Each bonus source row shows: Label + value — e.g. "Ring of Protection  +1"
- Tapping the score number allows inline editing of the base stat value; total re-derives automatically

**HP tracker (HITP-01, HITP-02)**
- Large +/- buttons flanking a large current HP number
- Format: "current / max" — e.g. "8 / 14"
- Tap the max HP number to edit it inline
- Current HP can go below 0 (no floor) — negatives are valid in Shadowdark
- No special visual at 0 — the number speaks for itself

**Gear list (GEAR-01 through GEAR-04)**
- Unified item list — no visual distinction between mundane gear and magic items
- All items have: name, slot count, item type (free text), note (free text)
- Slot counter (used / total) displayed as a header above the list
- Tap any item row to open an edit modal with all fields
- "Add Item" button at the bottom of the list opens the same modal for new items
- Delete button inside the edit modal (one extra tap — avoids accidental deletes)
- Slot total = max(STR score, 10); coin slots auto-calculated per GEAR-04 rules

**Gear model unification**
- GearItem and MagicItem are treated identically in the UI — a single unified list
- The planner should decide whether to merge the model types or map them to a shared view model; the user has no preference on the internal representation

**Attacks (ATCK-01)**
- Editable list of free-form text entries — no structured fields
- Same modal-based pattern as gear: tap to edit, add button, delete inside modal
- Displayed as a section in the Sheet tab below Stats

**Identity fields (IDNT-01, IDNT-02)**
- All identity fields shown at the top of the Sheet tab: name, class, ancestry, level, title, alignment, background, deity, languages, XP
- Editing approach: Claude's Discretion

### Claude's Discretion
- Exact visual styling, typography, colors — follow MAUI default styles unless an obvious improvement exists
- Identity field editing interaction (inline vs. form)
- Currency display placement within Gear tab (top, bottom, or inline with slot counter)
- Notes tab: simple Editor control, no formatting needed
- Loading/empty states

### Deferred Ideas (OUT OF SCOPE)
- None — discussion stayed within phase scope
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| IDNT-01 | View and edit character identity fields (name, class, ancestry, level, title, alignment, background, deity, languages) | Shell tab structure; Entry/Label inline editing pattern or identity form page |
| IDNT-02 | Track XP | Same as IDNT-01 — XP is a numeric Entry on the identity section |
| STAT-01 | View and edit the 6 core stats with automatic modifier display | ObservableProperty computed modifiers with NotifyPropertyChangedFor; inline tap-to-edit for base stat |
| STAT-02 | See breakdown of all bonus sources contributing to each stat total | BonusSource model already present; CommunityToolkit Expander per stat row; BindableLayout over filtered bonus list |
| HITP-01 | Track current HP with increment/decrement controls | RelayCommand +/- buttons; [ObservableProperty] for CurrentHP with no floor; TwoWay binding |
| HITP-02 | Set maximum HP | Tap-to-edit inline pattern on max HP label; computed display string |
| GEAR-01 | Add, edit, and remove gear items | ObservableCollection<GearItemViewModel>; Popup modal for add/edit; delete button inside popup |
| GEAR-02 | Each gear item has name, slot count, type, note | GearItemViewModel wrapper with four [ObservableProperty] fields; bound to Popup Entry controls |
| GEAR-03 | Display total gear slots used vs total available (max(STR,10)) | Computed property on CharacterViewModel; [NotifyPropertyChangedFor] chains from STR and gear collection |
| GEAR-04 | Auto-calculate coin slots from GP/SP/CP (100 per denomination, first 100 free) | Pure computed property: (max(GP-100,0)/100) + (max(SP-100,0)/100) + (max(CP-100,0)/100) |
| CURR-01 | Track current GP, SP, and CP totals | Three [ObservableProperty] int fields; Entry with Keyboard.Numeric |
| ATCK-01 | Maintain editable list of free-form attack text entries | ObservableCollection<string> exposed via AttackViewModel wrapper; same Popup pattern as gear |
| NOTE-01 | Write and edit freeform notes | Editor control (multiline) with TwoWay binding to CharacterViewModel.Notes |
</phase_requirements>

---

## Summary

Phase 2 is entirely a UI-build phase: all domain models and file services exist from Phase 1. The work is to wire `CharacterViewModel` with `[ObservableProperty]` fields and computed properties, replace the single-page `AppShell.xaml` with a `TabBar` shell, and build three content pages (SheetPage, GearPage, NotesPage) bound to the shared singleton ViewModel.

The highest-complexity areas are (1) the stat expand/collapse interaction — use a `BindableLayout`-backed vertical stack per stat rather than `Expander` inside `CollectionView` to avoid a known iOS resize bug — and (2) the unified gear list requiring a thin `GearItemViewModel` wrapper to bridge two model types (`GearItem` and `MagicItem`) into a single `ObservableCollection`. Everything else maps cleanly to standard MAUI patterns with the toolkit already installed.

The `CharacterViewModel.LoadCharacter()` stub in Phase 1 has a comment explicitly calling out that Phase 2 must add `OnPropertyChanged` calls for all new bound properties. That is the critical integration point: every property added to the ViewModel must also be refreshed in `LoadCharacter()`.

**Primary recommendation:** Build section by section within each tab page, test each binding in isolation, and do not use `Expander` inside `CollectionView` — instead implement stat expansion with a per-stat `IsExpanded` flag on a view model item and `BindableLayout` for the bonus source sub-list.

---

## Standard Stack

### Core (already installed — no new packages needed)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CommunityToolkit.Maui | 14.0.1 | Popup modal, Expander, Toast | Already in csproj; official MAUI toolkit |
| CommunityToolkit.Mvvm | 8.4.0 | [ObservableProperty], [RelayCommand], source generators | Already in csproj; code-gen avoids boilerplate |
| Microsoft.Maui.Controls | (MauiVersion) | Shell, TabBar, CollectionView, Entry, Editor | The framework itself |

### Supporting patterns (no new packages)

| Facility | Purpose | When to Use |
|----------|---------|-------------|
| `ObservableCollection<T>` | Live-updating gear and attack lists | Any list that adds/removes items at runtime |
| `BindableLayout.ItemsSource` | Render bonus source sub-rows inline in a VerticalStackLayout | Preferred over CollectionView inside another layout due to iOS resize safety |
| `Shell.Current.ShowPopupAsync()` | Show gear/attack edit popup from anywhere | Available on all platforms via CommunityToolkit.Maui 14+ |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `BindableLayout` for bonus sub-rows | `Expander` from CommunityToolkit.Maui | Expander inside CollectionView has an open iOS resize bug (issues #1557, #1670, #23033). BindableLayout renders inline with no resize concerns. |
| Modal Popup (CommunityToolkit) for gear editing | Shell.PushModalAsync with a full ContentPage | Popup is lighter; full page navigation is overkill for a 4-field form |
| Inline tap-to-edit Label/Entry toggle | A separate "Edit Identity" page | Inline is consistent with stat editing; a separate page wastes a navigation push for simple fields |

**Installation:** No new packages needed. CommunityToolkit.Maui 14.0.1 and CommunityToolkit.Mvvm 8.4.0 are already referenced.

---

## Architecture Patterns

### Recommended Project Structure

```
TorchKeeper/
├── ViewModels/
│   ├── CharacterViewModel.cs      # Expanded with all observable properties (Phase 2)
│   ├── StatRowViewModel.cs        # Per-stat: IsExpanded, TotalScore, Modifier, BonusSources
│   └── GearItemViewModel.cs       # Thin wrapper over unified GearItem/MagicItem
├── Views/
│   ├── SheetPage.xaml / .cs       # Sheet tab: Identity, HP/XP, Stats, Attacks
│   ├── GearPage.xaml / .cs        # Gear tab: slot header, unified item list, currency
│   ├── NotesPage.xaml / .cs       # Notes tab: Editor
│   └── Popups/
│       ├── GearItemPopup.xaml / .cs   # Add/Edit gear item modal
│       └── AttackPopup.xaml / .cs     # Add/Edit attack text modal
└── AppShell.xaml / .cs            # Replaced with TabBar (3 tabs)
```

### Pattern 1: Shell TabBar (3 tabs)

**What:** Replace single `ShellContent` in `AppShell.xaml` with a `TabBar` containing three `Tab` entries.
**When to use:** Any MAUI app with bottom-tab navigation. `TabBar` disables the flyout automatically.

```xaml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/tabs?view=net-maui-10.0 -->
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:TorchKeeper.Views"
       x:Class="TorchKeeper.AppShell"
       Shell.FlyoutBehavior="Disabled">
    <TabBar>
        <Tab Title="Sheet">
            <ShellContent ContentTemplate="{DataTemplate views:SheetPage}" />
        </Tab>
        <Tab Title="Gear">
            <ShellContent ContentTemplate="{DataTemplate views:GearPage}" />
        </Tab>
        <Tab Title="Notes">
            <ShellContent ContentTemplate="{DataTemplate views:NotesPage}" />
        </Tab>
    </TabBar>
</Shell>
```

Pages are created on demand via `DataTemplate` — this is how Shell works; do NOT instantiate pages manually.

### Pattern 2: CharacterViewModel with [ObservableProperty] and Computed Properties

**What:** Add all character fields as `[ObservableProperty]` backed fields. Computed properties (modifier, slot total) use `[NotifyPropertyChangedFor]` to chain notifications.
**When to use:** Any value that the UI reads and that changes at runtime.

```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty
public partial class CharacterViewModel : ObservableObject
{
    // Base stat — user can edit this
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalSTR))]
    [NotifyPropertyChangedFor(nameof(ModSTR))]
    [NotifyPropertyChangedFor(nameof(GearSlotTotal))]  // STR affects slot total
    private int baseSTR;

    // Computed total (base + bonuses filtered to STR)
    public int TotalSTR => BaseSTR + Bonuses
        .Where(b => b.BonusTo.StartsWith("STR:"))
        .Sum(b => ParseBonus(b.BonusTo));

    // Modifier: (total - 10) / 2, floored in C# integer division
    public int ModSTR => (TotalSTR - 10) / 2;

    // Gear slots: max(STR score, 10)
    public int GearSlotTotal => Math.Max(TotalSTR, 10);

    // HP — no floor enforced (negatives valid in Shadowdark)
    [ObservableProperty]
    private int currentHP;

    [ObservableProperty]
    private int maxHP;

    // Batch refresh after LoadCharacter() replaces Character
    public void LoadCharacter(Character character)
    {
        Character = character;
        BaseSTR = character.BaseSTR;
        // ... set all backing fields ...
        OnPropertyChanged(string.Empty); // notify all properties at once
    }
}
```

**Critical detail:** `OnPropertyChanged(string.Empty)` (or `""`) notifies the UI that ALL properties may have changed — the correct pattern for a full character replacement rather than calling `OnPropertyChanged(nameof(X))` for every property.

### Pattern 3: Stat Expand/Collapse with BindableLayout (NOT Expander in CollectionView)

**What:** Each stat row is a `VerticalStackLayout` with a tappable header row and a conditionally-visible sub-list rendered via `BindableLayout.ItemsSource`. Visibility is controlled by `IsExpanded` on a `StatRowViewModel`.
**When to use:** Any scenario requiring accordion-style inline expansion inside a scrollable vertical list. Avoids the known iOS Expander+CollectionView resize bug.

```xaml
<!-- The stat section is NOT a CollectionView — it's 6 static VerticalStackLayouts or a BindableLayout
     over an ObservableCollection<StatRowViewModel> in a parent VerticalStackLayout (not CollectionView) -->
<VerticalStackLayout BindableLayout.ItemsSource="{Binding StatRows}">
    <BindableLayout.ItemTemplate>
        <DataTemplate x:DataType="vm:StatRowViewModel">
            <VerticalStackLayout>
                <!-- Tappable header row: StatName | TotalScore | Modifier -->
                <Grid>
                    <Grid.GestureRecognizers>
                        <TapGestureRecognizer Command="{Binding ToggleExpandCommand}" />
                    </Grid.GestureRecognizers>
                    <Label Text="{Binding StatName}" />
                    <Label Text="{Binding TotalScore}" />
                    <Label Text="{Binding ModifierDisplay}" />
                </Grid>
                <!-- Bonus source sub-rows, visible only when expanded -->
                <VerticalStackLayout IsVisible="{Binding IsExpanded}"
                                     BindableLayout.ItemsSource="{Binding BonusSources}">
                    <BindableLayout.ItemTemplate>
                        <DataTemplate x:DataType="models:BonusSource">
                            <HorizontalStackLayout>
                                <Label Text="{Binding Label}" />
                                <Label Text="{Binding BonusTo}" />
                            </HorizontalStackLayout>
                        </DataTemplate>
                    </BindableLayout.ItemTemplate>
                </VerticalStackLayout>
            </VerticalStackLayout>
        </DataTemplate>
    </BindableLayout.ItemTemplate>
</VerticalStackLayout>
```

`StatRowViewModel` holds: `StatName`, `TotalScore`, `ModifierDisplay`, `IsExpanded`, `ObservableCollection<BonusSource> BonusSources`, `[RelayCommand] ToggleExpand()`.

### Pattern 4: CommunityToolkit.Maui Popup for Gear/Attack Edit

**What:** A `ContentView`-based popup shown via `this.ShowPopupAsync()` (or `Shell.Current.ShowPopupAsync()`). The popup receives a `GearItemViewModel` as its `BindingContext`.
**When to use:** Any modal form that collects a small set of fields and returns a result.

```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/popup
// From GearPage.xaml.cs (code-behind, since ShowPopupAsync requires a Page reference)
async void OnEditItemTapped(GearItemViewModel item)
{
    var popup = new GearItemPopup(item);  // Pass existing item (or new empty item for Add)
    await this.ShowPopupAsync(popup, PopupOptions.Empty);
    // After popup closes, the ObservableCollection already updated via the shared reference
}
```

The Popup XAML is a `ContentView` with Entry fields bound to a `GearItemViewModel` passed into the constructor. The Delete button calls `CharacterViewModel.RemoveGear(item)` then closes the popup.

**Important:** CommunityToolkit.Maui 14's Popup API uses `ShowPopupAsync()` as an extension method on `ContentPage` or `Shell.Current`. The older `Popup` base class approach still works but the extension method is the preferred 2025 pattern.

### Pattern 5: DI Injection of CharacterViewModel into Tab Pages

**What:** Register each tab page in DI so the shell can inject the singleton `CharacterViewModel` via constructor.
**When to use:** All three tab pages need the same singleton ViewModel instance.

```csharp
// MauiProgram.cs additions
builder.Services.AddTransient<SheetPage>();
builder.Services.AddTransient<GearPage>();
builder.Services.AddTransient<NotesPage>();
// CharacterViewModel already registered as singleton
```

```csharp
// SheetPage.xaml.cs
public partial class SheetPage : ContentPage
{
    public SheetPage(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
```

Shell will resolve the page from DI when navigating, injecting the singleton ViewModel automatically.

### Pattern 6: GEAR-04 Coin Slot Calculation

**What:** Shadowdark rule — each denomination (GP, SP, CP) occupies 1 slot per 100 coins, but the first 100 of each denomination are free.
**Formula:** `CoinSlots = Math.Max(GP - 100, 0) / 100 + Math.Max(SP - 100, 0) / 100 + Math.Max(CP - 100, 0) / 100`
**Note:** Integer division in C# automatically floors, which is the correct rounding behavior.

```csharp
public int CoinSlots =>
    Math.Max(GP - 100, 0) / 100 +
    Math.Max(SP - 100, 0) / 100 +
    Math.Max(CP - 100, 0) / 100;

public int GearSlotsUsed =>
    GearItems.Sum(g => g.Slots) + CoinSlots;
```

Both `CoinSlots` and `GearSlotsUsed` need `[NotifyPropertyChangedFor]` or manual `OnPropertyChanged` triggers from the GP/SP/CP and GearItems properties.

### Pattern 7: Inline Tap-to-Edit (for base stat score and max HP)

**What:** Toggle between a `Label` and an `Entry` on tap. The `Label` shows the value; tapping shows the `Entry`, which saves on `Completed` or focus loss.
**Implementation:** Use `IsVisible` on both controls driven by a `bool IsEditing` property on the ViewModel (or inline in code-behind for simplicity — this is Claude's Discretion territory).

```xaml
<!-- Label visible when not editing -->
<Label Text="{Binding BaseSTR}"
       IsVisible="{Binding IsEditingSTR, Converter={StaticResource InverseBoolConverter}}">
    <Label.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding StartEditSTRCommand}" />
    </Label.GestureRecognizers>
</Label>
<!-- Entry visible when editing -->
<Entry Text="{Binding BaseSTR, Mode=TwoWay}"
       IsVisible="{Binding IsEditingSTR}"
       Keyboard="Numeric"
       ReturnCommand="{Binding CommitEditSTRCommand}" />
```

**Simpler alternative (Claude's Discretion):** Use a single `Entry` for identity fields and stat base values without a Label toggle — just show the Entry always. This removes the toggle complexity entirely and is acceptable for a utility app.

### Anti-Patterns to Avoid

- **Expander inside CollectionView:** Produces iOS resize bugs where the CollectionView cell does not shrink when the Expander collapses (issue #1557, #1670). Use `BindableLayout` in a `VerticalStackLayout` instead.
- **Serializing CharacterViewModel:** Established in Phase 1 — always use `CharacterSaveData` DTO. Never add `[JsonInclude]` to the ViewModel.
- **Creating new ViewModel instances in tab pages:** All tabs must share the singleton. Never `new CharacterViewModel()` in a page constructor.
- **Not refreshing after LoadCharacter:** If `LoadCharacter()` doesn't call `OnPropertyChanged(string.Empty)` (or individual property notifiers), the UI will show stale data after file load.
- **Using STR from Character.Stats vs BaseSTR:** The Shadowdarklings import sets `BaseSTR` (rolled stats), not a final computed stat. Always derive the display total by adding bonuses at display time, not from a stored "final" value.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Observable property boilerplate | Manual `INotifyPropertyChanged` with backing fields | `[ObservableProperty]` from CommunityToolkit.Mvvm | Source-generated, null-safe, generates partial hooks |
| Modal edit dialog | Custom overlay with `AbsoluteLayout` | `CommunityToolkit.Maui` Popup | Cross-platform, handles dismiss-on-outside-tap, lifecycle correct |
| Command pattern | Manual `ICommand` implementations | `[RelayCommand]` attribute | Source-generated, handles CanExecute automatically |
| Expand/collapse accordion | Custom animation with `HeightRequest` tweening | `IsVisible` bool on a `VerticalStackLayout` containing the sub-list | No animation needed; visibility toggle is instant and reliable |
| Stat modifier math | Custom clamping logic | `(TotalSTR - 10) / 2` integer division | C# integer division already floors; no `Math.Floor` needed |
| Coin slot math | Custom parsing | Pure integer expression per GEAR-04 formula above | Single-line expression, no library needed |

---

## Common Pitfalls

### Pitfall 1: LoadCharacter Not Notifying UI
**What goes wrong:** After loading a character file, all fields on screen remain showing the previous character (or empty defaults).
**Why it happens:** `CharacterViewModel.LoadCharacter()` replaces the backing `Character` object but none of the `[ObservableProperty]` backing fields are updated, and no property change notifications fire.
**How to avoid:** In `LoadCharacter()`, assign every `[ObservableProperty]` backing field from the new character object, then call `OnPropertyChanged(string.Empty)` to notify all bindings at once. Also rebuild `ObservableCollection<GearItemViewModel>` by clearing and re-adding.
**Warning signs:** UI shows old data or default values after file open.

### Pitfall 2: Expander Resize Bug on iOS Inside Scrollable Containers
**What goes wrong:** When a stat row Expander collapses on iOS, the CollectionView cell retains the expanded height, creating whitespace and overlap.
**Why it happens:** Known open bug in CommunityToolkit.Maui Expander when nested inside ListView or CollectionView (issues #1557, #1670).
**How to avoid:** Do not use `Expander` inside `CollectionView` or `ListView`. Use `BindableLayout.ItemsSource` on a `VerticalStackLayout` for the stat list, and `IsVisible` on the bonus sub-list.
**Warning signs:** Collapsed stat rows have excessive whitespace on iOS; Android looks fine.

### Pitfall 3: GearItems and MagicItems as Two Separate Collections
**What goes wrong:** Slot count computation treats them separately; UI shows two separate lists.
**Why it happens:** `Character` model has `List<GearItem> Gear` and `List<MagicItem> MagicItems` as distinct collections.
**How to avoid:** In `CharacterViewModel`, create a single `ObservableCollection<GearItemViewModel>` that combines both lists (projecting `MagicItem` to match `GearItemViewModel`). Track source type internally if needed for saving back (or unify the save DTO — MagicItem only adds "note" which GearItem already has). The planner must decide: (a) unify DTO types, or (b) keep both and merge to unified ViewModel collection at load time.
**Warning signs:** Slot counter doesn't include magic item slots.

### Pitfall 4: BonusTo Parsing for Stats vs AC
**What goes wrong:** AC bonuses (prefixed "AC:") appear in stat drill-downs.
**Why it happens:** Phase 1 decision: all bonuses go in one list, differentiated by `BonusTo` prefix. Phase 2 must filter by prefix.
**How to avoid:** Filter `Character.Bonuses` by `b.BonusTo.StartsWith("STR:")` etc. when computing stat totals and populating `StatRowViewModel.BonusSources`. AC-prefixed bonuses are ignored in Phase 2 (no AC display requirement).
**Warning signs:** Armor items show up in STR or DEX breakdown.

### Pitfall 5: Singleton ViewModel Page Registration
**What goes wrong:** Each tab page creates a new `CharacterViewModel`, so changes on one tab do not appear on another.
**Why it happens:** Page registered as transient but ViewModel not injected — page does `new CharacterViewModel()` inline.
**How to avoid:** Register all tab pages in DI as `AddTransient<SheetPage>()` etc. Accept `CharacterViewModel` (singleton) via constructor injection. Set `BindingContext = vm` in constructor.
**Warning signs:** HP change on Sheet tab not visible on refresh; gear changes not reflected in slot counter.

### Pitfall 6: Integer Division Sign for Negative Modifiers
**What goes wrong:** STR 8 gives modifier 0 instead of -1.
**Why it happens:** C# integer division truncates toward zero: `(8-10)/2 = -2/2 = -1` is actually correct. But `(9-10)/2 = -1/2 = 0` in C# (truncation), whereas Shadowdark math expects -1 for a 9.
**How to avoid:** Use explicit floor division: `(int)Math.Floor((TotalSTR - 10.0) / 2.0)`. This gives: 9 → -1, 8 → -1, 11 → 0, 12 → +1 — matching Shadowdark tables.
**Warning signs:** Odd stat scores give modifiers one higher than expected.

---

## Code Examples

### Shell TabBar (3 tabs)
```xaml
<!-- Source: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/tabs?view=net-maui-10.0 -->
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:TorchKeeper.Views"
       x:Class="TorchKeeper.AppShell"
       Shell.FlyoutBehavior="Disabled">
    <TabBar>
        <Tab Title="Sheet">
            <ShellContent ContentTemplate="{DataTemplate views:SheetPage}" />
        </Tab>
        <Tab Title="Gear">
            <ShellContent ContentTemplate="{DataTemplate views:GearPage}" />
        </Tab>
        <Tab Title="Notes">
            <ShellContent ContentTemplate="{DataTemplate views:NotesPage}" />
        </Tab>
    </TabBar>
</Shell>
```

### ObservableProperty with Dependent Computed Property
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(TotalSTR))]
[NotifyPropertyChangedFor(nameof(ModSTR))]
[NotifyPropertyChangedFor(nameof(GearSlotTotal))]
private int baseSTR;

public int TotalSTR => BaseSTR + /* sum of STR: bonuses */;
public int ModSTR => (int)Math.Floor((TotalSTR - 10.0) / 2.0);
public int GearSlotTotal => Math.Max(TotalSTR, 10);
```

### Popup Show Pattern (CommunityToolkit.Maui 14+)
```csharp
// Source: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/popup
// In GearPage.xaml.cs — must be called from a ContentPage
async void OnItemTapped(object sender, TappedEventArgs e)
{
    var item = (GearItemViewModel)((View)sender).BindingContext;
    var popup = new GearItemPopup(item);
    await this.ShowPopupAsync(popup, PopupOptions.Empty);
}
```

### Notifying All Properties After Batch Update
```csharp
// Standard pattern for replacing the entire model
public void LoadCharacter(Character character)
{
    Character = character;
    baseSTR = character.BaseSTR;   // set backing fields directly (bypass setter equality check)
    baseDEX = character.BaseDEX;
    // ... all other backing fields ...
    currentHP = character.CurrentHP;
    maxHP = character.MaxHP;
    gp = character.GP;
    sp = character.SP;
    cp = character.CP;
    // Rebuild collections
    GearItems.Clear();
    foreach (var g in character.Gear)
        GearItems.Add(new GearItemViewModel(g, GearItemSource.Gear));
    foreach (var m in character.MagicItems)
        GearItems.Add(new GearItemViewModel(m, GearItemSource.Magic));
    // Notify everything
    OnPropertyChanged(string.Empty);
}
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual `INotifyPropertyChanged` with backing field boilerplate | `[ObservableProperty]` source generator | CommunityToolkit.Mvvm 8.x | Eliminates 80% of ViewModel boilerplate |
| `TabbedPage` for tab navigation | `Shell` with `TabBar` | MAUI GA (2022) | TabbedPage still works but Shell is the standard; TabbedPage docs note Shell is preferred |
| `ListView` for lists | `CollectionView` | MAUI (2022+) | CollectionView is more performant and flexible; ListView is legacy |
| Popup via `PushModalAsync` to a full `ContentPage` | `CommunityToolkit.Maui` Popup with `ShowPopupAsync` | CommunityToolkit.Maui ~1.0 | Lighter-weight; better dismiss behavior; no full navigation push |
| `Popup` base class (old CT.Maui) | `ContentView`-based popup shown via extension method | CommunityToolkit.Maui ~2.0 | Extension method approach is current; `ShowPopupAsync` on `Page` or `Shell.Current` |

**Deprecated/outdated:**
- `TabbedPage`: Still functional but not recommended for new apps; use Shell TabBar.
- `ListView`: Superseded by `CollectionView`; avoid for new UI.
- Manual `ICommand` classes: Use `[RelayCommand]` from CommunityToolkit.Mvvm.
- Old CommunityToolkit.Maui `Popup` base class pattern: Current API is `ContentView` + `ShowPopupAsync()` extension.

---

## Open Questions

1. **Gear model unification strategy**
   - What we know: `GearItem` and `MagicItem` have near-identical shape (name, slots, note); MagicItem lacks `ItemType`.
   - What's unclear: Should the planner unify them in `TorchKeeper.Core` (add `ItemType` to `MagicItem` and treat as one type), or keep them separate and merge only in the ViewModel layer?
   - Recommendation: Merge at the ViewModel layer only using a `GearItemViewModel(GearItem | MagicItem, source)` constructor overload. Keep Core models unchanged to avoid breaking Phase 1 save/load logic. When saving back, split by the `source` enum field.

2. **Identity field editing: inline vs. always-editable**
   - What we know: This is Claude's Discretion. Inline toggle adds Label/Entry switching complexity; always-show-Entry is simpler.
   - What's unclear: Whether the clean visual of a label vs. Entry matters for a utility-style app.
   - Recommendation: Use always-visible `Entry` controls for identity fields. This matches the stat base-value editing (inline tap-to-edit) only for the numeric stat field where it matters most (large tap targets). Text fields like name/class are fine as persistent Entry controls.

3. **StatRowViewModel placement**
   - What we know: `CharacterViewModel` is a singleton. `StatRowViewModel` instances per stat need to live somewhere accessible to the Sheet tab.
   - What's unclear: Should `StatRowViewModel` instances be properties on `CharacterViewModel`, or created by `SheetPage`?
   - Recommendation: Make `StatRows` an `ObservableCollection<StatRowViewModel>` property on `CharacterViewModel`. This ensures `LoadCharacter()` can rebuild them when a new character loads, and the singleton propagation works correctly.

---

## Validation Architecture

> nyquist_validation is enabled in .planning/config.json

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 (already installed in TorchKeeper.Tests) |
| Config file | TorchKeeper.Tests/TorchKeeper.Tests.csproj |
| Quick run command | `dotnet test TorchKeeper.Tests/ --filter "Category=Unit" -x` |
| Full suite command | `dotnet test TorchKeeper.Tests/` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| STAT-01 | Modifier computed correctly from base stat (incl. floor for odd values below 10) | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~StatModifier" -x` | ❌ Wave 0 |
| STAT-02 | Bonus sources filtered by stat prefix (STR: vs AC:) | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~BonusFilter" -x` | ❌ Wave 0 |
| GEAR-03 | Slot total = max(STR score, 10) at various STR values | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~GearSlotTotal" -x` | ❌ Wave 0 |
| GEAR-04 | Coin slots = correct formula for GP/SP/CP at boundaries (0, 100, 101, 200, 201) | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~CoinSlots" -x` | ❌ Wave 0 |
| HITP-01 | CurrentHP can go below 0 (no floor) | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~NegativeHP" -x` | ❌ Wave 0 |
| IDNT-01 | LoadCharacter populates all identity fields | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~LoadCharacter" -x` | ❌ Wave 0 |
| GEAR-01 | GearItemViewModel correctly unifies GearItem and MagicItem | unit | `dotnet test TorchKeeper.Tests/ --filter "FullyQualifiedName~GearItemViewModel" -x` | ❌ Wave 0 |
| NOTE-01, ATCK-01, IDNT-02, HITP-02, CURR-01, GEAR-02 | UI-only binding — no pure logic to isolate | manual-only | N/A — verified by inspection in the app | N/A |

**Manual-only justification:** The UI binding tests (Entry shows correct value, Popup opens on tap, Editor saves on blur) require the MAUI runtime and cannot run in the headless xUnit test project which targets `net10.0`, not `net10.0-android` etc.

### Sampling Rate
- **Per task commit:** `dotnet test TorchKeeper.Tests/ --filter "Category=Unit" -x`
- **Per wave merge:** `dotnet test TorchKeeper.Tests/`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps

- [ ] `TorchKeeper.Tests/ViewModels/CharacterViewModelTests.cs` — covers STAT-01 (modifier math), STAT-02 (bonus filter), GEAR-03 (slot total), GEAR-04 (coin slots), HITP-01 (negative HP), IDNT-01 (LoadCharacter population)
- [ ] `TorchKeeper.Tests/ViewModels/GearItemViewModelTests.cs` — covers GEAR-01 (unified GearItem/MagicItem wrapper)

Note: Both test files target `TorchKeeper.Core` and `TorchKeeper` assemblies. The test project already has `ProjectReference` to `TorchKeeper.Core`. The planner must also add a `ProjectReference` to `TorchKeeper` (the MAUI app project) for the ViewModel tests — or extract ViewModel logic into Core. Given MAUI-specific DI (`ObservableObject` from CommunityToolkit.Mvvm), the test project will need `CommunityToolkit.Mvvm` added as a package reference, or ViewModels should be extracted to a separate class library. **Simplest path:** Add `CommunityToolkit.Mvvm` NuGet ref to the test project and reference the main project directly. Test MAUI-free ViewModel logic (pure C# computed properties) only; skip UI event tests.

---

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn: Shell Tabs (.NET MAUI 10)](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/tabs?view=net-maui-10.0) — TabBar XAML structure, implicit conversion operators, page-on-demand via DataTemplate
- [Microsoft Learn: ObservableProperty attribute](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty) — `[NotifyPropertyChangedFor]`, partial hooks, `[property:]` target
- [Microsoft Learn: CommunityToolkit.Maui Popup](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/popup) — `ShowPopupAsync()`, `ClosePopupAsync()`, lifecycle behavior
- [Microsoft Learn: CommunityToolkit.Maui Expander](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/expander) — `IsExpanded`, `HandleHeaderTapped`, explicit CollectionView warning

### Secondary (MEDIUM confidence)
- [CommunityToolkit/Maui Issue #1557: Expander does not resize in CollectionView](https://github.com/CommunityToolkit/Maui/issues/1557) — confirmed open bug; BindableLayout workaround recommended
- [CommunityToolkit/Maui Issue #1670: Expander not working in iOS inside CollectionView](https://github.com/CommunityToolkit/Maui/issues/1670) — iOS-specific; Android unaffected
- [dotnet/maui Issue #23033: CollectionView resize on expand/collapse on iOS](https://github.com/dotnet/maui/issues/23033) — underlying MAUI issue

### Tertiary (LOW confidence — informational only)
- WebSearch results for MAUI DI + ContentPage constructor pattern — consistent with official docs; verified against Microsoft Learn DI article

---

## Metadata

**Confidence breakdown:**
- Standard Stack: HIGH — all packages are already installed; versions confirmed from csproj
- Architecture (TabBar, ObservableProperty): HIGH — verified against official .NET MAUI 10 and CommunityToolkit docs
- Popup pattern: HIGH — verified against CommunityToolkit.Maui 14 docs (installed version)
- Expander + CollectionView pitfall: HIGH — multiple open GitHub issues with active community discussion
- Coin slot formula / modifier math: HIGH — derived from Shadowdark rules stated in REQUIREMENTS.md and CONTEXT.md
- Test infrastructure: MEDIUM — test project exists and uses xUnit 2.9.3; ViewModel test file additions are new Wave 0 gaps

**Research date:** 2026-03-14
**Valid until:** 2026-09-14 (stable APIs; shorter if CommunityToolkit.Maui Popup API changes in major version)
