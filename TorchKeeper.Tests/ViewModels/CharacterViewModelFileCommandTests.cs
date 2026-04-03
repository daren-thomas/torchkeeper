using System.IO;
using System.Text;
using System.Text.Json;
using TorchKeeper.DTOs;
using TorchKeeper.Models;
using TorchKeeper.Services;
using Xunit;

namespace TorchKeeper.Tests.ViewModels;

/// <summary>
/// Unit tests for SaveCommand, LoadCommand, ImportCommand logic using test-local fakes.
/// The test project only references TorchKeeper.Core, so we cannot use the real
/// CharacterViewModel or MAUI services. TestFileCommandVM mirrors the command logic.
/// </summary>
[Trait("Category", "Unit")]
public class CharacterViewModelFileCommandTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Test-local fakes
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Captures save calls so tests can assert what was saved.
    /// </summary>
    private sealed class FakeFileSaver : IFileSaver
    {
        public string? LastFileName { get; private set; }
        public byte[]? LastContent { get; private set; }

        public async Task SaveAsync(string fileName, Stream stream, CancellationToken ct = default)
        {
            LastFileName = fileName;
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            LastContent = ms.ToArray();
        }
    }

    private enum GearItemSource { Gear, Magic }

    private class TestGearItem
    {
        public string Name { get; set; } = "";
        public int Slots { get; set; } = 1;
        public string ItemType { get; set; } = "";
        public string Note { get; set; } = "";
        public GearItemSource Source { get; set; } = GearItemSource.Gear;
    }

    /// <summary>
    /// Test-local stub that mirrors CharacterViewModel command logic and
    /// BuildCharacterFromViewModel behavior. Uses Core services directly.
    /// </summary>
    private class TestFileCommandVM
    {
        private readonly CharacterFileService? _fileService;
        private readonly ShadowdarklingsImportService? _importService;

        // Backing fields for Character properties
        public string Name { get; set; } = "";
        public string Class { get; set; } = "";
        public string Ancestry { get; set; } = "";
        public int Level { get; set; }
        public string Title { get; set; } = "";
        public string Alignment { get; set; } = "";
        public string Background { get; set; } = "";
        public string Deity { get; set; } = "";
        public string Languages { get; set; } = "";
        public int XP { get; set; }
        public int MaxXP { get; set; } = 10;

        public int BaseSTR { get; set; }
        public int BaseDEX { get; set; }
        public int BaseCON { get; set; }
        public int BaseINT { get; set; }
        public int BaseWIS { get; set; }
        public int BaseCHA { get; set; }

        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }

        public int GP { get; set; }
        public int SP { get; set; }
        public int CP { get; set; }

        public string SpellsKnown { get; set; } = "";
        public string Notes { get; set; } = "";

        public List<string> Attacks { get; set; } = [];
        public List<TestGearItem> GearItems { get; set; } = [];

        // Keep original Character for Bonuses passthrough (not editable in tests)
        private Character _character = new();

        public TestFileCommandVM(CharacterFileService? fileService = null,
                                  ShadowdarklingsImportService? importService = null)
        {
            _fileService = fileService;
            _importService = importService;
        }

        // Mirrors CharacterViewModel.BuildCharacterFromViewModel
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
            Bonuses = _character.Bonuses,
            Gear = GearItems
                .Where(g => g.Source == GearItemSource.Gear)
                .Select(g => new GearItem { Name = g.Name, Slots = g.Slots, ItemType = g.ItemType, Note = g.Note })
                .ToList(),
            MagicItems = GearItems
                .Where(g => g.Source == GearItemSource.Magic)
                .Select(g => new MagicItem { Name = g.Name, Slots = g.Slots, Note = g.Note })
                .ToList(),
            Attacks = Attacks.ToList(),
            SpellsKnown = SpellsKnown,
            Notes = Notes,
        };

        // Mirrors CharacterViewModel.LoadCharacter
        public void LoadCharacter(Character character)
        {
            _character = character;
            Name = character.Name;
            Class = character.Class;
            Ancestry = character.Ancestry;
            Level = character.Level;
            Title = character.Title;
            Alignment = character.Alignment;
            Background = character.Background;
            Deity = character.Deity;
            Languages = character.Languages;
            XP = character.XP;
            MaxXP = character.MaxXP;
            BaseSTR = character.BaseSTR;
            BaseDEX = character.BaseDEX;
            BaseCON = character.BaseCON;
            BaseINT = character.BaseINT;
            BaseWIS = character.BaseWIS;
            BaseCHA = character.BaseCHA;
            MaxHP = character.MaxHP;
            CurrentHP = character.CurrentHP;
            GP = character.GP;
            SP = character.SP;
            CP = character.CP;
            SpellsKnown = character.SpellsKnown;
            Notes = character.Notes;
            Attacks = character.Attacks.ToList();
            GearItems = [
                ..character.Gear.Select(g => new TestGearItem
                {
                    Name = g.Name, Slots = g.Slots, ItemType = g.ItemType, Note = g.Note,
                    Source = GearItemSource.Gear
                }),
                ..character.MagicItems.Select(m => new TestGearItem
                {
                    Name = m.Name, Slots = m.Slots, Note = m.Note,
                    Source = GearItemSource.Magic
                }),
            ];
        }

        // Mirrors CharacterViewModel.SaveAsync
        public async Task SaveAsync()
        {
            if (_fileService is null) return;
            var character = BuildCharacterFromViewModel();
            await _fileService.SaveAsync(character);
        }

        // Mirrors CharacterViewModel.LoadAsync (receives Character? from file service)
        public void LoadFromCharacter(Character? character)
        {
            if (character is null) return;
            LoadCharacter(character);
        }

        // Mirrors CharacterViewModel.ImportAsync (receives Character? from import service)
        public void ImportFromCharacter(Character? character)
        {
            if (character is null) return;
            LoadCharacter(character);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper: deserialize captured FakeFileSaver content back to CharacterSaveData
    // ─────────────────────────────────────────────────────────────────────────
    private static readonly JsonSerializerOptions LoadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
    };

    private static async Task<CharacterSaveData?> DeserializeCapture(byte[] content)
    {
        using var ms = new MemoryStream(content);
        return await JsonSerializer.DeserializeAsync<CharacterSaveData>(ms, LoadOptions);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tests
    // ─────────────────────────────────────────────────────────────────────────

    // FILE-01: SaveCommand builds Character from current VM state (not stale backing field)
    [Fact]
    public async Task SaveCommand_BuildsCharacterFromVMState_NotBackingField()
    {
        var saver = new FakeFileSaver();
        var service = new CharacterFileService(saver);
        var vm = new TestFileCommandVM(service);

        vm.Name = "Test";
        vm.Level = 5;
        vm.BaseSTR = 14;

        await vm.SaveAsync();

        Assert.NotNull(saver.LastContent);
        var saved = await DeserializeCapture(saver.LastContent!);
        Assert.NotNull(saved);
        Assert.Equal("Test", saved!.Name);
        Assert.Equal(5, saved.Level);
        Assert.Equal(14, saved.BaseSTR);
    }

    // FILE-01: SaveCommand suggests filename from character name
    [Fact]
    public async Task SaveCommand_SuggestsFileNameFromCharacterName()
    {
        var saver = new FakeFileSaver();
        var service = new CharacterFileService(saver);
        var vm = new TestFileCommandVM(service);

        vm.Name = "Brim";

        await vm.SaveAsync();

        Assert.Equal("Brim.sdchar", saver.LastFileName);
    }

    // FILE-01: SaveCommand splits gear by Source enum (not ItemType)
    [Fact]
    public async Task SaveCommand_GearSplitBySource_NotItemType()
    {
        var saver = new FakeFileSaver();
        var service = new CharacterFileService(saver);
        var vm = new TestFileCommandVM(service);

        vm.GearItems =
        [
            new TestGearItem { Name = "Sword", Slots = 1, ItemType = "weapon", Source = GearItemSource.Gear },
            new TestGearItem { Name = "Ring", Slots = 0, ItemType = "", Source = GearItemSource.Magic },
        ];

        await vm.SaveAsync();

        Assert.NotNull(saver.LastContent);
        var saved = await DeserializeCapture(saver.LastContent!);
        Assert.NotNull(saved);
        Assert.Single(saved!.Gear);
        Assert.Equal("Sword", saved.Gear[0].Name);
        Assert.Single(saved.MagicItems);
        Assert.Equal("Ring", saved.MagicItems[0].Name);
    }

    // FILE-02: LoadCommand replaces VM state with loaded Character
    [Fact]
    public void LoadCommand_ReplacesVMState()
    {
        var vm = new TestFileCommandVM();
        vm.Name = "Before";

        var loaded = new Character { Name = "After", Level = 7 };
        vm.LoadFromCharacter(loaded);

        Assert.Equal("After", vm.Name);
        Assert.Equal(7, vm.Level);
    }

    // FILE-03: ImportCommand replaces VM state with imported Character
    [Fact]
    public void ImportCommand_ReplacesVMState()
    {
        var vm = new TestFileCommandVM();
        vm.Name = "Before";

        var imported = new Character { Name = "Imported", Class = "Thief" };
        vm.ImportFromCharacter(imported);

        Assert.Equal("Imported", vm.Name);
        Assert.Equal("Thief", vm.Class);
    }

    // FILE-01 edge case: SaveCommand with null service does not throw
    [Fact]
    public async Task SaveCommand_NullService_NoOp()
    {
        var vm = new TestFileCommandVM(fileService: null);
        vm.Name = "Test";

        // Should not throw
        await vm.SaveAsync();
    }
}
