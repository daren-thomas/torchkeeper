using TorchKeeper.Core.Export;
using Xunit;

namespace TorchKeeper.Tests.Export;

/// <summary>
/// Unit tests for MarkdownBuilder.BuildMarkdown and BuildFileName.
/// Tests verify all formatting decisions D-05 through D-21 from 03-CONTEXT.md.
/// </summary>
public class MarkdownBuilderTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Test helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static CharacterExportData MinimalData(
        string name = "Brim",
        string className = "Thief",
        int level = 4,
        IReadOnlyList<StatExportData>? stats = null,
        IReadOnlyList<BonusExportData>? acBonuses = null,
        IReadOnlyList<GearExportItem>? gear = null,
        IReadOnlyList<string>? attacks = null,
        string talents = "",
        string spellsKnown = "",
        string notes = "",
        int gearSlotTotal = 10,
        int gearSlotsUsed = 0,
        int coinSlots = 0,
        int currentHp = 8,
        int maxHp = 14,
        int xp = 3,
        int maxXp = 10,
        int gp = 0,
        int sp = 0,
        int cp = 0,
        int acTotal = 11,
        IReadOnlyList<GearExportItem>? freeCarryItems = null) =>
        new CharacterExportData
        {
            Name = name,
            Class = className,
            Level = level,
            CurrentHP = currentHp,
            MaxHP = maxHp,
            XP = xp,
            MaxXP = maxXp,
            GP = gp,
            SP = sp,
            CP = cp,
            ACTotal = acTotal,
            GearSlotTotal = gearSlotTotal,
            GearSlotsUsed = gearSlotsUsed,
            CoinSlots = coinSlots,
            Talents = talents,
            SpellsKnown = spellsKnown,
            Notes = notes,
            Stats = stats ?? DefaultStats(),
            ACBonuses = acBonuses ?? Array.Empty<BonusExportData>(),
            GearItems = gear ?? Array.Empty<GearExportItem>(),
            FreeCarryItems = freeCarryItems ?? Array.Empty<GearExportItem>(),
            Attacks = attacks ?? Array.Empty<string>(),
        };

    private static IReadOnlyList<StatExportData> DefaultStats() =>
    [
        new StatExportData("STR", 16, "+3", Array.Empty<BonusExportData>()),
        new StatExportData("DEX", 12, "+1", Array.Empty<BonusExportData>()),
        new StatExportData("CON", 14, "+2", Array.Empty<BonusExportData>()),
        new StatExportData("INT", 10, "+0", Array.Empty<BonusExportData>()),
        new StatExportData("WIS", 8, "-1", Array.Empty<BonusExportData>()),
        new StatExportData("CHA", 13, "+1", Array.Empty<BonusExportData>()),
    ];

    // ─────────────────────────────────────────────────────────────────────────
    // Test 1 (D-09, D-06): Section order
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_SectionOrder_IsIdentityStatsCurrencyGearNotes()
    {
        var data = MinimalData();
        var md = MarkdownBuilder.BuildMarkdown(data);

        var nameIdx = md.IndexOf("# Brim", StringComparison.Ordinal);
        var statsIdx = md.IndexOf("## Stats", StringComparison.Ordinal);
        var attacksIdx = md.IndexOf("## Attacks", StringComparison.Ordinal);
        var currencyIdx = md.IndexOf("## Currency", StringComparison.Ordinal);
        var gearIdx = md.IndexOf("## Gear", StringComparison.Ordinal);
        var notesIdx = md.IndexOf("## Notes", StringComparison.Ordinal);

        Assert.True(nameIdx >= 0, "Should contain # {Name} heading");
        Assert.True(nameIdx < statsIdx, "# Name should appear before ## Stats");
        Assert.True(statsIdx < attacksIdx, "## Stats should appear before ## Attacks");
        Assert.True(attacksIdx < currencyIdx, "## Attacks should appear before ## Currency");
        Assert.True(currencyIdx < gearIdx, "## Currency should appear before ## Gear");
        Assert.True(gearIdx < notesIdx, "## Gear should appear before ## Notes");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 2 (D-20): HP and XP appear in identity section
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_IdentitySection_ContainsHpAndXpLines()
    {
        var data = MinimalData(currentHp: 8, maxHp: 14, xp: 3, maxXp: 10);
        var md = MarkdownBuilder.BuildMarkdown(data);

        var hpLine = "HP: 8 / 14";
        var xpLine = "XP: 3 / 10";
        var statsIdx = md.IndexOf("## Stats", StringComparison.Ordinal);

        Assert.Contains(hpLine, md);
        Assert.Contains(xpLine, md);

        // Both must appear before ## Stats (in identity section)
        var hpIdx = md.IndexOf(hpLine, StringComparison.Ordinal);
        var xpIdx = md.IndexOf(xpLine, StringComparison.Ordinal);
        Assert.True(hpIdx < statsIdx, "HP line should appear before ## Stats");
        Assert.True(xpIdx < statsIdx, "XP line should appear before ## Stats");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 3 (D-10, D-08): Stat format is **NAME** Total (Mod)
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_StatsSection_FormatsStatLineCorrectly()
    {
        var stats = new List<StatExportData>
        {
            new("STR", 16, "+3", Array.Empty<BonusExportData>()),
        };
        var data = MinimalData(stats: stats);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("**STR** 16 (+3)", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 4 (D-10): Stat bonus sources appear as indented bullets
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_StatsSection_ShowsBonusSourceBulletsIndented()
    {
        var bonuses = new List<BonusExportData>
        {
            new("Ring of Protection", "+2"),
        };
        var stats = new List<StatExportData>
        {
            new("STR", 16, "+3", bonuses),
        };
        var data = MinimalData(stats: stats);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("  - Ring of Protection: +2", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 5 (D-11): AC subsection with bonus source bullets
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_StatsSection_ShowsAcSubsectionWithBonuses()
    {
        var acBonuses = new List<BonusExportData>
        {
            new("Shield", "+2"),
        };
        var data = MinimalData(acTotal: 13, acBonuses: acBonuses);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("**AC** 13", md);
        Assert.Contains("  - Shield: +2", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 6 (D-12): Attacks section has one bullet per attack
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_AttacksSection_ShowsBulletPerAttack()
    {
        var attacks = new List<string> { "DAGGER: +3 (N), 1d4 (FIN)", "SHORTBOW: +3, 1d6" };
        var data = MinimalData(attacks: attacks);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("- DAGGER: +3 (N), 1d4 (FIN)", md);
        Assert.Contains("- SHORTBOW: +3, 1d6", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 7 (D-19): Currency section has GP, SP, CP each on own line
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_CurrencySection_ShowsGpSpCpLines()
    {
        var data = MinimalData(gp: 50, sp: 10, cp: 5);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("GP: 50", md);
        Assert.Contains("SP: 10", md);
        Assert.Contains("CP: 5", md);

        // Ensure they appear after ## Currency
        var currencyIdx = md.IndexOf("## Currency", StringComparison.Ordinal);
        var gpIdx = md.IndexOf("GP: 50", StringComparison.Ordinal);
        var spIdx = md.IndexOf("SP: 10", StringComparison.Ordinal);
        var cpIdx = md.IndexOf("CP: 5", StringComparison.Ordinal);
        Assert.True(gpIdx > currencyIdx, "GP line should be in Currency section");
        Assert.True(spIdx > currencyIdx, "SP line should be in Currency section");
        Assert.True(cpIdx > currencyIdx, "CP line should be in Currency section");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 8 (D-13, D-14, D-15): Gear table header and structure
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_GearSection_HasCorrectHeaderAndTableStructure()
    {
        var data = MinimalData(gearSlotTotal: 10, gearSlotsUsed: 2);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("## Gear (2 / 10 slots)", md);
        Assert.Contains("| Slot | Item |", md);
        Assert.Contains("|------|------|", md);

        // Count data rows — should be exactly GearSlotTotal = 10
        var lines = md.Split('\n');
        var dataRows = lines.Count(l => l.StartsWith("| ") && l.Contains(" | ") && !l.Contains("Slot") && !l.Contains("---"));
        Assert.Equal(10, dataRows);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 9 (D-16): Multi-slot item expands with (cont. Name) rows
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_GearSection_ExpandsMultiSlotItemWithContinuationRows()
    {
        var gear = new List<GearExportItem>
        {
            new("Chain Mail", 2),
        };
        var data = MinimalData(gear: gear, gearSlotTotal: 10, gearSlotsUsed: 2);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("Chain Mail", md);
        Assert.Contains("(cont. Chain Mail)", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 10 (D-17): Coin slots appear as "Coins" rows
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_GearSection_ShowsCoinRowsWhenCoinSlotsGreaterThanZero()
    {
        var data = MinimalData(coinSlots: 2, gearSlotTotal: 10, gearSlotsUsed: 2);
        var md = MarkdownBuilder.BuildMarkdown(data);

        // Should have 2 Coins rows
        var lines = md.Split('\n');
        var coinRows = lines.Count(l => l.Contains("| Coins |") || l.Contains("| Coins|") || (l.StartsWith("| ") && l.TrimEnd().EndsWith("| Coins |")));
        Assert.True(coinRows >= 2, $"Expected at least 2 Coins rows, found {coinRows}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 11 (D-18): Unused slots are empty numbered rows
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_GearSection_ShowsEmptyRowsForUnusedSlots()
    {
        var gear = new List<GearExportItem>
        {
            new("Rope", 1),
        };
        // 10 slots total, 1 used — 9 empty
        var data = MinimalData(gear: gear, gearSlotTotal: 10, gearSlotsUsed: 1);
        var md = MarkdownBuilder.BuildMarkdown(data);

        // Row 2 through 10 should be empty: "| 2 |  |" pattern
        Assert.Contains("| 2 |  |", md);
        Assert.Contains("| 10 |  |", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 12 (D-21): Spells section appears when SpellsKnown non-empty
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_SpellsSection_AppearsWhenSpellsKnownNonEmpty()
    {
        var data = MinimalData(spellsKnown: "Magic Missile, Shield, Sleep");
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("## Spells", md);
        Assert.Contains("Magic Missile, Shield, Sleep", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 13 (D-21): Spells section is omitted when SpellsKnown is empty
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_SpellsSection_OmittedWhenSpellsKnownEmpty()
    {
        var data = MinimalData(spellsKnown: "");
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.DoesNotContain("## Spells", md);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 14 (D-07): No horizontal rules in output
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildMarkdown_Output_ContainsNoHorizontalRules()
    {
        var data = MinimalData();
        var md = MarkdownBuilder.BuildMarkdown(data);

        // Check for standalone --- lines (not table separator lines like |---|)
        var lines = md.Split('\n');
        var hrLines = lines.Where(l => l.Trim() == "---").ToList();
        Assert.Empty(hrLines);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 15 (D-05): Standard filename pattern
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildFileName_StandardCase_ReturnsNameDashClassLevelMd()
    {
        var data = MinimalData(name: "Brim", className: "Thief", level: 4);
        var fileName = MarkdownBuilder.BuildFileName(data);
        Assert.Equal("Brim-Thief4.md", fileName);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 16: Empty Name falls back to Character.md
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildFileName_EmptyName_ReturnsCharacterMd()
    {
        var data = MinimalData(name: "", className: "Thief", level: 4);
        var fileName = MarkdownBuilder.BuildFileName(data);
        Assert.Equal("Character.md", fileName);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 17: Filesystem-unsafe chars in name are stripped
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildFileName_UnsafeCharsInName_AreStripped()
    {
        var data = MinimalData(name: "A/B", className: "Fighter", level: 1);
        var fileName = MarkdownBuilder.BuildFileName(data);
        Assert.DoesNotContain("/", fileName);
        Assert.EndsWith(".md", fileName);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test 18: Empty Class omits dash, class, and level
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void BuildFileName_EmptyClass_ReturnsNameOnlyMd()
    {
        var data = MinimalData(name: "Brim", className: "", level: 4);
        var fileName = MarkdownBuilder.BuildFileName(data);
        Assert.Equal("Brim.md", fileName);
    }

    // -----------------------------------------------------------------
    // Test 19: Talents section appears when Talents non-empty
    // -----------------------------------------------------------------
    [Fact]
    public void BuildMarkdown_TalentsSection_AppearsWhenTalentsNonEmpty()
    {
        var data = MinimalData(talents: "Backstab +1, Tough as Nails");
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("## Talents", md);
        Assert.Contains("Backstab +1", md);
    }

    // -----------------------------------------------------------------
    // Test 20: Talents section omitted when Talents empty
    // -----------------------------------------------------------------
    [Fact]
    public void BuildMarkdown_TalentsSection_OmittedWhenTalentsEmpty()
    {
        var data = MinimalData(talents: "");
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.DoesNotContain("## Talents", md);
    }

    // GEAR-02: Free Carry section appears when FreeCarryItems is non-empty
    [Fact]
    [Trait("Category", "Unit")]
    public void BuildMarkdown_FreeCarrySection_AppearsWhenFreeCarryItemsPresent()
    {
        var data = MinimalData(freeCarryItems: [new GearExportItem("Backpack", 0, true)]);
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.Contains("### Free Carry", md);
        Assert.Contains("- Backpack", md);
    }

    // GEAR-02: Free Carry section is absent when FreeCarryItems is empty
    [Fact]
    [Trait("Category", "Unit")]
    public void BuildMarkdown_FreeCarrySection_OmittedWhenEmpty()
    {
        var data = MinimalData(freeCarryItems: Array.Empty<GearExportItem>());
        var md = MarkdownBuilder.BuildMarkdown(data);

        Assert.DoesNotContain("### Free Carry", md);
    }

    // GEAR-02 / D-09: GearSlotsUsed in export header matches the value from CharacterExportData
    // (which in turn comes from vm.GearSlotsUsed, now excluding free-carry items)
    [Fact]
    [Trait("Category", "Unit")]
    public void BuildMarkdown_GearSlotCount_MatchesExportData()
    {
        // Arrange: 2 regular slots used, 1 coin slot, total = 3; free-carry item not counted
        var gear = new[] { new GearExportItem("Sword", 1), new GearExportItem("Torch", 1) };
        var freeCarry = new[] { new GearExportItem("Backpack", 0, true) };
        var data = MinimalData(
            gear: gear,
            freeCarryItems: freeCarry,
            gearSlotsUsed: 3,   // 2 gear + 1 coin slot — Backpack excluded
            coinSlots: 1,
            gearSlotTotal: 10);

        var md = MarkdownBuilder.BuildMarkdown(data);

        // The gear header must show exactly the GearSlotsUsed value, not re-count items
        Assert.Contains("## Gear (3 / 10 slots)", md);
    }

    // GEAR-02: Regular gear table excludes free-carry items (they are only in the service mapping,
    // but this test confirms GearItems in the export data does not include free-carry items)
    [Fact]
    [Trait("Category", "Unit")]
    public void BuildMarkdown_RegularGear_ExcludesFreeCarryItems()
    {
        var regularGear = new[] { new GearExportItem("Sword", 1) };
        var freeCarry = new[] { new GearExportItem("Backpack", 0, true) };
        var data = MinimalData(
            gear: regularGear,
            freeCarryItems: freeCarry,
            gearSlotsUsed: 1,
            gearSlotTotal: 10);

        var md = MarkdownBuilder.BuildMarkdown(data);

        // Backpack appears only in Free Carry section, not in the main gear table rows
        var gearSectionIdx = md.IndexOf("## Gear", StringComparison.Ordinal);
        var freeCarrySectionIdx = md.IndexOf("### Free Carry", StringComparison.Ordinal);

        // "Backpack" must appear only after the Free Carry header
        var backpackIdx = md.IndexOf("Backpack", StringComparison.Ordinal);
        Assert.True(backpackIdx > freeCarrySectionIdx,
            "Backpack should appear in the Free Carry section, not in the regular gear table");
    }
}
