using System.IO;
using System.Text;

namespace SdCharacterSheet.Core.Export;

public static class MarkdownBuilder
{
    /// <summary>
    /// Builds a formatted Markdown string from the given export data.
    /// Section order: Identity → Stats → Attacks → Currency → Gear → [Spells] → Notes
    /// Per decisions D-06 through D-21 from phase 03 context.
    /// </summary>
    public static string BuildMarkdown(CharacterExportData data)
    {
        var sb = new StringBuilder();

        // 1. Identity section (D-09)
        sb.AppendLine($"# {data.Name}");
        sb.AppendLine();
        AppendIfNotEmpty(sb, "**Class:**", data.Class);
        AppendIfNotEmpty(sb, "**Ancestry:**", data.Ancestry);
        AppendIfNotEmpty(sb, "**Level:**", data.Level > 0 ? data.Level.ToString() : "");
        AppendIfNotEmpty(sb, "**Title:**", data.Title);
        AppendIfNotEmpty(sb, "**Alignment:**", data.Alignment);
        AppendIfNotEmpty(sb, "**Background:**", data.Background);
        AppendIfNotEmpty(sb, "**Deity:**", data.Deity);
        AppendIfNotEmpty(sb, "**Languages:**", data.Languages);
        // HP and XP (D-20) — always included, plain format
        sb.AppendLine($"HP: {data.CurrentHP} / {data.MaxHP}");
        sb.AppendLine($"XP: {data.XP} / {data.MaxXP}");
        sb.AppendLine();

        // 2. Stats section (D-06, D-08, D-10, D-11)
        sb.AppendLine("## Stats");
        sb.AppendLine();
        foreach (var stat in data.Stats)
        {
            sb.AppendLine($"**{stat.Name}** {stat.Total} ({stat.ModifierDisplay})");
            foreach (var bonus in stat.Bonuses)
            {
                sb.AppendLine($"  - {bonus.Label}: {bonus.Value}");
            }
        }

        // AC subsection (D-11)
        sb.AppendLine($"**AC** {data.ACTotal}");
        foreach (var bonus in data.ACBonuses)
        {
            sb.AppendLine($"  - {bonus.Label}: {bonus.Value}");
        }
        sb.AppendLine();

        // 3. Attacks section (D-12)
        sb.AppendLine("## Attacks");
        sb.AppendLine();
        foreach (var attack in data.Attacks)
        {
            sb.AppendLine($"- {attack}");
        }
        sb.AppendLine();

        // 4. Currency section (D-19)
        sb.AppendLine("## Currency");
        sb.AppendLine();
        sb.AppendLine($"GP: {data.GP}");
        sb.AppendLine($"SP: {data.SP}");
        sb.AppendLine($"CP: {data.CP}");
        sb.AppendLine();

        // 5. Gear section (D-13 through D-18)
        sb.AppendLine($"## Gear ({data.GearSlotsUsed} / {data.GearSlotTotal} slots)");
        sb.AppendLine();
        sb.AppendLine("| Slot | Item |");
        sb.AppendLine("|------|------|");

        // Build exactly GearSlotTotal rows
        var rows = new List<string>(data.GearSlotTotal);

        // Expand gear items (multi-slot items get continuation rows)
        foreach (var item in data.GearItems)
        {
            rows.Add(item.Name);
            for (int i = 1; i < item.Slots; i++)
            {
                rows.Add($"(cont. {item.Name})");
            }
        }

        // Add coin rows (D-17)
        for (int i = 0; i < data.CoinSlots; i++)
        {
            rows.Add("Coins");
        }

        // Fill remaining with empty strings (D-18)
        while (rows.Count < data.GearSlotTotal)
        {
            rows.Add("");
        }

        // Render table rows
        for (int i = 0; i < data.GearSlotTotal; i++)
        {
            var rowValue = i < rows.Count ? rows[i] : "";
            sb.AppendLine($"| {i + 1} | {rowValue} |");
        }
        sb.AppendLine();

        // 6. Talents section — only when non-empty
        if (!string.IsNullOrWhiteSpace(data.Talents))
        {
            sb.AppendLine("## Talents");
            sb.AppendLine();
            sb.AppendLine(data.Talents);
            sb.AppendLine();
        }

        // 7. Spells section (D-21) — only when non-empty
        if (!string.IsNullOrWhiteSpace(data.SpellsKnown))
        {
            sb.AppendLine("## Spells");
            sb.AppendLine();
            sb.AppendLine(data.SpellsKnown);
            sb.AppendLine();
        }

        // 8. Notes section
        sb.AppendLine("## Notes");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(data.Notes))
        {
            sb.AppendLine(data.Notes);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds a filesystem-safe filename for the exported Markdown file.
    /// Pattern: {Name}-{Class}{Level}.md, falling back to Character.md or {Name}.md.
    /// Per decision D-05.
    /// </summary>
    public static string BuildFileName(CharacterExportData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            return "Character.md";
        }

        string raw;
        if (string.IsNullOrWhiteSpace(data.Class))
        {
            raw = data.Name.Trim();
        }
        else
        {
            raw = $"{data.Name.Trim()}-{data.Class.Trim()}{data.Level}";
        }

        // Strip filesystem-unsafe characters
        var safe = string.Concat(raw.Split(Path.GetInvalidFileNameChars()));
        return safe + ".md";
    }

    private static void AppendIfNotEmpty(StringBuilder sb, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            sb.AppendLine($"{label} {value}");
        }
    }
}
