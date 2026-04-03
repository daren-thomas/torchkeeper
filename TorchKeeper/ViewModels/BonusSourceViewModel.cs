using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TorchKeeper.Models;

namespace TorchKeeper.ViewModels;

/// <summary>
/// Observable wrapper for a BonusSource, used in the stat drill-down panel.
/// Toggling IsActive writes back to the underlying model and notifies the parent stat row.
/// </summary>
public partial class BonusSourceViewModel : ObservableObject
{
    private readonly Action _onChanged;

    public BonusSource Source { get; }

    public string Label => Source.Label;

    /// <summary>Numeric value parsed from BonusTo (e.g. "STR:2" → 2, "DEX:-1" → -1).</summary>
    public int BonusValue { get; }

    public string BonusDisplay => BonusValue >= 0 ? $"+{BonusValue}" : $"{BonusValue}";

    [ObservableProperty]
    private bool isActive;

    public IRelayCommand RemoveCommand { get; }

    public BonusSourceViewModel(BonusSource source, Action onChanged, Action<BonusSourceViewModel> onRemove)
    {
        Source = source;
        isActive = source.IsActive;
        BonusValue = ParseBonusValue(source.BonusTo);
        _onChanged = onChanged;
        RemoveCommand = new RelayCommand(() => onRemove(this));
    }

    partial void OnIsActiveChanged(bool value)
    {
        Source.IsActive = value;
        _onChanged();
    }

    private static int ParseBonusValue(string bonusTo)
    {
        var parts = bonusTo.Split(':');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var v))
            return v;
        return 0;
    }
}
