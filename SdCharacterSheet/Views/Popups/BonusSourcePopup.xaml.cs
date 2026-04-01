using CommunityToolkit.Maui.Views;
using SdCharacterSheet.ViewModels;

namespace SdCharacterSheet.Views.Popups;

public partial class BonusSourcePopup : Popup
{
    private readonly StatRowViewModel _statRow;

    public BonusSourcePopup(StatRowViewModel statRow)
    {
        InitializeComponent();
        _statRow = statRow;
        TitleLabel.Text = $"Add {statRow.StatName} bonus";
    }

    private async void OnSave(object sender, EventArgs e)
    {
        var raw = ValueEntry.Text?.Trim() ?? "";
        if (!int.TryParse(raw, out var value)) { await CloseAsync(); return; }

        var label = LabelEntry.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(label)) { await CloseAsync(); return; }

        _statRow.AddBonus(label, value, ActiveCheckBox.IsChecked);
        await CloseAsync();
    }
}
