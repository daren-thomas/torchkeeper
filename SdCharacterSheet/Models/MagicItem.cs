namespace SdCharacterSheet.Models;

public class MagicItem
{
    public string Name { get; set; } = "";
    public int Slots { get; set; } = 1;
    public string Note { get; set; } = "";        // benefits, curses, personality — free text
}
