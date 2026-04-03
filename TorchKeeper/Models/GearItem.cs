namespace TorchKeeper.Models;

public class GearItem
{
    public string Name { get; set; } = "";
    public int Slots { get; set; } = 1;
    public string ItemType { get; set; } = "";    // free text, e.g. "weapon", "armor"
    public string Note { get; set; } = "";        // optional free-text
    public bool IsFreeCarry { get; set; }         // true = excluded from GearSlotsUsed (D-05)
}
