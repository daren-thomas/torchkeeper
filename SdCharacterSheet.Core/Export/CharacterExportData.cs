namespace SdCharacterSheet.Core.Export;

public record CharacterExportData
{
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

    // HP
    public int CurrentHP { get; init; }
    public int MaxHP { get; init; }

    // Stats — pre-computed totals and modifiers
    public required IReadOnlyList<StatExportData> Stats { get; init; }

    // AC
    public int ACTotal { get; init; }
    public required IReadOnlyList<BonusExportData> ACBonuses { get; init; }

    // Currency
    public int GP { get; init; }
    public int SP { get; init; }
    public int CP { get; init; }

    // Gear
    public required IReadOnlyList<GearExportItem> GearItems { get; init; }
    public required IReadOnlyList<GearExportItem> FreeCarryItems { get; init; }
    public int GearSlotTotal { get; init; }
    public int GearSlotsUsed { get; init; }
    public int CoinSlots { get; init; }

    // Attacks
    public required IReadOnlyList<string> Attacks { get; init; }

    // Talents and Spells
    public string Talents { get; init; } = "";
    public string SpellsKnown { get; init; } = "";

    // Notes
    public string Notes { get; init; } = "";
}

public record StatExportData(string Name, int Total, string ModifierDisplay, IReadOnlyList<BonusExportData> Bonuses);

public record BonusExportData(string Label, string Value);  // Value = "+2", "-1", etc.

public record GearExportItem(string Name, int Slots, bool IsFreeCarry = false);
