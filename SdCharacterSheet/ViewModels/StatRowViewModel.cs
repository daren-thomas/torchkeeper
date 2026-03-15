using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SdCharacterSheet.Models;
using System.Collections.ObjectModel;

namespace SdCharacterSheet.ViewModels;

/// <summary>
/// Per-stat row ViewModel with expand/collapse and bonus source sub-list.
/// Holds the editable base stat and notifies CharacterViewModel of changes via callback delegate.
/// </summary>
public partial class StatRowViewModel : ObservableObject
{
    private readonly Action<int> _onBaseStatChanged;

    public string StatName { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalScore))]
    [NotifyPropertyChangedFor(nameof(ModifierDisplay))]
    private int baseStat;

    [ObservableProperty]
    private bool isExpanded;

    /// <summary>
    /// True while the inline base-stat Entry is visible (tap-to-edit mode).
    /// </summary>
    [ObservableProperty]
    private bool isEditingBase;

    [RelayCommand]
    private void BeginEditBase() => IsEditingBase = true;

    /// <summary>
    /// Called on Entry Completed or Unfocused — hides the Entry and restores the score Label.
    /// TotalScore recalculates automatically because it depends on BaseStat.
    /// Note: if the row Grid TapGestureRecognizer also fires when the score Label is tapped,
    /// both ToggleExpand and BeginEditBase will run; this is acceptable because expanding
    /// the row while editing is harmless.
    /// </summary>
    [RelayCommand]
    private void CommitBaseEdit() => IsEditingBase = false;

    public ObservableCollection<BonusSource> BonusSources { get; }

    public int TotalScore => BaseStat + BonusSources.Sum(b => ParseBonusValue(b.BonusTo));

    public string ModifierDisplay
    {
        get
        {
            var mod = (int)Math.Floor((TotalScore - 10.0) / 2.0);
            return mod >= 0 ? $"+{mod}" : $"{mod}";
        }
    }

    public StatRowViewModel(string statName, int baseValue, Action<int> onBaseStatChanged, IEnumerable<BonusSource> allBonuses)
    {
        StatName = statName;
        _onBaseStatChanged = onBaseStatChanged;

        var filtered = allBonuses
            .Where(b => b.BonusTo.StartsWith(statName + ":"))
            .ToList();
        BonusSources = new ObservableCollection<BonusSource>(filtered);

        // Set backing field directly to avoid triggering callback during construction
        baseStat = baseValue;
    }

    partial void OnBaseStatChanged(int value)
    {
        // Write back to CharacterViewModel so BaseSTR etc. stay in sync
        _onBaseStatChanged(value);
    }

    [RelayCommand]
    private void ToggleExpand() => IsExpanded = !IsExpanded;

    private static int ParseBonusValue(string bonusTo)
    {
        var parts = bonusTo.Split(':');
        if (parts.Length >= 2 && int.TryParse(parts[1], out var value))
            return value;
        return 0;
    }
}
