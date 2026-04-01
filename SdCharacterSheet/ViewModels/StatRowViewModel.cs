using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SdCharacterSheet.Models;
using System.Collections.ObjectModel;

namespace SdCharacterSheet.ViewModels;

/// <summary>
/// Per-stat row ViewModel with expand/collapse and bonus source sub-list.
/// Holds the editable base stat and notifies CharacterViewModel of changes via callback delegates.
/// </summary>
public partial class StatRowViewModel : ObservableObject
{
    private readonly Action<int> _onBaseStatChanged;
    private readonly Action _onBonusesChanged;
    private readonly List<BonusSource> _masterBonusList;  // shared reference to Character.Bonuses

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
    /// </summary>
    [RelayCommand]
    private void CommitBaseEdit() => IsEditingBase = false;

    public ObservableCollection<BonusSourceViewModel> BonusSources { get; }

    public int TotalScore => BaseStat + BonusSources.Where(b => b.IsActive).Sum(b => b.BonusValue);

    public string ModifierDisplay
    {
        get
        {
            var mod = (int)Math.Floor((TotalScore - 10.0) / 2.0);
            return mod >= 0 ? $"+{mod}" : $"{mod}";
        }
    }

    public StatRowViewModel(
        string statName,
        int baseValue,
        Action<int> onBaseStatChanged,
        List<BonusSource> masterBonusList,
        Action onBonusesChanged)
    {
        StatName = statName;
        _onBaseStatChanged = onBaseStatChanged;
        _onBonusesChanged = onBonusesChanged;
        _masterBonusList = masterBonusList;

        var viewModels = masterBonusList
            .Where(b => b.BonusTo.StartsWith(statName + ":"))
            .Select(b => new BonusSourceViewModel(b, OnBonusChanged, RemoveBonusSource))
            .ToList();
        BonusSources = new ObservableCollection<BonusSourceViewModel>(viewModels);

        baseStat = baseValue;
    }

    partial void OnBaseStatChanged(int value)
    {
        _onBaseStatChanged(value);
    }

    [RelayCommand]
    private void ToggleExpand() => IsExpanded = !IsExpanded;

    /// <summary>
    /// Adds a user-defined bonus source to both the master list and the local view.
    /// </summary>
    public void AddBonus(string label, int value, bool isActive)
    {
        var source = new BonusSource
        {
            Label = label,
            BonusTo = $"{StatName}:{value}",
            SourceType = "manual",
            IsActive = isActive,
        };
        _masterBonusList.Add(source);
        BonusSources.Add(new BonusSourceViewModel(source, OnBonusChanged, RemoveBonusSource));
        OnBonusChanged();
    }

    private void RemoveBonusSource(BonusSourceViewModel bvm)
    {
        _masterBonusList.Remove(bvm.Source);
        BonusSources.Remove(bvm);
        OnBonusChanged();
    }

    private void OnBonusChanged()
    {
        OnPropertyChanged(nameof(TotalScore));
        OnPropertyChanged(nameof(ModifierDisplay));
        _onBonusesChanged();
    }
}
