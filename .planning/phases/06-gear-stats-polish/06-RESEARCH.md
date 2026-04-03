# Phase 6 Research: Gear & Stats Polish

**Researched:** 2026-03-28
**Domain:** .NET MAUI / CommunityToolkit.Mvvm, Shadowdark RPG encumbrance rules
**Confidence:** HIGH — all findings based on direct PDF and source code reads

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Free-Carry Item Mechanism (GEAR-01)**
- D-01: Hybrid approach — auto-mark known free-carry items by name AND user-settable "Free Carry" checkbox in GearItemPopup.
- D-02: Known free-carry names to auto-mark: Backpack, Bag of Coins, Thieves Tools.
- D-03: Free-carry items appear in a separate "Free Carry" section below regular gear items.
- D-04: Checkbox/flag label is "Free Carry".
- D-05: Free-carry items are excluded from GearSlotsUsed — they consume 0 slots.

**Gear List Display (GEAR-01)**
- D-06: Gear list splits into two sections: regular gear above, "Free Carry" section below.

**Markdown Export (GEAR-02)**
- D-07: Markdown export mirrors UI layout — regular gear table first, then separate "Free Carry" section below.
- D-08: MarkdownExportService already reads vm.GearSlotsUsed and vm.CoinSlots — export parity is automatic once slot calc is fixed.
- D-09: Add a test verifying coin slot count in CharacterViewModel.GearSlotsUsed matches slot count parsed from MarkdownBuilder output.

**Stat Drill-Down (STAT-01)**
- D-10: Base stat value appears in expanded panel only — not in collapsed row header.
- D-11: Base stat labeled "Base: 14" in expanded panel.
- D-12: Base stat row appears first in the expanded panel, above bonus source rows.

**Talents Import**
- D-13: Import populates Talents field from levels[].talentRolledDesc, formatted as "Lv{N}: {description}". Empty entries skipped.
- D-14: SpellsKnown top-level field continues to map to Spells field (no change).
- D-15: ShadowdarklingsJson DTO needs a Levels list property; add SdLevelEntry class with Level (int) and TalentRolledDesc (string).

### Claude's Discretion
- Ordering of "Base: N" row if panel has no bonuses (no bonuses case — include Base row anyway, it always shows).
- Whether Rolled12ChosenTalentDesc should be included in Talents import — Claude reads example data and includes if non-empty.
- Whether free-carry auto-detection is case-insensitive — recommended: yes.

### Deferred Ideas (OUT OF SCOPE)
None — discussion stayed within phase scope.
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| GEAR-01 | Gear slot count is rules-accurate — free-carry items consume 0 slots, coin weight matches Shadowdark rules | PDF confirms Backpack (0 slots), Coin (first 100 free per denomination). Research identifies all touch points: GearItem model, GearItemViewModel, CharacterViewModel.GearSlotsUsed, CharacterSaveData, GearPage UI, GearItemPopup. |
| GEAR-02 | Exported Markdown gear section shows the same slot count as UI display | MarkdownExportService.MapToExportData already reads vm.GearSlotsUsed — fixing GEAR-01 calculation makes GEAR-02 automatic. MarkdownBuilder.BuildMarkdown needs a Free Carry section appended. CharacterExportData needs a FreeCarryItems list. |
| STAT-01 | Stat drill-down shows the raw base stat value alongside modifier and bonus sources | StatRowViewModel already holds BaseStat. SheetPage.xaml expanded panel only needs a new Label bound to BaseStat, inserted before the BonusSources BindableLayout. |
</phase_requirements>

---

## Shadowdark Rules Findings

**Source:** Shadowdark RPG V4-9.pdf, pages 34–36 (Gear and Gear Slots sections), read directly.

### Gear Slots Rule (page 35)

> "You can carry a number of items equal to your Strength stat or 10, whichever is higher."
> "Unless noted, all gear besides typical clothing fills one gear slot. Gear that is hard to transport might fill more than one slot."

This confirms the existing `GearSlotTotal = Math.Max(TotalSTR, 10)` formula is correct.

