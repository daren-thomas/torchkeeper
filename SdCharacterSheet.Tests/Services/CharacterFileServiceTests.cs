using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using SdCharacterSheet.DTOs;
using SdCharacterSheet.Models;
using SdCharacterSheet.Services;
using Xunit;

namespace SdCharacterSheet.Tests.Services;

[Trait("Category", "Unit")]
public class CharacterFileServiceTests
{
    private sealed class NullFileSaver : IFileSaver
    {
        public Task SaveAsync(string fileName, Stream stream, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private readonly CharacterFileService _service = new(new NullFileSaver());

    // FILE-02 + FILE-03: Round-trip save/load produces identical DTO
    [Fact]
    public async Task RoundTrip_SaveLoad_NoDataLoss()
    {
        var character = new Character
        {
            Name = "Brim",
            Class = "Fighter",
            Ancestry = "Human",
            Level = 3,
            Title = "Champion",
            Alignment = "Lawful",
            Background = "Soldier",
            Deity = "Thoth",
            Languages = "Common,Elvish",
            XP = 450,
            BaseSTR = 14,
            BaseDEX = 12,
            BaseCON = 10,
            BaseINT = 8,
            BaseWIS = 9,
            BaseCHA = 11,
            MaxHP = 20,
            CurrentHP = 15,
            GP = 100,
            SP = 50,
            CP = 25,
            Bonuses = [new BonusSource { Label = "Ring", BonusTo = "DEX:+2", SourceType = "Item", GainedAtLevel = 1 }],
            Gear = [new GearItem { Name = "Sword", Slots = 1, ItemType = "weapon", Note = "sharp" }],
            MagicItems = [new MagicItem { Name = "Ring of Protection", Slots = 0, Note = "AC+1" }],
            Attacks = ["DAGGER: +3 (N), 1d4 (FIN)"],
            SpellsKnown = "Magic Missile",
            Notes = "Keep left flank"
        };

        var dto = _service.MapToDto(character);

        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, dto);
        stream.Position = 0;

        var loaded = await _service.LoadFromStreamAsync(stream);

        Assert.NotNull(loaded);
        Assert.Equal("Brim", loaded.Name);
        Assert.Equal("Fighter", loaded.Class);
        Assert.Equal("Human", loaded.Ancestry);
        Assert.Equal(3, loaded.Level);
        Assert.Equal("Champion", loaded.Title);
        Assert.Equal("Lawful", loaded.Alignment);
        Assert.Equal("Soldier", loaded.Background);
        Assert.Equal("Thoth", loaded.Deity);
        Assert.Equal("Common,Elvish", loaded.Languages);
        Assert.Equal(450, loaded.XP);
        Assert.Equal(14, loaded.BaseSTR);
        Assert.Equal(12, loaded.BaseDEX);
        Assert.Equal(10, loaded.BaseCON);
        Assert.Equal(8, loaded.BaseINT);
        Assert.Equal(9, loaded.BaseWIS);
        Assert.Equal(11, loaded.BaseCHA);
        Assert.Equal(20, loaded.MaxHP);
        Assert.Equal(15, loaded.CurrentHP);
        Assert.Equal(100, loaded.GP);
        Assert.Equal(50, loaded.SP);
        Assert.Equal(25, loaded.CP);
        Assert.Equal("Magic Missile", loaded.SpellsKnown);
        Assert.Equal("Keep left flank", loaded.Notes);

        Assert.Single(loaded.Bonuses);
        Assert.Equal("Ring", loaded.Bonuses[0].Label);
        Assert.Equal("DEX:+2", loaded.Bonuses[0].BonusTo);

        Assert.Single(loaded.Gear);
        Assert.Equal("Sword", loaded.Gear[0].Name);
        Assert.Equal(1, loaded.Gear[0].Slots);
        Assert.Equal("weapon", loaded.Gear[0].ItemType);

        Assert.Single(loaded.MagicItems);
        Assert.Equal("Ring of Protection", loaded.MagicItems[0].Name);

        Assert.Single(loaded.Attacks);
        Assert.Equal("DAGGER: +3 (N), 1d4 (FIN)", loaded.Attacks[0]);
    }

    // FILE-03: Saved .sdchar JSON contains Version field
    [Fact]
    public void Save_ContainsVersionField()
    {
        var character = new Character { Name = "Test" };
        var dto = _service.MapToDto(character);
        var json = JsonSerializer.Serialize(dto);

        Assert.Contains("Version", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1", json);
    }
}
