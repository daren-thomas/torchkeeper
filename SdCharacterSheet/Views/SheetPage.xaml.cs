using CommunityToolkit.Maui.Views;
using SdCharacterSheet.ViewModels;
using SdCharacterSheet.Views.Popups;

namespace SdCharacterSheet.Views;

public partial class SheetPage : ContentPage
{
    private readonly CharacterViewModel _vm;

    public SheetPage(CharacterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private void OnIncrementHP(object sender, EventArgs e) => _vm.CurrentHP++;
    private void OnDecrementHP(object sender, EventArgs e) => _vm.CurrentHP--;

    // MaxHP tap-to-edit toggle
    private void OnMaxHpLabelTapped(object sender, TappedEventArgs e)
    {
        MaxHpLabel.IsVisible = false;
        MaxHpEntry.IsVisible = true;
        MaxHpEntry.Focus();
    }

    private void OnMaxHpEntryCompleted(object sender, EventArgs e)
    {
        MaxHpEntry.IsVisible = false;
        MaxHpLabel.IsVisible = true;
    }

    private void OnMaxHpEntryUnfocused(object sender, FocusEventArgs e)
    {
        MaxHpEntry.IsVisible = false;
        MaxHpLabel.IsVisible = true;
    }

    private async void OnAttackTapped(object sender, TappedEventArgs e)
    {
        var label = (Label)sender;
        var attack = (string)label.BindingContext;
        var index = _vm.Attacks.IndexOf(attack);
        await this.ShowPopupAsync(new AttackPopup(_vm, attack, index));
    }

    private async void OnAddAttackClicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new AttackPopup(_vm));
    }
}