### Free-Carry Items — Confirmed from Basic Gear Table (page 35)

The "Quantity Per Gear Slot" column in the official table uses the phrase "first one free to carry" and "first 100 free to carry" to signal slotless items:

| Item | Quantity Per Gear Slot | Free? |
|------|----------------------|-------|
| Backpack | 1 (first one free to carry) | YES — 0 slots |
| Coin | 100 (first 100 free to carry) | YES — first 100 of each denomination free |
| Caltrops (one bag) | 1 | NO — 1 slot |

**Critical finding:** The PDF free-carry table lists only two explicit "free to carry" items:
1. **Backpack** — "1 (first one free to carry)" — exactly one backpack is always free.
2. **Coin** — "100 (first 100 free to carry)" — the first 100 coins of each denomination are free.

**"Bag of Coins" and "Thieves Tools" are NOT listed in the PDF as explicit free-carry items.** The PDF gear table does not mention them with a free-carry notation. They are decisions locked in D-02 from the user discussion — include them in the auto-detect list per D-02, but their "free carry" status is a design choice by the user, not a rulebook mandate.

### Coin Weight Rule — Confirmed (page 35)

From the table: Coin — "100 (first 100 free to carry)".

The existing implementation in `CharacterViewModel.CoinSlots` (lines 89-92):
```
public int CoinSlots =>
    (GP  > 100 ? (GP  - 1) / 100 : 0) +
    (SP  > 100 ? (SP  - 1) / 100 : 0) +
    (CP  > 100 ? (CP  - 1) / 100 : 0);
```

This implements: first 100 of each denomination free, each additional 100 (or part thereof) = 1 slot. The formula `(coins - 1) / 100` is correct integer ceiling for coins > 100. **This calculation is already correct — no change needed.**

### Crawling Kit table (page 36) — Additional Confirmation

The Crawling Kit breakdown on page 36 lists Backpack with 0 gear slots, confirming 0-slot status of the Backpack item.

### Other Free-Carry Exceptions

No other items in the visible PDF pages are marked "free to carry." The PDF does not mention Thieves Tools or Bag of Coins in a gear slot context. The three names in D-02 (Backpack, Bag of Coins, Thieves Tools) are the user's authoritative list — implement them as-is.

---

## Current Code State

### `TorchKeeper.Core/Models/GearItem.cs` (lines 1-9)

Current shape:
```csharp
public class GearItem
{
    public string Name { get; set; } = "";
    public int Slots { get; set; } = 1;
    public string ItemType { get; set; } = "";
    public string Note { get; set; } = "";
}
```

**Needs:** Add `public bool IsFreeCarry { get; set; }`.

### `TorchKeeper.Core/Models/MagicItem.cs` (lines 1-8)

Current shape:
```csharp
public class MagicItem
{
    public string Name { get; set; } = "";
    public int Slots { get; set; } = 1;
    public string Note { get; set; } = "";
}
```

**Needs:** Add `public bool IsFreeCarry { get; set; }` — magic items can also be free-carry by user choice. Without this, the checkbox in GearItemPopup cannot persist for magic items.

### `TorchKeeper.Core/DTOs/CharacterSaveData.cs` (lines 59-73)

Current `GearItemData`:
```csharp
public class GearItemData
{
    public string Name { get; init; } = "";
    public int Slots { get; init; } = 1;
    public string ItemType { get; init; } = "";
    public string Note { get; init; } = "";
}
```

**Needs:** Add `public bool IsFreeCarry { get; init; }` to `GearItemData`.

Current `MagicItemData`:
```csharp
public class MagicItemData
{
    public string Name { get; init; } = "";
    public int Slots { get; init; } = 1;
    public string Note { get; init; } = "";
}
```

**Needs:** Add `public bool IsFreeCarry { get; init; }` to `MagicItemData`.

`CharacterSaveData` is in `TorchKeeper.Core/DTOs/CharacterSaveData.cs` (the path shown in git status deletion was the old MAUI-project copy — the canonical version is now in Core).

