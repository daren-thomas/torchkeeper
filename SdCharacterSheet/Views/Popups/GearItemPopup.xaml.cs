using CommunityToolkit.Maui.Views;
using SdCharacterSheet.ViewModels;

namespace SdCharacterSheet.Views.Popups;

public partial class GearItemPopup : Popup
{
    private readonly CharacterViewModel _vm;
    private readonly GearItemViewModel? _existingItem;  // null = new item

    // Edit existing item
    public GearItemPopup(CharacterViewModel vm, GearItemViewModel item)
    {
        InitializeComponent();
        _vm = vm;
        _existingItem = item;
        // Pre-fill fields
        NameEntry.Text = item.Name;
        SlotsEntry.Text = item.Slots.ToString();
        ItemTypeEntry.Text = item.ItemType;
        NoteEntry.Text = item.Note;
    }

    // Add new item
    public GearItemPopup(CharacterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        _existingItem = null;
        SlotsEntry.Text = "1"; // default 1 slot
    }

    private void OnSave(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(name)) { Close(); return; }
        int.TryParse(SlotsEntry.Text, out var slots);
        slots = Math.Max(1, slots); // minimum 1 slot

        if (_existingItem != null)
        {
            _existingItem.Name = name;
            _existingItem.Slots = slots;
            _existingItem.ItemType = ItemTypeEntry.Text?.Trim() ?? "";
            _existingItem.Note = NoteEntry.Text?.Trim() ?? "";
        }
        else
        {
            var newItem = new GearItemViewModel(name, slots,
                ItemTypeEntry.Text?.Trim() ?? "",
                NoteEntry.Text?.Trim() ?? "");
            _vm.GearItems.Add(newItem);
        }
        Close();
    }

    private void OnDelete(object sender, EventArgs e)
    {
        if (_existingItem != null)
            _vm.GearItems.Remove(_existingItem);
        Close();
    }
}
