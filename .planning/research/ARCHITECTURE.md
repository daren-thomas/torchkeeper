# Architecture Patterns

**Domain:** .NET MAUI character sheet app (Shadowdark RPG)
**Researched:** 2026-03-08
**Overall confidence:** HIGH (Microsoft official docs, confirmed patterns)

---

## Recommended Architecture

Standard .NET MAUI MVVM with Shell navigation, CommunityToolkit.Mvvm source generators, and a service layer. The character model is the central shared state; all sections of the sheet bind to one `CharacterViewModel` that owns a single `Character` model instance.

```
┌─────────────────────────────────────────────────────────┐
│                        Views / Pages                     │
│  (XAML ContentPages — zero business logic in code-behind)│
│  StatsPage  |  GearPage  |  NotesPage  |  LevelPage     │
└────────────────────┬────────────────────────────────────┘
                     │  data binding (two-way where editable)
                     ▼
┌─────────────────────────────────────────────────────────┐
│                    ViewModels                            │
│  CharacterViewModel  (owns Character, exposes all state) │
│  ├── StatBlockViewModel  (per-stat bonus tree)           │
│  ├── GearViewModel       (slot math, gear list commands) │
│  └── ImportViewModel     (Shadowdarklings JSON parsing)  │
└────────────────────┬────────────────────────────────────┘
                     │  calls into
                     ▼
┌─────────────────────────────────────────────────────────┐
│                     Services                             │
│  ICharacterService  (save / load / export)               │
│  IImportService     (parse Shadowdarklings JSON)         │
│  IMarkdownExportService  (format character as Markdown)  │
└────────────────────┬────────────────────────────────────┘
                     │  reads/writes
                     ▼
┌─────────────────────────────────────────────────────────┐
│                     Models                               │
│  Character           (root aggregate)                    │
│  StatBlock           (base values + bonus sources)       │
│  BonusSource         (name, value, category, toggleable) │
│  GearItem / MagicItem                                    │
│  LevelRecord         (per-level talent/HP notes)         │
│  CoinLedger                                              │
└─────────────────────────────────────────────────────────┘
```

---

## Component Boundaries

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `AppShell.xaml` | Tab bar structure, route registration | Views |
| Views (ContentPage) | Layout, user input, visual feedback | CharacterViewModel via BindingContext |
| `CharacterViewModel` | Exposes all character state, computed properties, commands; coordinates saves | Models, Services |
| `StatBlockViewModel` | Per-stat drill-down: base value, each bonus source, running total, toggle state | CharacterViewModel (parent owns the instance) |
| `GearViewModel` | Gear list (ObservableCollection), slot math, add/edit/remove commands | CharacterViewModel |
| `ImportViewModel` | Parses Shadowdarklings JSON, maps to native Character model | IImportService |
| `ICharacterService` | Serialize/deserialize native `.sdchar` JSON; uses `FileSystem.AppDataDirectory` | File system |
| `IImportService` | Maps Shadowdarklings JSON shape to internal model | ICharacterService (hands result back) |
| `IMarkdownExportService` | Renders character as formatted Markdown string for clipboard/share | CharacterViewModel |
| Models | Pure data — no dependency on MAUI, services, or ViewModels | Nothing |

**Key rule:** Services have no knowledge of ViewModels. ViewModels have no knowledge of Views. Models know nothing else.

---

## Calculated Stats: How Bonus Sources Are Modeled

This is the most design-sensitive area of the app. The Shadowdarklings JSON reveals the pattern: a stat's final value is `rolledStats[stat] + sum of enabled bonuses where bonusTo targets that stat`.

### Model Shape

```csharp
// Pure model — no INotifyPropertyChanged here
public class BonusSource
{
    public string Name        { get; set; }   // e.g., "StatBonus (Thief Lv3)"
    public string BonusTo     { get; set; }   // e.g., "DEX" or "" (non-stat bonus)
    public int    Value       { get; set; }   // e.g., +2
    public string SourceType  { get; set; }   // "Class", "Ancestry", "Item", "Spell", "Situational"
    public string SourceCategory { get; set; } // "Talent", "Equipped", "Effect"
    public bool   IsToggleable { get; set; }  // false = always on; true = user can disable
}

public class StatValue
{
    public string Stat       { get; set; }   // "STR", "DEX", etc.
    public int    BaseValue  { get; set; }   // the rolled/entered base
    // Active bonus sources for this stat come from Character.Bonuses filtered by BonusTo
}

public class Character
{
    // Identity
    public string Name, Ancestry, Class, Title, Alignment, Background, Deity, Languages;
    public int Level, XP;

    // Stats
    public Dictionary<string, int> BaseStats { get; set; }  // rolled/entered base values

    // Bonus sources — all bonuses in one flat list, filtered by stat in the ViewModel
    public List<BonusSource> BonusSources { get; set; }

    // HP
    public int CurrentHp, MaxHp;

    // Gear
    public List<GearItem> Gear { get; set; }
    public List<MagicItem> MagicItems { get; set; }

    // Currency
    public int Gold, Silver, Copper;

    // Level history
    public List<LevelRecord> Levels { get; set; }

    // Notes
    public string Notes { get; set; }
    public string SpellsKnown { get; set; }
}
```