### `TorchKeeper.Core/Services/CharacterFileService.cs`

`MapToDto` (line 74): maps `character.Gear` to `GearItemData` — must add `IsFreeCarry = g.IsFreeCarry`.
`MapToDto` (line 83): maps `character.MagicItems` to `MagicItemData` — must add `IsFreeCarry = m.IsFreeCarry`.
`MapFromDto` (line 130): maps `GearItemData` to `GearItem` — must add `IsFreeCarry = g.IsFreeCarry`.
`MapFromDto` (line 139): maps `MagicItemData` to `MagicItem` — must add `IsFreeCarry = m.IsFreeCarry`.

### `TorchKeeper/ViewModels/GearItemViewModel.cs` (lines 1-48)

Current: no `IsFreeCarry` property.

**Needs:**
- Add `[ObservableProperty] private bool isFreeCarry;`
- Constructor from `GearItem` (line 21): set `isFreeCarry = g.IsFreeCarry` and also auto-detect by name.
- Constructor from `MagicItem` (line 31): set `isFreeCarry = m.IsFreeCarry`.
- Constructor for new user-created item (line 41): default `isFreeCarry = false`; after name is set, apply auto-detect.

**Auto-detect logic** (applies at construction time, overrideable by stored value):
```csharp
private static readonly HashSet<string> KnownFreeCarryNames =
    new(StringComparer.OrdinalIgnoreCase) { "Backpack", "Bag of Coins", "Thieves Tools" };

public static bool IsKnownFreeCarry(string name) => KnownFreeCarryNames.Contains(name.Trim());
```

The stored `IsFreeCarry` value wins over auto-detect (user can override). On fresh import from Shadowdarklings where `IsFreeCarry` has no stored value (defaults to `false`), auto-detect fires because the model value is `false` and the name matches. Implementation: set `isFreeCarry = g.IsFreeCarry || IsKnownFreeCarry(g.Name)`.

### `TorchKeeper/ViewModels/CharacterViewModel.cs`

**`GearSlotsUsed` (line 93):**
```csharp
public int GearSlotsUsed => GearItems.Sum(g => g.Slots) + CoinSlots;
```

**Needs:** Filter out free-carry items:
```csharp
public int GearSlotsUsed => GearItems.Where(g => !g.IsFreeCarry).Sum(g => g.Slots) + CoinSlots;
```

**`OnGearItemChanged` (line 315-319):** Currently only listens for `Slots` changes. Must also listen for `IsFreeCarry` changes so the slot total recalculates when the checkbox is toggled:
```csharp
if (e.PropertyName is nameof(GearItemViewModel.Slots) or nameof(GearItemViewModel.IsFreeCarry))
    OnPropertyChanged(nameof(GearSlotsUsed));
```

**`BuildCharacterFromViewModel` (line 226):** The gear mapping selects only `GearItemSource.Gear` items. Must add `IsFreeCarry = g.IsFreeCarry` to both the Gear and MagicItems mappings (lines 226-231).

**`LoadCharacter` (line 280):** `GearItems.Add(new GearItemViewModel(g))` — no change needed here as the GearItemViewModel constructor will handle IsFreeCarry from the model.

### `TorchKeeper/Views/Popups/GearItemPopup.xaml` (lines 1-24)

Currently has: Name, Slots, Type, Note entries plus Save/Delete buttons.

**Needs:** Add a `CheckBox` with label "Free Carry" between Note and Save button.

XAML pattern to add:
```xml
<HorizontalStackLayout Spacing="8" VerticalOptions="Center">
    <CheckBox x:Name="FreeCarryCheckBox" />
    <Label Text="Free Carry" VerticalOptions="Center" />
</HorizontalStackLayout>
```

### `TorchKeeper/Views/Popups/GearItemPopup.xaml.cs` (lines 1-63)

