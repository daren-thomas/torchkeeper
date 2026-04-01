using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Storage;
using SdCharacterSheet.DTOs;
using SdCharacterSheet.Models;

namespace SdCharacterSheet.Services;

public class CharacterFileService
{
    private readonly IFileSaver _fileSaver;

    public CharacterFileService(IFileSaver fileSaver)
    {
        _fileSaver = fileSaver;
    }

    private static readonly JsonSerializerOptions SaveOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private static readonly JsonSerializerOptions LoadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    private static readonly FilePickerFileType SdCharFileType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS,     new[] { "com.sdcharactersheet.sdchar" } },
            { DevicePlatform.macOS,   new[] { "com.sdcharactersheet.sdchar" } },
            { DevicePlatform.Android, new[] { "application/octet-stream" } },
            { DevicePlatform.WinUI,   new[] { ".sdchar" } },
        });

    private static readonly PickOptions SdCharPickOptions = new()
    {
        PickerTitle = "Open character file",
        FileTypes = SdCharFileType,
    };

    public async Task<Character?> OpenAsync(CancellationToken ct = default)
    {
        var fileResult = await FilePicker.Default.PickAsync(SdCharPickOptions);
        if (fileResult is null) return null;   // user cancelled
        using var stream = await fileResult.OpenReadAsync();  // NOT FullPath — cross-platform safe
        var dto = await LoadFromStreamAsync(stream);
        return dto is null ? null : MapFromDto(dto);
    }

    public async Task SaveAsync(Character character, CancellationToken ct = default)
    {
        await using var stream = new MemoryStream();
        await SaveToStreamAsync(character, stream);
        stream.Position = 0;
        var suggestedName = string.IsNullOrWhiteSpace(character.Name)
            ? "character.sdchar"
            : $"{character.Name}.sdchar";
        await _fileSaver.SaveAsync(suggestedName, stream, ct);
    }

    public CharacterSaveData MapToDto(Character character) => new()
    {
        Version = 1,
        Name = character.Name,
        Class = character.Class,
        Ancestry = character.Ancestry,
        Level = character.Level,
        Title = character.Title,
        Alignment = character.Alignment,
        Background = character.Background,
        Deity = character.Deity,
        Languages = character.Languages,
        XP = character.XP,
        MaxXP = character.MaxXP,
        BaseSTR = character.BaseSTR,
        BaseDEX = character.BaseDEX,
        BaseCON = character.BaseCON,
        BaseINT = character.BaseINT,
        BaseWIS = character.BaseWIS,
        BaseCHA = character.BaseCHA,
        MaxHP = character.MaxHP,
        CurrentHP = character.CurrentHP,
        GP = character.GP,
        SP = character.SP,
        CP = character.CP,
        Bonuses = character.Bonuses
            .Select(b => new BonusSourceData
            {
                Label = b.Label,
                BonusTo = b.BonusTo,
                SourceType = b.SourceType,
                GainedAtLevel = b.GainedAtLevel,
                IsActive = b.IsActive,
            })
            .ToList(),
        Gear = character.Gear
            .Select(g => new GearItemData
            {
                Name = g.Name,
                Slots = g.Slots,
                ItemType = g.ItemType,
                Note = g.Note,
                IsFreeCarry = g.IsFreeCarry,
            })
            .ToList(),
        MagicItems = character.MagicItems
            .Select(m => new MagicItemData
            {
                Name = m.Name,
                Slots = m.Slots,
                Note = m.Note,
                IsFreeCarry = m.IsFreeCarry,
            })
            .ToList(),
        Attacks = character.Attacks.ToList(),
        Talents = character.Talents,
        SpellsKnown = character.SpellsKnown,
        Notes = character.Notes
    };

    public Character MapFromDto(CharacterSaveData dto) => new()
    {
        Name = dto.Name,
        Class = dto.Class,
        Ancestry = dto.Ancestry,
        Level = dto.Level,
        Title = dto.Title,
        Alignment = dto.Alignment,
        Background = dto.Background,
        Deity = dto.Deity,
        Languages = dto.Languages,
        XP = dto.XP,
        MaxXP = dto.MaxXP,
        BaseSTR = dto.BaseSTR,
        BaseDEX = dto.BaseDEX,
        BaseCON = dto.BaseCON,
        BaseINT = dto.BaseINT,
        BaseWIS = dto.BaseWIS,
        BaseCHA = dto.BaseCHA,
        MaxHP = dto.MaxHP,
        CurrentHP = dto.CurrentHP,
        GP = dto.GP,
        SP = dto.SP,
        CP = dto.CP,
        Bonuses = dto.Bonuses
            .Select(b => new BonusSource
            {
                Label = b.Label,
                BonusTo = b.BonusTo,
                SourceType = b.SourceType,
                GainedAtLevel = b.GainedAtLevel,
                IsActive = b.IsActive,
            })
            .ToList(),
        Gear = dto.Gear
            .Select(g => new GearItem
            {
                Name = g.Name,
                Slots = g.Slots,
                ItemType = g.ItemType,
                Note = g.Note,
                IsFreeCarry = g.IsFreeCarry,
            })
            .ToList(),
        MagicItems = dto.MagicItems
            .Select(m => new MagicItem
            {
                Name = m.Name,
                Slots = m.Slots,
                Note = m.Note,
                IsFreeCarry = m.IsFreeCarry,
            })
            .ToList(),
        Attacks = dto.Attacks.ToList(),
        Talents = dto.Talents,
        SpellsKnown = dto.SpellsKnown,
        Notes = dto.Notes
    };

    public async Task SaveToStreamAsync(Character character, Stream stream)
    {
        var dto = MapToDto(character);
        await JsonSerializer.SerializeAsync(stream, dto, SaveOptions);
    }

    public async Task<CharacterSaveData?> LoadFromStreamAsync(Stream stream)
    {
        if (stream.Length == 0)
            return null;

        try
        {
            return await JsonSerializer.DeserializeAsync<CharacterSaveData>(stream, LoadOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