### ViewModel Shape for Stat Calculation

```csharp
// One instance per stat, owned by CharacterViewModel
public partial class StatBlockViewModel : ObservableObject
{
    private readonly Character _character;
    private readonly string _stat; // "DEX"

    public int BaseValue => _character.BaseStats[_stat];

    // All bonus sources that target this stat, projected as bindable rows
    public ObservableCollection<BonusSourceViewModel> BonusSources { get; }

    // Computed — recalculated whenever any BonusSource.IsEnabled changes
    public int Total => BaseValue + BonusSources
        .Where(b => b.IsEnabled)
        .Sum(b => b.Value);

    public int Modifier => (Total - 10) / 2; // Shadowdark formula: floor((total-10)/2)
}

public partial class BonusSourceViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Parent.Total))]   // propagate up
    private bool _isEnabled;

    public string Label { get; }  // e.g., "Thief Talent (Lv3): +2"
    public int Value    { get; }
}
```

**Data flow for stat total:**

```
User toggles BonusSource checkbox
  → BonusSourceViewModel.IsEnabled changes
  → StatBlockViewModel.Total recomputed (derived property)
  → View Label updates via binding
  → CharacterViewModel.IsDirty = true (triggers save-needed indicator)
```

The toggle state (`IsEnabled`) is part of the saved native file. Shadowdarklings JSON tracks which bonuses exist but not user-defined toggle state — that is app-only state.

---

## Navigation Structure

Use `Shell` with a `TabBar` (bottom tabs). The character sheet has five natural sections:

```xaml
<Shell>
  <TabBar>
    <ShellContent Title="Stats"    Icon="stats.png"    Route="stats"
                  ContentTemplate="{DataTemplate views:StatsPage}" />
    <ShellContent Title="Gear"     Icon="gear.png"     Route="gear"
                  ContentTemplate="{DataTemplate views:GearPage}" />
    <ShellContent Title="Level"    Icon="level.png"    Route="levels"
                  ContentTemplate="{DataTemplate views:LevelsPage}" />
    <ShellContent Title="Notes"    Icon="notes.png"    Route="notes"
                  ContentTemplate="{DataTemplate views:NotesPage}" />
    <ShellContent Title="Settings" Icon="settings.png" Route="settings"
                  ContentTemplate="{DataTemplate views:SettingsPage}" />
  </TabBar>
</Shell>
```

Modal/push routes registered separately:

```csharp
Routing.RegisterRoute("gear/edit",      typeof(GearItemEditPage));
Routing.RegisterRoute("stats/bonus",    typeof(StatBonusDetailPage));
Routing.RegisterRoute("import",         typeof(ImportPage));
```

**Tab breakdown:**

| Tab | Content | Notes |
|-----|---------|-------|
| Stats | Identity fields, 6 stat blocks with modifier + toggle-drill-down, HP tracker, XP, attacks | Primary play screen |
| Gear | Slot counter, gear list (swipe-to-delete), magic items, coin totals | Slot math live here |
| Level | Per-level records (HP rolled, talent chosen), level advancement notes | Append-only log feel |
| Notes | Freeform text area, spells known text | Simple ContentPage |
| Settings | Load / Save / Import / Export Markdown; file management | All I/O commands here |

**Why TabBar not FlyoutItem:** Character sheet use is breadth-first (jump between sections at the table), not depth-first. Bottom tabs match this interaction model. FlyoutItem better suits apps with dozens of sections.

**Stat drill-down navigation:**

```
StatsPage (tab)
  → tap stat row → push StatBonusDetailPage (shows base + each bonus source with toggles)
```

---

## File Format Design (Native `.sdchar`)

The native format is app-owned JSON. It is **not** the Shadowdarklings format — that is import-only. Design goals: round-trip all app state (including toggle states), be human-readable enough for manual recovery, forward-compatible via version field.