**Constructor (edit existing, line 17):** Pre-fill `FreeCarryCheckBox.IsChecked = item.IsFreeCarry`.
**`OnSave` (line 38):** Read `FreeCarryCheckBox.IsChecked` and set `_existingItem.IsFreeCarry` or pass it to new `GearItemViewModel`.
**Min slots enforcement (line 38):** Currently `slots = Math.Max(1, slots)`. When `IsFreeCarry` is true, 0 slots is valid — change to `slots = Math.Max(0, slots)` and let free-carry status drive the slot-exclusion logic in `GearSlotsUsed`, not the slot value itself. The item's `Slots` value remains user-editable for display purposes; GearSlotsUsed simply ignores free-carry items regardless of their Slots value.

### `TorchKeeper/Views/GearPage.xaml` (lines 1-71)

Currently has one `VerticalStackLayout` bound to `GearItems` (line 34).

**Needs:** Split into two BindableLayouts:
1. Regular gear section (existing) — bound to a new `RegularGearItems` computed property on the VM (or filter in XAML using a converter).
2. Free Carry section below — a new `Frame` with header "Free Carry" bound to `FreeCarryItems` computed property.

**Recommended approach:** Add two `ObservableCollection`-backed or computed properties on `CharacterViewModel`:
- `IEnumerable<GearItemViewModel> RegularGearItems` — items where `!IsFreeCarry`
- `IEnumerable<GearItemViewModel> FreeCarryItems` — items where `IsFreeCarry`

However, since `GearItems` is an `ObservableCollection` and MAUI `BindableLayout` does not support `Where` predicates natively, the cleanest implementation is to use two separate `ObservableCollection` properties maintained in sync, OR to use a single collection with a filter converter. The established pattern in this codebase avoids converters for list filtering — use two separate collections.

**Alternative (simpler):** Keep `GearItems` as-is, add a `[ObservableProperty]` `IsVisible` trick via `DataTrigger` in the template — but this still counts free-carry slots. The safest approach is two separate collections.

**Simplest viable approach:** Add computed `IEnumerable<GearItemViewModel>` properties that filter `GearItems`, then rebind when `GearItems` changes. Since `BindableLayout.ItemsSource` does not support change notifications on computed `IEnumerable` unless wrapped in `ObservableCollection`, the planner should use two separate maintained `ObservableCollection` properties: `RegularGearItems` and `FreeCarryItems`, both updated whenever `GearItems` changes or an item's `IsFreeCarry` changes.

### `TorchKeeper/Views/SheetPage.xaml` (lines 191-208)

Current expanded panel (lines 191-208):
```xml
<VerticalStackLayout IsVisible="{Binding IsExpanded}" Padding="16,0,0,8"
                     BindableLayout.ItemsSource="{Binding BonusSources}">
    <BindableLayout.ItemTemplate>
        <DataTemplate x:DataType="models:BonusSource">
            <HorizontalStackLayout Spacing="8">
                <Label Text="{Binding Label}" />
                <Label Text="{Binding BonusTo}" TextColor="Gray" />
            </HorizontalStackLayout>
        </DataTemplate>
    </BindableLayout.ItemTemplate>
</VerticalStackLayout>
```

**Needs:** Wrap in a `VerticalStackLayout` so the Base row can appear before the BonusSources list:
```xml
<VerticalStackLayout IsVisible="{Binding IsExpanded}" Padding="16,0,0,8">
    <!-- Base stat row — always first (D-12) -->
    <Label Text="{Binding BaseStat, StringFormat='Base: {0}'}" FontSize="13" TextColor="Gray" />
    <!-- Bonus sources below -->
    <VerticalStackLayout BindableLayout.ItemsSource="{Binding BonusSources}">
        ...existing template...
    </VerticalStackLayout>
</VerticalStackLayout>
```

`StatRowViewModel.BaseStat` (line 22) is already an `[ObservableProperty]` — no VM change needed for STAT-01. The XAML binding `{Binding BaseStat}` works directly.

### `TorchKeeper.Core/Export/CharacterExportData.cs` (lines 1-55)

Current `GearExportItem` (line 55): `public record GearExportItem(string Name, int Slots);`

