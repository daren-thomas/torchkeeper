namespace SdCharacterSheet.DTOs;

public class CharacterSaveData
{
    public int Version { get; init; } = 1;

    // Identity
    public string Name { get; init; } = "";
    public string Class { get; init; } = "";
    public string Ancestry { get; init; } = "";
    public int Level { get; init; }
    public string Title { get; init; } = "";
    public string Alignment { get; init; } = "";
    public string Background { get; init; } = "";
    public string Deity { get; init; } = "";
    public string Languages { get; init; } = "";
    public int XP { get; init; }
    public int MaxXP { get; init; } = 10;

    // Stats
    public int BaseSTR { get; init; }
    public int BaseDEX { get; init; }
    public int BaseCON { get; init; }
    public int BaseINT { get; init; }
    public int BaseWIS { get; init; }
    public int BaseCHA { get; init; }

    // HP
    public int MaxHP { get; init; }
    public int CurrentHP { get; init; }

    // Currency
    public int GP { get; init; }
    public int SP { get; init; }
    public int CP { get; init; }

    // Bonuses and AC contributors
    public List<BonusSourceData> Bonuses { get; init; } = [];

    // Gear
    public List<GearItemData> Gear { get; init; } = [];
    public List<MagicItemData> MagicItems { get; init; } = [];

    // Attacks, talents, spells, notes
    public List<string> Attacks { get; init; } = [];
    public string Talents { get; init; } = "";
    public string SpellsKnown { get; init; } = "";
    public string Notes { get; init; } = "";
}

public class BonusSourceData
{
    public string Label { get; init; } = "";
    public string BonusTo { get; init; } = "";
    public string SourceType { get; init; } = "";
    public int GainedAtLevel { get; init; }
}

public class GearItemData
{
    public string Name { get; init; } = "";
    public int Slots { get; init; } = 1;
    public string ItemType { get; init; } = "";
    public string Note { get; init; } = "";
    public bool IsFreeCarry { get; init; }        // added for GEAR-01 persistence
}

public class MagicItemData
{
    public string Name { get; init; } = "";
    public int Slots { get; init; } = 1;
    public string Note { get; init; } = "";
    public bool IsFreeCarry { get; init; }        // added for GEAR-01 persistence
}