```json
{
  "version": 1,
  "name": "Brim Qu'arkh",
  "ancestry": "Dwarf",
  "class": "Thief",
  "level": 4,
  "title": "Cutthroat",
  "alignment": "Chaotic",
  "background": "Thieves' Guild",
  "deity": "Memnon",
  "languages": "Common, Dwarvish",
  "xp": 4,
  "currentHp": 10,
  "maxHp": 14,
  "baseStats": {
    "STR": 9, "DEX": 14, "CON": 7, "INT": 9, "WIS": 6, "CHA": 9
  },
  "bonusSources": [
    {
      "name": "Thief Talent Lv3: +2 DEX",
      "bonusTo": "DEX",
      "value": 2,
      "sourceType": "Class",
      "sourceCategory": "Talent",
      "isToggleable": false,
      "isEnabled": true
    }
  ],
  "gear": [
    {
      "id": "mhgjjidu",
      "name": "Shortbow",
      "type": "weapon",
      "slots": 1,
      "quantity": 1,
      "notes": ""
    }
  ],
  "magicItems": [
    {
      "id": "mly1yj10",
      "name": "Potion of Mind Reading",
      "slots": 1,
      "benefits": "Read minds of all creatures within near for 1h",
      "notes": ""
    }
  ],
  "gold": 120,
  "silver": 0,
  "copper": 0,
  "levels": [
    {
      "level": 1,
      "hpRolled": 1,
      "talentChosen": "Backstab deals +1 dice of damage",
      "notes": ""
    }
  ],
  "spellsKnown": "None",
  "notes": ""
}
```

**Key differences from Shadowdarklings format:**
- No `rolledStats` vs `stats` split — app uses a single `baseStats` (the actual entered base values)
- `bonusSources` are user-authored entries; `isEnabled` is persisted here
- No `ledger` array — coins stored as current totals, no transaction log needed
- No `gearId` lookup keys — items are self-contained records
- No `activeSources` (rulebook source tracking) — out of scope

**File location:** `FileSystem.AppDataDirectory` + `/<charactername>.sdchar`. On iOS/macOS this is the Library folder (iCloud-backed). On Android it is FilesDir (Auto Backup). On Windows it is LocalFolder.

**Import path:** Shadowdarklings JSON → `IImportService.Parse()` → `Character` model → `ICharacterService.Save()` → `.sdchar`

---

## Patterns to Follow

### Pattern 1: Single Shared CharacterViewModel via DI

All tab pages bind to the same `CharacterViewModel` instance, registered as singleton in the DI container. Do not create a new ViewModel per page.

```csharp
// MauiProgram.cs
builder.Services.AddSingleton<CharacterViewModel>();
builder.Services.AddSingleton<ICharacterService, CharacterService>();
builder.Services.AddSingleton<IImportService, ImportService>();
builder.Services.AddSingleton<IMarkdownExportService, MarkdownExportService>();

// Page registration — transient pages, singleton VM
builder.Services.AddTransient<StatsPage>();
builder.Services.AddTransient<GearPage>();
// ...
```

Pages receive the shared VM via constructor injection:

```csharp
public partial class StatsPage : ContentPage
{
    public StatsPage(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
```

**Why:** The character is one object. All sections display different views of the same state. A singleton VM ensures that editing a stat in StatsPage immediately reflects in gear slot math (gear slots = STR score).

### Pattern 2: CommunityToolkit.Mvvm Source Generators

Use `[ObservableProperty]` and `[RelayCommand]` on the CharacterViewModel to eliminate boilerplate. Partial classes required.

```csharp
[ObservableProperty]
private int _currentHp;

[RelayCommand]
private async Task SaveCharacterAsync()
{
    await _characterService.SaveAsync(_character);
    IsDirty = false;
}
```

### Pattern 3: Derived Properties for Computed Stats

Computed values (stat totals, modifiers, slot counts) are get-only properties that call `OnPropertyChanged` explicitly when their inputs change.

```csharp
public int StrTotal => BaseStats["STR"] + EnabledBonusesFor("STR").Sum(b => b.Value);
public int GearSlotsUsed => Gear.Sum(g => g.Slots) + CoinSlots;
public int CoinSlots => (Gold + Silver + Copper) / 20;  // Shadowdark rule: 20 coins = 1 slot
```

When a bonus is toggled or an item is added, the ViewModel manually fires:

```csharp
OnPropertyChanged(nameof(StrTotal));
OnPropertyChanged(nameof(GearSlotsUsed));
```

**Why not full reactive pipeline:** The bonus graph is shallow (one level of dependencies) and infrequently updated. A reactive framework (Rx.NET, ReactiveUI) adds complexity without meaningful payoff at this scale.

### Pattern 4: IsDirty Pattern for Save Prompts

Track unsaved state with a boolean. Prompt user on navigation-away or app suspend.

```csharp
[ObservableProperty]
private bool _isDirty;

// Called whenever any property that changes character state changes:
partial void OnCurrentHpChanged(int value) => IsDirty = true;
```

---

## Anti-Patterns to Avoid

### Anti-Pattern 1: Per-Tab ViewModel

