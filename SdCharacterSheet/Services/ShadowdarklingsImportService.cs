using System.Text.Json;
using SdCharacterSheet.DTOs;
using SdCharacterSheet.Models;

namespace SdCharacterSheet.Services;

public class ShadowdarklingsImportService
{
    private static readonly JsonSerializerOptions ImportOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        // Unknown fields are silently ignored by System.Text.Json default — no extra option needed.
    };

    public async Task<Character?> ImportAsync(Stream jsonStream)
    {
        ShadowdarklingsJson? sdJson;
        try
        {
            sdJson = await JsonSerializer.DeserializeAsync<ShadowdarklingsJson>(jsonStream, ImportOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (sdJson is null)
            return null;

        // Currency: prefer top-level fields; fall back to ledger sum when top-level fields are absent (null).
        var gp = sdJson.Gold ?? (sdJson.Ledger?.Sum(e => e.GoldChange) ?? 0);
        var sp = sdJson.Silver ?? (sdJson.Ledger?.Sum(e => e.SilverChange) ?? 0);
        var cp = sdJson.Copper ?? (sdJson.Ledger?.Sum(e => e.CopperChange) ?? 0);

        // Map all bonuses — both stat bonuses (e.g. "DEX:+2") and AC contributors (e.g. "AC:+2")
        // go into the same Bonuses list. Differentiation by prefix happens at display time in Phase 2.
        var bonuses = sdJson.Bonuses?.Select(b => new BonusSource
        {
            Label = b.SourceName,
            BonusTo = b.BonusTo,
            SourceType = b.SourceType,
            GainedAtLevel = b.GainedAtLevel,
        }).ToList() ?? [];

        var gear = sdJson.Gear?.Select(g => new GearItem
        {
            Name = g.Name,
            Slots = g.Slots,
            ItemType = g.Type,
            Note = g.Note,
        }).ToList() ?? [];

        var magicItems = sdJson.MagicItems?.Select(m => new MagicItem
        {
            Name = m.Name,
            Slots = m.Slots,
            Note = m.Note,
        }).ToList() ?? [];

        return new Character
        {
            Name = sdJson.Name,
            Class = sdJson.Class,
            Ancestry = sdJson.Ancestry,
            Level = sdJson.Level,
            Title = sdJson.Title,
            Alignment = sdJson.Alignment,
            Background = sdJson.Background,
            Deity = sdJson.Deity,
            Languages = sdJson.Languages,
            XP = sdJson.XP,
            BaseSTR = sdJson.RolledStats?.STR ?? 0,
            BaseDEX = sdJson.RolledStats?.DEX ?? 0,
            BaseCON = sdJson.RolledStats?.CON ?? 0,
            BaseINT = sdJson.RolledStats?.INT ?? 0,
            BaseWIS = sdJson.RolledStats?.WIS ?? 0,
            BaseCHA = sdJson.RolledStats?.CHA ?? 0,
            MaxHP = sdJson.MaxHitPoints,
            CurrentHP = sdJson.MaxHitPoints,
            GP = gp,
            SP = sp,
            CP = cp,
            Bonuses = bonuses,
            Gear = gear,
            MagicItems = magicItems,
            Attacks = sdJson.Attacks ?? [],
            SpellsKnown = sdJson.SpellsKnown,
        };
    }
}