**Needs:**
- Add `IsFreeCarry` to `GearExportItem`: `public record GearExportItem(string Name, int Slots, bool IsFreeCarry = false);`
- Add `FreeCarryItems` list to `CharacterExportData`: `public required IReadOnlyList<GearExportItem> FreeCarryItems { get; init; }`
- The `GearItems` list should contain only regular (non-free-carry) gear for export parity with the UI.

### `TorchKeeper/Services/MarkdownExportService.cs` (lines 59-62)

Current gear mapping (line 60):
```csharp
var gearItems = vm.GearItems
    .Select(g => new GearExportItem(g.Name, g.Slots))
    .ToList();
```

**Needs:**
```csharp
var gearItems = vm.GearItems
    .Where(g => !g.IsFreeCarry)
    .Select(g => new GearExportItem(g.Name, g.Slots))
    .ToList();

var freeCarryItems = vm.GearItems
    .Where(g => g.IsFreeCarry)
    .Select(g => new GearExportItem(g.Name, g.Slots, true))
    .ToList();
```

And pass `FreeCarryItems = freeCarryItems` to `CharacterExportData`.

### `TorchKeeper.Core/Export/MarkdownBuilder.cs` (lines 70-107)

Current gear section starts at line 71:
```csharp
sb.AppendLine($"## Gear ({data.GearSlotsUsed} / {data.GearSlotTotal} slots)");
```

**Needs:** After the main gear table (after line 107), add a Free Carry section:
```csharp
// Free Carry section (D-07)
if (data.FreeCarryItems.Count > 0)
{
    sb.AppendLine("### Free Carry");
    sb.AppendLine();
    foreach (var item in data.FreeCarryItems)
    {
        sb.AppendLine($"- {item.Name}");
    }
    sb.AppendLine();
}
```

The existing `GearItems` loop (line 80) already only iterates non-free-carry items after the service fix — the slot count in the header (`data.GearSlotsUsed`) will match because the VM already excludes free-carry items.

### `TorchKeeper.Core/DTOs/ShadowdarklingsJson.cs` (lines 1-77)

**Needs:** Add `Levels` property and `SdLevelEntry` class (D-15):
```csharp
public List<SdLevelEntry>? Levels { get; set; }
```

New class:
```csharp
public class SdLevelEntry
{
    public int Level { get; set; }
    public string TalentRolledDesc { get; set; } = "";
    public string Rolled12ChosenTalentDesc { get; set; } = "";
}
```

### `TorchKeeper.Core/Services/ShadowdarklingsImportService.cs` (lines 1-92)

**Needs:** After the `bonuses` mapping (line 45), build the Talents string from `sdJson.Levels` (D-13):
```csharp
var talents = "";
if (sdJson.Levels is { Count: > 0 })
{
    var lines = sdJson.Levels
        .OrderBy(l => l.Level)
        .SelectMany(l =>
        {
            var entries = new List<string>();
            if (!string.IsNullOrWhiteSpace(l.TalentRolledDesc))
                entries.Add($"Lv{l.Level}: {l.TalentRolledDesc.Trim()}");
            if (!string.IsNullOrWhiteSpace(l.Rolled12ChosenTalentDesc))
                entries.Add($"Lv{l.Level} (chosen): {l.Rolled12ChosenTalentDesc.Trim()}");
            return entries;
        })
        .ToList();
    talents = string.Join("\n", lines);
}
```

Then set `Talents = talents` in the returned `Character` object (line 89 area — currently `Talents` field is not set).

---

## Import Data Structure

### `levels` Array Shape (from examples)

Both `Brim.json` and `Ghellence Pnidd.json` confirm the following JSON structure:

```json
"levels": [
  {
    "level": 1,
    "talentRolledDesc": "Your Backstab deals +1 dice of damage",
    "talentRolledName": "BackstabIncrease",
    "Rolled12TalentOrTwoStatPoints": "",
    "Rolled12ChosenTalentDesc": "",
    "Rolled12ChosenTalentName": "",
    "HitPointRoll": 1,
    "stoutHitPointRoll": 1
  },
  ...
]
```

