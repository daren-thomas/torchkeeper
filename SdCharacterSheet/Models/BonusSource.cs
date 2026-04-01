namespace SdCharacterSheet.Models;

public class BonusSource
{
    public string Label { get; set; } = "";       // e.g. "Ring of Protection" or "Leather Armor: 11+DEX"
    public string BonusTo { get; set; } = "";     // e.g. "DEX:+2" for stats, "AC:+1" for AC
    public string SourceType { get; set; } = "";
    public int GainedAtLevel { get; set; }
    public bool IsActive { get; set; } = true;    // false = item not worn/attuned; excluded from totals
}
