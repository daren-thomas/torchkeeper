using CommunityToolkit.Mvvm.ComponentModel;
using SdCharacterSheet.Models;

namespace SdCharacterSheet.ViewModels;

public enum GearItemSource { Gear, Magic }

/// <summary>
/// Unified GearItem/MagicItem wrapper for the gear list.
/// ItemType is empty string for magic items (MagicItem has no ItemType field).
/// </summary>
public partial class GearItemViewModel : ObservableObject
{
    [ObservableProperty] private string name = "";
    [ObservableProperty] private int slots;
    [ObservableProperty] private string itemType = "";
    [ObservableProperty] private string note = "";

    public GearItemSource Source { get; }

    public GearItemViewModel(GearItem g)
    {
        name = g.Name;
        slots = g.Slots;
        itemType = g.ItemType;
        note = g.Note;
        Source = GearItemSource.Gear;
    }

    public GearItemViewModel(MagicItem m)
    {
        name = m.Name;
        slots = m.Slots;
        itemType = "";   // MagicItem has no ItemType field
        note = m.Note;
        Source = GearItemSource.Magic;
    }

    // Constructor for creating a new item from user input (not from a saved Character model)
    public GearItemViewModel(string name, int slots, string itemType, string note)
    {
        this.name = name;
        this.slots = slots;
        this.itemType = itemType;
        this.note = note;
        Source = GearItemSource.Gear; // user-created items are mundane gear by default
    }
}