**What:** Creating separate `StatsViewModel`, `GearViewModel` etc. as top-level independent ViewModels each loading their own copy of the character.

**Why bad:** Stat changes in StatsViewModel won't propagate to slot math in GearViewModel (gear slots = STR). Requires synchronization logic that introduces bugs.

**Instead:** One `CharacterViewModel` singleton; `StatBlockViewModel` and `GearViewModel` are child objects owned by it, not independent top-level services.

### Anti-Pattern 2: Storing Derived State in the Model

**What:** Saving computed values like `StatTotal`, `GearSlotsUsed`, `ArmorClass` in the `.sdchar` file.

**Why bad:** Stored derived state goes stale when inputs change. Creates subtle bugs when bonuses are toggled.

**Instead:** Store only raw inputs (base stats, bonus sources with enabled flags, gear slots). Recompute everything in the ViewModel on load.

### Anti-Pattern 3: Importing Shadowdarklings `stats` as Final Values

**What:** Using the `stats` field (post-bonus totals like `DEX: 16`) from the Shadowdarklings JSON as base stats.

**Why bad:** `stats` includes bonus modifiers already baked in. The correct base is `rolledStats` (e.g., `DEX: 14`). The +2 DEX is a `StatBonus` in `bonuses[]`. Importing `stats` double-counts bonuses.

**Instead:** In `IImportService`, map `rolledStats` to `baseStats`, then convert `bonuses[]` to `BonusSource` records.

### Anti-Pattern 4: Business Logic in Views

**What:** Calculating slot counts, computing modifiers, or toggling bonus state in XAML code-behind.

**Why bad:** Untestable, duplicated across platforms if platform-specific code branches are needed.

**Instead:** All calculations in the ViewModel. Views are pure bindings.

---

## Suggested Build Order

Dependencies flow downward; build lower layers first.

```
1. Models          — pure C# data classes, no MAUI dependencies
      └── BonusSource, GearItem, MagicItem, LevelRecord, Character

2. Services (interfaces + implementations)
      └── ICharacterService (JSON serialize/deserialize with System.Text.Json)
      └── IImportService (Shadowdarklings JSON → Character)
      └── IMarkdownExportService (Character → Markdown string)

3. CharacterViewModel
      └── Depends on: Models + Services
      └── Includes child StatBlockViewModel instances and GearViewModel

4. Shell + Tab Pages (layout, binding, no logic)
      └── StatsPage → binds to CharacterViewModel
      └── GearPage → binds to CharacterViewModel
      └── LevelsPage, NotesPage, SettingsPage

5. Detail / Modal Pages
      └── StatBonusDetailPage (drill-down for one stat's bonuses)
      └── GearItemEditPage (add/edit a gear item)
      └── ImportPage (file picker + import flow)

6. Export feature
      └── IMarkdownExportService implementation
      └── Share sheet integration (MAUI Share API)
```

**Phase implication:** Models + Services + CharacterViewModel (steps 1-3) are the core foundation. They can be built and unit-tested before any UI exists. Tab pages (step 4) are the first visible milestone. Detail pages (step 5) are the second. Export (step 6) is isolated and deferrable.

---

## Scalability Considerations

| Concern | Now (single character) | If multi-character later |
|---------|------------------------|--------------------------|
| State scope | Singleton CharacterViewModel | Add CharacterListViewModel; CharacterViewModel becomes per-instance |
| File management | One `.sdchar` in AppDataDirectory | Index file or directory scan |
| Navigation | TabBar is sufficient | Consider flyout for character switcher |

The singleton ViewModel pattern does not block a future multi-character model — it just needs to become a factory-produced instance rather than a singleton.

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Shell TabBar navigation | HIGH | Official .NET MAUI docs, confirmed patterns |
| MVVM with CommunityToolkit | HIGH | Official Microsoft recommendation, used in first-party Windows apps |
| File I/O via FileSystem.AppDataDirectory | HIGH | Official docs, cross-platform tested |
| Bonus toggle state modeling | MEDIUM | Derived from Shadowdarklings JSON structure; no existing app to reference for exact pattern |
| Native `.sdchar` format design | MEDIUM | App-defined; format is sound but will need iteration as features land |

---

## Sources

- [.NET MAUI MVVM Fundamentals](https://learn.microsoft.com/en-us/dotnet/maui/xaml/fundamentals/mvvm) — official Microsoft docs
- [Enterprise App Patterns: MVVM](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm) — Microsoft architecture guide
- [CommunityToolkit.Mvvm Introduction](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) — official CommunityToolkit docs
- [Shell Navigation](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation) — official .NET MAUI docs
- [Shell Tabs](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/tabs) — official .NET MAUI docs
- [File System Helpers](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers) — official .NET MAUI docs
- `examples/Brim.json` — Shadowdarklings export format reference (project file)
