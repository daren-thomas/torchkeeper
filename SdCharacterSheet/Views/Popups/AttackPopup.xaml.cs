using CommunityToolkit.Maui.Views;
using SdCharacterSheet.ViewModels;

namespace SdCharacterSheet.Views.Popups;

public partial class AttackPopup : Popup
{
    private readonly CharacterViewModel _vm;
    private readonly string? _existingAttack;  // null = new attack
    private readonly int _editIndex;           // -1 = new attack

    // Constructor for editing an existing attack
    public AttackPopup(CharacterViewModel vm, string existingAttack, int index)
    {
        InitializeComponent();
        _vm = vm;
        _existingAttack = existingAttack;
        _editIndex = index;
        AttackEntry.Text = existingAttack;
    }

    // Constructor for adding a new attack
    public AttackPopup(CharacterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        _existingAttack = null;
        _editIndex = -1;
    }

    private void OnSave(object sender, EventArgs e)
    {
        var text = AttackEntry.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(text)) { Close(); return; }

        if (_editIndex >= 0 && _editIndex < _vm.Attacks.Count)
            _vm.Attacks[_editIndex] = text;
        else
            _vm.Attacks.Add(text);

        Close();
    }

    private void OnDelete(object sender, EventArgs e)
    {
        if (_editIndex >= 0 && _editIndex < _vm.Attacks.Count)
            _vm.Attacks.RemoveAt(_editIndex);
        Close();
    }
}
