using CommunityToolkit.Mvvm.ComponentModel;
using SdCharacterSheet.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SdCharacterSheet.ViewModels;

/// <summary>
/// Singleton ViewModel shared across all tabs.
/// CharacterViewModel must NEVER be serialized directly — use CharacterSaveData DTO.
/// </summary>
public partial class CharacterViewModel : ObservableObject
{
    // Backing character — for file services
    public Character Character { get; private set; } = new();

    // ===== IDENTITY =====
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string @class = "";   // 'class' is a reserved word — use @class
    [ObservableProperty] private string ancestry = "";
    [ObservableProperty] private int level;
    [ObservableProperty] private string title = "";
    [ObservableProperty] private string alignment = "";
    [ObservableProperty] private string background = "";
    [ObservableProperty] private string deity = "";
    [ObservableProperty] private string languages = "";
    [ObservableProperty] private int xP;   // XP — source generator: xP → XP
    [ObservableProperty] private int maxXP = 10;

    // ===== HP =====
    [ObservableProperty] private int currentHP;
    [ObservableProperty] private int maxHP;

    // ===== BASE STATS =====
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalSTR))]
    [NotifyPropertyChangedFor(nameof(ModSTR))]
    [NotifyPropertyChangedFor(nameof(GearSlotTotal))]
    private int baseSTR;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalDEX))]
    [NotifyPropertyChangedFor(nameof(ModDEX))]
    private int baseDEX;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCON))]
    [NotifyPropertyChangedFor(nameof(ModCON))]
    private int baseCON;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalINT))]
    [NotifyPropertyChangedFor(nameof(ModINT))]
    private int baseINT;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalWIS))]
    [NotifyPropertyChangedFor(nameof(ModWIS))]
    private int baseWIS;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCHA))]
    [NotifyPropertyChangedFor(nameof(ModCHA))]
    private int baseCHA;

    // ===== COMPUTED STAT TOTALS =====
    public int TotalSTR => BaseSTR + SumBonusesFor("STR");
    public int TotalDEX => BaseDEX + SumBonusesFor("DEX");
    public int TotalCON => BaseCON + SumBonusesFor("CON");
    public int TotalINT => BaseINT + SumBonusesFor("INT");
    public int TotalWIS => BaseWIS + SumBonusesFor("WIS");
    public int TotalCHA => BaseCHA + SumBonusesFor("CHA");

    // ===== COMPUTED MODIFIERS (floor division) =====
    public int ModSTR => FloorMod(TotalSTR);
    public int ModDEX => FloorMod(TotalDEX);
    public int ModCON => FloorMod(TotalCON);
    public int ModINT => FloorMod(TotalINT);
    public int ModWIS => FloorMod(TotalWIS);
    public int ModCHA => FloorMod(TotalCHA);

    // ===== GEAR SLOTS =====
    public int GearSlotTotal => Math.Max(TotalSTR, 10);
    public int CoinSlots =>
        Math.Max(GP - 100, 0) / 100 +
        Math.Max(SP - 100, 0) / 100 +
        Math.Max(CP - 100, 0) / 100;
    public int GearSlotsUsed => GearItems.Sum(g => g.Slots) + CoinSlots;

    // ===== CURRENCY =====
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoinSlots))]
    [NotifyPropertyChangedFor(nameof(GearSlotsUsed))]
    private int gP;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoinSlots))]
    [NotifyPropertyChangedFor(nameof(GearSlotsUsed))]
    private int sP;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoinSlots))]
    [NotifyPropertyChangedFor(nameof(GearSlotsUsed))]
    private int cP;

    // ===== COLLECTIONS =====
    public ObservableCollection<GearItemViewModel> GearItems { get; } = [];
    public ObservableCollection<string> Attacks { get; } = [];
    public ObservableCollection<StatRowViewModel> StatRows { get; } = [];

    // ===== NOTES =====
    [ObservableProperty] private string notes = "";

    // ===== CONSTRUCTOR =====
    public CharacterViewModel()
    {
        GearItems.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
                foreach (GearItemViewModel g in e.NewItems)
                    g.PropertyChanged += OnGearItemChanged;
            if (e.OldItems != null)
                foreach (GearItemViewModel g in e.OldItems)
                    g.PropertyChanged -= OnGearItemChanged;
            OnPropertyChanged(nameof(GearSlotsUsed));
        };

        // Build StatRows from default (zeroed) character so the stats section is visible on startup
        RebuildStatRows(Character);
    }

    // ===== LOAD =====
    public void LoadCharacter(Character character)
    {
        Character = character;

        // Identity — set backing fields directly to bypass equality-check short-circuit
        name = character.Name;
        @class = character.Class;
        ancestry = character.Ancestry;
        level = character.Level;
        title = character.Title;
        alignment = character.Alignment;
        background = character.Background;
        deity = character.Deity;
        languages = character.Languages;
        xP = character.XP;
        maxXP = character.MaxXP;

        // HP
        currentHP = character.CurrentHP;
        maxHP = character.MaxHP;

        // Stats
        baseSTR = character.BaseSTR;
        baseDEX = character.BaseDEX;
        baseCON = character.BaseCON;
        baseINT = character.BaseINT;
        baseWIS = character.BaseWIS;
        baseCHA = character.BaseCHA;

        // Currency
        gP = character.GP;
        sP = character.SP;
        cP = character.CP;

        // Notes
        notes = character.Notes;

        // Rebuild GearItems
        GearItems.Clear();
        foreach (var g in character.Gear)
            GearItems.Add(new GearItemViewModel(g));
        foreach (var m in character.MagicItems)
            GearItems.Add(new GearItemViewModel(m));

        // Rebuild Attacks
        Attacks.Clear();
        foreach (var a in character.Attacks)
            Attacks.Add(a);

        // Rebuild StatRows
        RebuildStatRows(character);

        // Notify everything — full character replacement
        OnPropertyChanged(string.Empty);
    }

    // ===== HELPERS =====
    private void RebuildStatRows(Character character)
    {
        StatRows.Clear();
        var statDefs = new (string name, int baseVal, Action<int> writeBack)[]
        {
            ("STR", character.BaseSTR, v => { BaseSTR = v; }),
            ("DEX", character.BaseDEX, v => { BaseDEX = v; }),
            ("CON", character.BaseCON, v => { BaseCON = v; }),
            ("INT", character.BaseINT, v => { BaseINT = v; }),
            ("WIS", character.BaseWIS, v => { BaseWIS = v; }),
            ("CHA", character.BaseCHA, v => { BaseCHA = v; }),
        };
        foreach (var (statName, baseVal, writeBack) in statDefs)
            StatRows.Add(new StatRowViewModel(statName, baseVal, writeBack, character.Bonuses));
    }

    private void OnGearItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GearItemViewModel.Slots))
            OnPropertyChanged(nameof(GearSlotsUsed));
    }

    private int SumBonusesFor(string statPrefix) =>
        Character.Bonuses
            .Where(b => b.BonusTo.StartsWith(statPrefix + ":"))
            .Sum(b =>
            {
                var valuePart = b.BonusTo.Split(':')[1];
                return int.TryParse(valuePart, out var v) ? v : 0;
            });

    private static int FloorMod(int score) =>
        (int)Math.Floor((score - 10.0) / 2.0);
}