Key observed fields:
- `level` (int) — level number
- `talentRolledDesc` (string) — human-readable talent description; **empty string when no talent rolled**
- `Rolled12ChosenTalentDesc` (string) — filled when the player rolled a 12 and chose a specific talent; empty string otherwise
- `talentRolledName` (string) — internal name key (not human-readable, skip)
- `Rolled12ChosenTalentName` — internal name key (skip)

### Observed `talentRolledDesc` Values

**Brim.json (Thief, level 4):**
- Level 1: `"Your Backstab deals +1 dice of damage"` — non-empty, import
- Level 2: `""` — empty, skip
- Level 3: `"+2 to Strength, Dexterity, or Charisma stat"` — non-empty, import
- Level 4: `""` — empty, skip

Expected Talents output for Brim:
```
Lv1: Your Backstab deals +1 dice of damage
Lv3: +2 to Strength, Dexterity, or Charisma stat
```

**Ghellence Pnidd.json (Wizard, level 4):**
- Level 1: `"Learn one additional wizard spell of any tier you know"` — non-empty, import
- Level 2: `""` — empty, skip
- Level 3: `"Gain advantage on casting one spell you know"` — non-empty, import
- Level 4: `""` — empty, skip

Expected Talents output for Ghellence:
```
Lv1: Learn one additional wizard spell of any tier you know
Lv3: Gain advantage on casting one spell you know
```

### `Rolled12ChosenTalentDesc` — Claude's Discretion Resolution

Both example files show `"Rolled12ChosenTalentDesc": ""` for all levels. The field is always present but empty in these samples. Include it in the import: if non-empty, append as `"Lv{N} (chosen): {desc}"`. This adds future-proofing at zero cost since empty strings are skipped.

### `Backpack` in Shadowdarklings JSON

Both example files include `"Backpack"` in gear with `"slots": 0`:
```json
{ "name": "Backpack", "type": "sundry", "slots": 0, ... }
```

Shadowdarklings already exports Backpack with `slots: 0`. When imported, `GearItem.Slots` will be 0. The auto-detect by name (`IsFreeCarry = true`) is still needed because the user can also add a Backpack manually. The imported Slots value of 0 is consistent — the Free Carry flag is the canonical state, not the Slots value.

### `Coins` Gear Item in Shadowdarklings JSON

`Brim.json` contains a gear entry: `{ "name": "Coins", "type": "sundry", "slots": 1, ... }`. This is NOT a free-carry item per the rules — it is a display artifact from Shadowdarklings representing the coin weight slot. The app handles coin weight separately via `CoinSlots`. The planner should note this item may appear in imported gear and consume a slot — this is a pre-existing divergence not addressed in this phase.

---

## Implementation Approach

### GEAR-01: Free-Carry Slot Exclusion

Touch-point order (dependency order):

1. `GearItem.cs` — add `IsFreeCarry` bool (model layer, no deps)
2. `MagicItem.cs` — add `IsFreeCarry` bool (model layer, no deps)
3. `CharacterSaveData.cs` — add `IsFreeCarry` to `GearItemData` and `MagicItemData` (DTO layer)
4. `CharacterFileService.cs` — wire `IsFreeCarry` through `MapToDto` and `MapFromDto` (service layer)
5. `GearItemViewModel.cs` — add `[ObservableProperty] bool isFreeCarry`, auto-detect logic, update all three constructors
6. `CharacterViewModel.cs` — update `GearSlotsUsed` filter, update `OnGearItemChanged`, update `BuildCharacterFromViewModel` gear mapping
7. `GearItemPopup.xaml` — add Free Carry CheckBox
8. `GearItemPopup.xaml.cs` — wire CheckBox to model; adjust min-slot check
9. `GearPage.xaml` — split gear list into RegularGearItems / FreeCarryItems sections
10. `CharacterViewModel.cs` — add `RegularGearItems` and `FreeCarryItems` collections (or recompute helper)

