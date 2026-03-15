using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
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

    private void OnIncrementXP(object sender, EventArgs e) => _vm.XP++;
    private void OnDecrementXP(object sender, EventArgs e) => _vm.XP--;

    // Current HP tap-to-edit
    private void OnCurrentHpLabelTapped(object sender, TappedEventArgs e)
    {
        CurrentHpLabel.IsVisible = false;
        CurrentHpEntry.IsVisible = true;
        CurrentHpEntry.Focus();
    }

    private void OnCurrentHpEntryCompleted(object sender, EventArgs e)
    {
        CurrentHpEntry.IsVisible = false;
        CurrentHpLabel.IsVisible = true;
    }

    private void OnCurrentHpEntryUnfocused(object sender, FocusEventArgs e)
    {
        CurrentHpEntry.IsVisible = false;
        CurrentHpLabel.IsVisible = true;
    }

    // Max HP tap-to-edit
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

    // Current XP tap-to-edit
    private void OnCurrentXpLabelTapped(object sender, TappedEventArgs e)
    {
        CurrentXpLabel.IsVisible = false;
        CurrentXpEntry.IsVisible = true;
        CurrentXpEntry.Focus();
    }

    private void OnCurrentXpEntryCompleted(object sender, EventArgs e)
    {
        CurrentXpEntry.IsVisible = false;
        CurrentXpLabel.IsVisible = true;
    }

    private void OnCurrentXpEntryUnfocused(object sender, FocusEventArgs e)
    {
        CurrentXpEntry.IsVisible = false;
        CurrentXpLabel.IsVisible = true;
    }

    // Max XP tap-to-edit
    private void OnMaxXpLabelTapped(object sender, TappedEventArgs e)
    {
        MaxXpLabel.IsVisible = false;
        MaxXpEntry.IsVisible = true;
        MaxXpEntry.Focus();
    }

    private void OnMaxXpEntryCompleted(object sender, EventArgs e)
    {
        MaxXpEntry.IsVisible = false;
        MaxXpLabel.IsVisible = true;
    }

    private void OnMaxXpEntryUnfocused(object sender, FocusEventArgs e)
    {
        MaxXpEntry.IsVisible = false;
        MaxXpLabel.IsVisible = true;
    }

    private async void OnAttackTapped(object sender, TappedEventArgs e)
    {
        var label = (Label)sender;
        var attack = (string)label.BindingContext;
        var index = _vm.Attacks.IndexOf(attack);
        await this.ShowPopupAsync(new AttackPopup(_vm, attack, index), new PopupOptions());
    }

    private async void OnAddAttackClicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new AttackPopup(_vm), new PopupOptions());
    }
}
