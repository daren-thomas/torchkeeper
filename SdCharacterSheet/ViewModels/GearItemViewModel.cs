using CommunityToolkit.Mvvm.ComponentModel;
using SdCharacterSheet.Models;

namespace SdCharacterSheet.ViewModels;

public enum GearItemSource { Gear, Magic }

/// <summary>
/// Unified GearItem/MagicItem wrapper for the gear list.
/// ItemType is empty string for magic items (MagicItem has no ItemType field).
/// IsFreeCarry = true means the item is excluded from GearSlotsUsed (D-05).
/// Auto-detect fires when item name matches KnownFreeCarryNames (D-01, D-02).
/// </summary>
public partial class GearItemViewModel : ObservableObject
{
    [ObservableProperty] private string name = "";
    [ObservableProperty] private int slots;
    [ObservableProperty] private string itemType = "";
    [ObservableProperty] private string note = "";
    [ObservableProperty] private bool isFreeCarry;

    public GearItemSource Source { get; }

    // D-02: Known free-carry names. Case-insensitive (Claude's discretion).
    private static readonly HashSet<string> KnownFreeCarryNames =
        new(StringComparer.OrdinalIgnoreCase) { "Backpack", "Bag of Coins", "Thieves Tools" };

    public static bool IsKnownFreeCarry(string name) =>
        KnownFreeCarryNames.Contains(name.Trim());

    // Constructor from saved GearItem: stored IsFreeCarry wins; auto-detect corrects old saves.
    public GearItemViewModel(GearItem g)
    {
        name = g.Name;
        slots = g.Slots;
        itemType = g.ItemType;
        note = g.Note;
        isFreeCarry = g.IsFreeCarry || IsKnownFreeCarry(g.Name);
        Source = GearItemSource.Gear;
    }

    // Constructor from saved MagicItem: stored IsFreeCarry wins; auto-detect not applied
    // (magic items are not known free-carry items by default).
    public GearItemViewModel(MagicItem m)
    {
        name = m.Name;
        slots = m.Slots;
        itemType = "";   // MagicItem has no ItemType field
        note = m.Note;
        isFreeCarry = m.IsFreeCarry;
        Source = GearItemSource.Magic;
    }

    // Constructor for new user-created gear item. Auto-detect applies on name.
    public GearItemViewModel(string name, int slots, string itemType, string note, bool isFreeCarry = false)
    {
        this.name = name;
        this.slots = slots;
        this.itemType = itemType;
        this.note = note;
        this.isFreeCarry = isFreeCarry || IsKnownFreeCarry(name);
        Source = GearItemSource.Gear; // user-created items are mundane gear by default
    }
}
