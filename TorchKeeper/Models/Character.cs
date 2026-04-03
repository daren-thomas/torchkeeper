namespace TorchKeeper.Models;

public class Character
{
    // Identity
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

    // Base stats (rolledStats from import — NOT the computed final values)
    public int BaseSTR { get; set; }
    public int BaseDEX { get; set; }
    public int BaseCON { get; set; }
    public int BaseINT { get; set; }
    public int BaseWIS { get; set; }
    public int BaseCHA { get; set; }

    // HP
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }

    // Currency
    public int GP { get; set; }
    public int SP { get; set; }
    public int CP { get; set; }

    // Bonuses (stat bonuses and AC contributors share this list; differentiated by BonusTo prefix)
    public List<BonusSource> Bonuses { get; set; } = [];

    // Gear
    public List<GearItem> Gear { get; set; } = [];
    public List<MagicItem> MagicItems { get; set; } = [];

    // Attacks (free-form text entries, e.g. "DAGGER: +3 (N), 1d4 (FIN)")
    public List<string> Attacks { get; set; } = [];

    // Talents and Spells (free text)
    public string Talents { get; set; } = "";
    public string SpellsKnown { get; set; } = "";

    // Notes (freeform player notes)
    public string Notes { get; set; } = "";
}
