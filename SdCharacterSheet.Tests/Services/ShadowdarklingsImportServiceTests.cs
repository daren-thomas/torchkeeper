using System.Text;
using SdCharacterSheet.Services;
using Xunit;

namespace SdCharacterSheet.Tests.Services;

[Trait("Category", "Unit")]
public class ShadowdarklingsImportServiceTests
{
    private readonly ShadowdarklingsImportService _sut = new();

    // FILE-01: Full import produces correct Character
    [Fact]
    public async Task Import_BrimJson_ProducesCorrectCharacter()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "Brim.json");
        await using var stream = File.OpenRead(path);

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.False(string.IsNullOrEmpty(character.Name));
        Assert.False(string.IsNullOrEmpty(character.Class));
        Assert.True(character.BaseSTR > 0 || character.BaseDEX > 0 || character.BaseCON > 0
                    || character.BaseINT > 0 || character.BaseWIS > 0 || character.BaseCHA > 0,
                    "At least one base stat should be non-zero");
        Assert.True(character.MaxHP > 0);
        Assert.Equal(character.MaxHP, character.CurrentHP);
    }

    // FILE-01: Uses rolledStats, not stats
    [Fact]
    public async Task Import_UsesRolledStats()
    {
        var json = """
            {
              "name": "Test",
              "class": "Fighter",
              "stats": { "STR": 18, "DEX": 10, "CON": 10, "INT": 10, "WIS": 10, "CHA": 10 },
              "rolledStats": { "STR": 14, "DEX": 10, "CON": 10, "INT": 10, "WIS": 10, "CHA": 10 }
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.Equal(14, character.BaseSTR);
    }

    // FILE-01: Currency reads top-level gold/silver/copper
    [Fact]
    public async Task Import_Currency_TopLevelFields()
    {
        var json = """
            {
              "name": "Test",
              "gold": 10,
              "silver": 5,
              "copper": 3,
              "ledger": [{ "goldChange": 999, "silverChange": 0, "copperChange": 0 }]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.Equal(10, character.GP);
        Assert.Equal(5, character.SP);
        Assert.Equal(3, character.CP);
    }

    // FILE-01: Currency falls back to ledger sum
    [Fact]
    public async Task Import_Currency_LedgerFallback()
    {
        var json = """
            {
              "name": "Test",
              "ledger": [
                { "goldChange": 10, "silverChange": 2, "copperChange": 0 },
                { "goldChange": 5, "silverChange": 1, "copperChange": 0 }
              ]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.Equal(15, character.GP);
        Assert.Equal(3, character.SP);
        Assert.Equal(0, character.CP);
    }

    // FILE-01: bonusTo "AC:..." entries go to Bonuses list (alongside stat bonuses)
    [Fact]
    public async Task Import_AcBonuses()
    {
        var json = """
            {
              "name": "Test",
              "bonuses": [
                { "bonusTo": "AC:+2", "sourceName": "Leather Armor", "sourceType": "Gear", "gainedAtLevel": 1 },
                { "bonusTo": "DEX:+1", "sourceName": "Ring", "sourceType": "Magic", "gainedAtLevel": 2 }
              ]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.Equal(2, character.Bonuses.Count);
        Assert.Single(character.Bonuses, b => b.BonusTo.StartsWith("AC:"));
        Assert.Single(character.Bonuses, b => b.BonusTo.StartsWith("DEX:"));
    }

    // FILE-01: Unknown JSON fields are silently ignored
    [Fact]
    public async Task Import_UnknownFields_Ignored()
    {
        var json = """
            {
              "name": "Test",
              "futureField": "value",
              "unknownArray": [1, 2]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var exception = await Record.ExceptionAsync(() => _sut.ImportAsync(stream));
        Assert.Null(exception);

        await using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var character = await _sut.ImportAsync(stream2);
        Assert.NotNull(character);
    }

    // FILE-01: currentHP = maxHP on import
    [Fact]
    public async Task Import_CurrentHP_EqualsMaxHP()
    {
        var json = """
            {
              "name": "Test",
              "maxHitPoints": 12
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.Equal(12, character.MaxHP);
        Assert.Equal(12, character.CurrentHP);
    }

    // STAT-01 / D-13: Levels array populates Talents field with "Lv{N}: {desc}" format
    [Fact]
    public async Task Import_Levels_PopulatesTalentsField()
    {
        var json = """
            {
              "name": "Brim",
              "levels": [
                { "level": 1, "talentRolledDesc": "Your Backstab deals +1 dice of damage", "Rolled12ChosenTalentDesc": "" },
                { "level": 2, "talentRolledDesc": "", "Rolled12ChosenTalentDesc": "" }
              ]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.Contains("Lv1: Your Backstab deals +1 dice of damage", character.Talents);
    }

    // D-13: Empty talentRolledDesc entries are skipped
    [Fact]
    public async Task Import_Levels_SkipsEmptyTalentRolledDesc()
    {
        var json = """
            {
              "name": "Test",
              "levels": [
                { "level": 1, "talentRolledDesc": "Something", "Rolled12ChosenTalentDesc": "" },
                { "level": 2, "talentRolledDesc": "",            "Rolled12ChosenTalentDesc": "" }
              ]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        // "Lv2" should not appear anywhere in Talents (empty desc skipped)
        Assert.DoesNotContain("Lv2:", character.Talents);
    }

    // Claude's discretion: Rolled12ChosenTalentDesc included when non-empty
    [Fact]
    public async Task Import_Levels_IncludesRolled12ChosenTalentDesc_WhenNonEmpty()
    {
        var json = """
            {
              "name": "Test",
              "levels": [
                {
                  "level": 12,
                  "talentRolledDesc": "",
                  "Rolled12ChosenTalentDesc": "Gain an ability score increase"
                }
              ]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.Contains("Lv12 (chosen): Gain an ability score increase", character.Talents);
    }

    // D-free-carry: Gear item with slots=0 maps to IsFreeCarry=true (Backpack convention)
    [Fact]
    public async Task Import_GearItemWithSlots0_SetsFreeCarry()
    {
        var json = """
            {
              "name": "Test",
              "gear": [
                { "name": "Backpack", "slots": 0, "type": "sundry", "gearId": "s2" },
                { "name": "Sword", "slots": 1, "type": "weapon", "gearId": "w1" }
              ]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        var backpack = character.Gear.Single(g => g.Name == "Backpack");
        Assert.True(backpack.IsFreeCarry);
        var sword = character.Gear.Single(g => g.Name == "Sword");
        Assert.False(sword.IsFreeCarry);
    }

    // D-coins-skip: Gear item with gearId="coins" is excluded (coin weight handled via GP/SP/CP)
    [Fact]
    public async Task Import_CoinsGearItem_IsSkipped()
    {
        var json = """
            {
              "name": "Test",
              "gold": 120,
              "gear": [
                { "name": "Coins", "slots": 1, "type": "sundry", "gearId": "coins" },
                { "name": "Dagger", "slots": 1, "type": "weapon", "gearId": "w4" }
              ]
            }
            """;
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var character = await _sut.ImportAsync(stream);

        Assert.NotNull(character);
        Assert.DoesNotContain(character.Gear, g => g.Name == "Coins");
        Assert.Single(character.Gear, g => g.Name == "Dagger");
        // Gold is still imported via the currency fields
        Assert.Equal(120, character.GP);
    }
}