### GEAR-02: Export Parity

Touch-point order (all depend on GEAR-01 being complete):

1. `CharacterExportData.cs` — add `FreeCarryItems` list; update `GearExportItem` record
2. `MarkdownExportService.cs` — split `vm.GearItems` into regular + free-carry when mapping
3. `MarkdownBuilder.cs` — add Free Carry section after main gear table

### STAT-01: Base Stat in Drill-Down

Single touch-point — no model changes needed:

1. `SheetPage.xaml` — wrap expanded panel in outer `VerticalStackLayout`, insert `"Base: {0}"` Label before BonusSources list

`StatRowViewModel.BaseStat` is already observable. No VM changes required.

### Talents Import

Touch-point order:

1. `ShadowdarklingsJson.cs` — add `Levels` property and `SdLevelEntry` class
2. `ShadowdarklingsImportService.cs` — build Talents string from levels, set on returned Character

---

## Test Strategy

### Existing Test Framework

- **Framework:** xUnit (confirmed from `TorchKeeper.Tests.csproj` build artifacts — xunit.core.dll, xunit.assert.dll)
- **Run command:** `dotnet test TorchKeeper.Tests/TorchKeeper.Tests.csproj`
- **Test files:** All in `TorchKeeper.Tests/`
- **Pattern:** Pure unit tests using test-local stubs for MAUI-dependent types; real service classes tested directly; `[Fact]` and `[Trait("Category", "Unit")]`

### Existing Test Files

| File | What It Tests |
|------|--------------|
| `Tests/Export/MarkdownBuilderTests.cs` | MarkdownBuilder.BuildMarkdown (20 tests) |
| `Tests/Services/CharacterFileServiceTests.cs` | Save/load round-trip, DTO version |
| `Tests/Services/ShadowdarklingsImportServiceTests.cs` | JSON import, currency, bonuses |
| `Tests/ViewModels/CharacterViewModelTests.cs` | Stat modifiers, coin slots, gear slot total |
| `Tests/ViewModels/GearItemViewModelTests.cs` | GearItem/MagicItem VM construction |

### New Tests to Add

**GEAR-01 tests (`Tests/ViewModels/CharacterViewModelTests.cs` or new file):**
```
GearSlotsUsed_FreeCarryItemExcluded — item with IsFreeCarry=true not counted
GearSlotsUsed_RegularItemCounted — item with IsFreeCarry=false is counted
GearSlotsUsed_MixedItems_OnlyRegularCounted — mix of free-carry and regular
GearItemViewModel_AutoDetectsBackpack — "Backpack" name → IsFreeCarry=true
GearItemViewModel_AutoDetectsCaseInsensitive — "backpack" → IsFreeCarry=true
GearItemViewModel_NonFreeCarryName_NotAutoDetected — "Sword" → IsFreeCarry=false
```

**GEAR-02 tests (`Tests/Export/MarkdownBuilderTests.cs`):**
```
BuildMarkdown_FreeCarrySection_AppearsWhenFreeCarryItemsPresent
BuildMarkdown_FreeCarrySection_OmittedWhenEmpty
BuildMarkdown_GearSlotCount_MatchesVmGearSlotsUsed — export parity test (D-09)
BuildMarkdown_RegularGear_ExcludesFreeCarryItems
```

**STAT-01:** No unit test needed — it is a pure XAML binding change. The existing `StatRowViewModel.BaseStat` is already tested implicitly through `TotalScore` and `ModifierDisplay`. No behavioral logic changes.

**Talents import tests (`Tests/Services/ShadowdarklingsImportServiceTests.cs`):**
```
Import_Levels_PopulatesTalentsField — verify Lv{N}: {desc} format
Import_Levels_SkipsEmptyTalentRolledDesc — empty strings not in output
Import_Levels_IncludesRolled12ChosenTalentDesc_WhenNonEmpty
Import_BrimJson_TalentsContainLevel1AndLevel3
```

**Round-trip test (`Tests/Services/CharacterFileServiceTests.cs`):**
```
RoundTrip_GearItem_IsFreeCarry_Persists — IsFreeCarry=true survives save/load
```

