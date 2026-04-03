namespace TorchKeeper.DTOs;

// Import deserialization shape — mirrors Shadowdarklings JSON format.
// Source: examples/Brim.json (confirmed field names and data types).
// All unknown/unlisted fields are silently ignored by System.Text.Json default behavior.
// NEVER reuse this class for save/load — only for import.
public class ShadowdarklingsJson
{
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
    public int MaxHitPoints { get; set; }
    public string SpellsKnown { get; set; } = "";

    // Use RolledStats — NOT Stats. Stats already includes bonuses; importing Stats double-counts.
    public StatBlock? RolledStats { get; set; }

    // Currency: prefer top-level fields; fall back to ledger sum if null
    public int? Gold { get; set; }
    public int? Silver { get; set; }
    public int? Copper { get; set; }
    public List<LedgerEntry>? Ledger { get; set; }

    public List<SdBonus>? Bonuses { get; set; }
    public List<SdGearItem>? Gear { get; set; }
    public List<SdMagicItem>? MagicItems { get; set; }
    public List<string>? Attacks { get; set; }
    public List<SdLevelEntry>? Levels { get; set; }
}

public class StatBlock
{
    public int STR { get; set; }
    public int DEX { get; set; }
    public int CON { get; set; }
    public int INT { get; set; }
    public int WIS { get; set; }
    public int CHA { get; set; }
}

public class SdBonus
{
    public string BonusName { get; set; } = "";
    public string BonusTo { get; set; } = "";      // "DEX:+2" → stat bonus; "AC:..." → AC contributor
    public string SourceType { get; set; } = "";
    public string SourceCategory { get; set; } = "";
    public int GainedAtLevel { get; set; }
    public string SourceName { get; set; } = "";
}

public class SdGearItem
{
    public string Name { get; set; } = "";
    public int Slots { get; set; } = 1;
    public string Type { get; set; } = "";
    public string Note { get; set; } = "";
    public string GearId { get; set; } = "";
}

public class SdMagicItem
{
    public string Name { get; set; } = "";
    public int Slots { get; set; } = 1;
    public string Note { get; set; } = "";
}

public class LedgerEntry
{
    public int GoldChange { get; set; }
    public int SilverChange { get; set; }
    public int CopperChange { get; set; }
}

public class SdLevelEntry
{
    public int Level { get; set; }
    public string TalentRolledDesc { get; set; } = "";
    public string Rolled12ChosenTalentDesc { get; set; } = "";
    // Other fields (talentRolledName, Rolled12ChosenTalentName, HitPointRoll, etc.)
    // are silently ignored by System.Text.Json — no action needed.
}
