using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TorchKeeper.Models;
using TorchKeeper.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TorchKeeper.ViewModels;

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
    // Rule: first 100 coins (any mix of denominations) are free; every additional 100 costs 1 slot.
    // All denominations weigh the same — pool them before applying the threshold.
    public int CoinSlots
    {
        get
        {
            var total = GP + SP + CP;
            return total > 100 ? (total - 1) / 100 : 0;
        }
    }
    public bool HasCoinSlots => CoinSlots > 0;
    public int GearSlotsUsed => GearItems.Where(g => !g.IsFreeCarry).Sum(g => g.Slots) + CoinSlots;

    // ===== CURRENCY =====
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoinSlots))]
    [NotifyPropertyChangedFor(nameof(HasCoinSlots))]
    [NotifyPropertyChangedFor(nameof(GearSlotsUsed))]
    private int gP;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoinSlots))]
    [NotifyPropertyChangedFor(nameof(HasCoinSlots))]
    [NotifyPropertyChangedFor(nameof(GearSlotsUsed))]
    private int sP;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CoinSlots))]
    [NotifyPropertyChangedFor(nameof(HasCoinSlots))]
    [NotifyPropertyChangedFor(nameof(GearSlotsUsed))]
    private int cP;

    // ===== COLLECTIONS =====
    public ObservableCollection<GearItemViewModel> GearItems { get; } = [];
    public ObservableCollection<GearItemViewModel> RegularGearItems { get; } = [];
    public ObservableCollection<GearItemViewModel> FreeCarryItems { get; } = [];
    public ObservableCollection<string> Attacks { get; } = [];
    public ObservableCollection<StatRowViewModel> StatRows { get; } = [];

    // ===== NOTES =====
    [ObservableProperty] private string notes = "";

    // ===== TALENTS & SPELLS =====
    [ObservableProperty] private string talents = "";
    [ObservableProperty] private string spellsKnown = "";

    // ===== FILE SERVICES =====
    private readonly MarkdownExportService? _exportService;
    private readonly MauiCharacterFileService? _fileService;
    private readonly MauiImportFileService? _importFileService;

    // ===== FILE COMMANDS =====
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_fileService is null) return;
        var character = BuildCharacterFromViewModel();
        await _fileService.SaveAsync(character);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_fileService is null) return;
        var character = await _fileService.OpenAsync();
        if (character is null) return;
        LoadCharacter(character);
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (_importFileService is null) return;
        var character = await _importFileService.ImportAsync();
        if (character is null) return;
        LoadCharacter(character);
    }

    // ===== EXPORT =====
    [RelayCommand]
    private async Task ExportAsync()
    {
        if (_exportService is null) return;
        await _exportService.ExportAsync(this);
    }

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
            RebuildGearSubCollections();   // added
        };

        // Build StatRows from default (zeroed) character so the stats section is visible on startup
        RebuildStatRows(Character);
    }

    public CharacterViewModel(MarkdownExportService exportService) : this()
    {
        _exportService = exportService;
    }

    public CharacterViewModel(
        MarkdownExportService exportService,
        MauiCharacterFileService fileService,
        MauiImportFileService importFileService) : this(exportService)
    {
        _fileService = fileService;
        _importFileService = importFileService;
    }

    // ===== BUILD FROM VM STATE =====
    /// <summary>
    /// Builds a fresh Character from current VM state.
    /// Do NOT use the Character backing field — it is stale after edits.
    /// </summary>
    private Character BuildCharacterFromViewModel() => new()
    {
        Name = Name,
        Class = Class,
        Ancestry = Ancestry,
        Level = Level,
        Title = Title,
        Alignment = Alignment,
        Background = Background,
        Deity = Deity,
        Languages = Languages,
        XP = XP,
        MaxXP = MaxXP,
        BaseSTR = BaseSTR,
        BaseDEX = BaseDEX,
        BaseCON = BaseCON,
        BaseINT = BaseINT,
        BaseWIS = BaseWIS,
        BaseCHA = BaseCHA,
        MaxHP = MaxHP,
        CurrentHP = CurrentHP,
        GP = GP,
        SP = SP,
        CP = CP,
        Bonuses = Character.Bonuses,
        Gear = GearItems
            .Where(g => g.Source == GearItemSource.Gear)
            .Select(g => new GearItem { Name = g.Name, Slots = g.Slots, ItemType = g.ItemType, Note = g.Note, IsFreeCarry = g.IsFreeCarry })
            .ToList(),
        MagicItems = GearItems
            .Where(g => g.Source == GearItemSource.Magic)
            .Select(g => new MagicItem { Name = g.Name, Slots = g.Slots, Note = g.Note, IsFreeCarry = g.IsFreeCarry })
            .ToList(),
        Attacks = Attacks.ToList(),
        Talents = Talents,
        SpellsKnown = SpellsKnown,
        Notes = Notes,
    };

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

        // Talents, Spells, Notes
        talents = character.Talents;
        spellsKnown = character.SpellsKnown;
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

        // Sync sub-collections after full character load
        RebuildGearSubCollections();
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
            StatRows.Add(new StatRowViewModel(statName, baseVal, writeBack, character.Bonuses, NotifyStatTotals));
    }

    private void NotifyStatTotals()
    {
        OnPropertyChanged(nameof(TotalSTR));
        OnPropertyChanged(nameof(TotalDEX));
        OnPropertyChanged(nameof(TotalCON));
        OnPropertyChanged(nameof(TotalINT));
        OnPropertyChanged(nameof(TotalWIS));
        OnPropertyChanged(nameof(TotalCHA));
        OnPropertyChanged(nameof(ModSTR));
        OnPropertyChanged(nameof(ModDEX));
        OnPropertyChanged(nameof(ModCON));
        OnPropertyChanged(nameof(ModINT));
        OnPropertyChanged(nameof(ModWIS));
        OnPropertyChanged(nameof(ModCHA));
        OnPropertyChanged(nameof(GearSlotTotal));
        OnPropertyChanged(nameof(GearSlotsUsed));
    }

    private void OnGearItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GearItemViewModel.Slots) or nameof(GearItemViewModel.IsFreeCarry))
        {
            OnPropertyChanged(nameof(GearSlotsUsed));
            RebuildGearSubCollections();
        }
    }

    private void RebuildGearSubCollections()
    {
        RegularGearItems.Clear();
        FreeCarryItems.Clear();
        foreach (var item in GearItems)
        {
            if (item.IsFreeCarry)
                FreeCarryItems.Add(item);
            else
                RegularGearItems.Add(item);
        }
    }

    private int SumBonusesFor(string statPrefix) =>
        Character.Bonuses
            .Where(b => b.IsActive && b.BonusTo.StartsWith(statPrefix + ":"))
            .Sum(b =>
            {
                var valuePart = b.BonusTo.Split(':')[1];
                return int.TryParse(valuePart, out var v) ? v : 0;
            });

    private static int FloorMod(int score) =>
        (int)Math.Floor((score - 10.0) / 2.0);
}