---

## Risk & Unknowns

### Risk 1: Two-Collection Pattern for GearPage — Complexity

The decision to show RegularGearItems and FreeCarryItems as two separate XAML sections means `CharacterViewModel` needs two maintained collections. The planner must ensure that:
- When `GearItems` collection changes (add/remove), both sub-collections update.
- When `IsFreeCarry` on an existing item changes (checkbox toggle), the item moves between collections.
- The "Add" button in GearPage adds to `GearItems` (not directly to a sub-collection).

A cleaner implementation: maintain only `GearItems` as the source of truth, and expose `RegularGearItems` and `FreeCarryItems` as `ObservableCollection<GearItemViewModel>` that are rebuilt on each change. This is slightly wasteful but avoids fragile sync logic. Given typical gear list sizes (< 20 items), performance is not a concern.

### Risk 2: `GearItemPopup` Min-Slot Enforcement

Current code: `slots = Math.Max(1, slots)` (line 38 in GearItemPopup.xaml.cs). If a user sets a free-carry item to 0 slots explicitly, the current enforcement would reset it to 1. Fix: `slots = Math.Max(0, slots)`. The slot value on free-carry items is informational only — `GearSlotsUsed` already ignores them.

### Risk 3: `Shadowdarklings` Backpack Import Creates Duplicate Detection

Imported characters from Shadowdarklings already have `Backpack` with `slots: 0`. When loaded into `GearItemViewModel`, the constructor sets `IsFreeCarry = g.IsFreeCarry || IsKnownFreeCarry(g.Name)`. Since `g.IsFreeCarry` defaults to `false` (not in current model or DTO), auto-detect fires correctly. After the phase adds `IsFreeCarry` to the model and DTO, newly saved characters will have `IsFreeCarry = true` explicitly. Old saves (pre-phase) will default to `false` and auto-detect will correct it on load. No migration needed.

### Risk 4: `Coins` Gear Item from Shadowdarklings

As noted in the Import Data section, Shadowdarklings exports a `"Coins"` gear item with `slots: 1`. This item will appear in the regular gear list and count as 1 slot even though coin weight is separately tracked by `CoinSlots`. This is a pre-existing issue not in scope for this phase. The planner should add a note in the plan acknowledging this.

### Risk 5: `MarkdownBuilderTests` — GearSlotCount Parity Test

D-09 calls for a test that "coin slot count in CharacterViewModel.GearSlotsUsed matches the slot count parsed from MarkdownBuilder output." The export parity test must construct a `CharacterExportData` with `GearSlotsUsed` set from a simulated VM and verify the markdown header `## Gear (N / M slots)` reflects the same value. This is purely a `MarkdownBuilderTests` addition — no VM instantiation needed in the test because `CharacterExportData` takes pre-computed values.

### Risk 6: `Rolled12ChosenTalentDesc` Casing

The JSON field uses PascalCase (`Rolled12ChosenTalentDesc`). `System.Text.Json` with `PropertyNameCaseInsensitive = true` (already set in `ShadowdarklingsImportService.ImportOptions`) will match this to a C# property named `Rolled12ChosenTalentDesc` regardless of casing. No special handling needed.

### Unknown: MagicItem Free Carry Persistence

The current `MagicItem` model has no `IsFreeCarry`. Magic items can theoretically be free-carry (user checkbox). The phase must add `IsFreeCarry` to `MagicItem` and `MagicItemData` for correct round-trip. Without this, toggling a magic item as free-carry would lose the flag on save/load.

---

## Sources

- `~/Downloads/Shadowdark_RPG_-_V4-9.pdf` pages 34-36 — Gear chapter, Basic Gear table, Gear Slots rule. HIGH confidence.
- Direct code reads of all listed files. HIGH confidence.
- `examples/Brim.json` and `examples/Ghellence Pnidd.json` — confirmed `levels[]` shape and `talentRolledDesc` population pattern. HIGH confidence.
