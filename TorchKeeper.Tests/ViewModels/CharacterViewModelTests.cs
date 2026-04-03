using TorchKeeper.Models;
using Xunit;

namespace TorchKeeper.Tests.ViewModels;

/// <summary>
/// Wave 0 test scaffold: documents the computed property contracts for CharacterViewModel.
/// Tests run against a test-local stub (TestCharacterVM). In plan 02-01 Task 1, the stub
/// will be replaced by the real CharacterViewModel.
/// </summary>
public class CharacterViewModelTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Test-local stub mirroring the CharacterViewModel computed properties.
    // Must match what plan 02-01 implements.
    // ─────────────────────────────────────────────────────────────────────────
    private class TestCharacterVM
    {
        private Character _character = new();
        private List<GearItemVM> _gearItems = [];

        public string Name { get; private set; } = "";
        public string Class { get; private set; } = "";
        public string Ancestry { get; private set; } = "";
        public int Level { get; private set; }
        public string Title { get; private set; } = "";
        public string Alignment { get; private set; } = "";
        public string Background { get; private set; } = "";
        public string Deity { get; private set; } = "";
        public string Languages { get; private set; } = "";
        public int XP { get; private set; }

        public int BaseSTR { get; private set; }
        public int BaseDEX { get; private set; }
        public int BaseCON { get; private set; }
        public int BaseINT { get; private set; }
        public int BaseWIS { get; private set; }
        public int BaseCHA { get; private set; }

        public int MaxHP { get; private set; }
        public int CurrentHP { get; set; }

        public int GP { get; private set; }
        public int SP { get; private set; }
        public int CP { get; private set; }

        private List<BonusSource> Bonuses { get; set; } = [];

        // ── Computed: total stat with bonus filtering ──────────────────────
        public int TotalSTR => BaseSTR + BonusSum("STR");
        public int TotalDEX => BaseDEX + BonusSum("DEX");
        public int TotalCON => BaseCON + BonusSum("CON");
        public int TotalINT => BaseINT + BonusSum("INT");
        public int TotalWIS => BaseWIS + BonusSum("WIS");
        public int TotalCHA => BaseCHA + BonusSum("CHA");

        // ── Computed: stat modifier using floor division ───────────────────
        public int ModSTR => (int)Math.Floor((TotalSTR - 10.0) / 2.0);
        public int ModDEX => (int)Math.Floor((TotalDEX - 10.0) / 2.0);
        public int ModCON => (int)Math.Floor((TotalCON - 10.0) / 2.0);
        public int ModINT => (int)Math.Floor((TotalINT - 10.0) / 2.0);
        public int ModWIS => (int)Math.Floor((TotalWIS - 10.0) / 2.0);
        public int ModCHA => (int)Math.Floor((TotalCHA - 10.0) / 2.0);

        // ── Computed: gear slot capacity ───────────────────────────────────
        public int GearSlotTotal => Math.Max(TotalSTR, 10);

        // ── Computed: coin slots consumed ─────────────────────────────────
        // Rule: first 100 coins are free; every additional 100 (or part thereof) costs 1 slot.
        public int CoinSlots =>
            (GP  > 100 ? (GP  - 1) / 100 : 0) +
            (SP  > 100 ? (SP  - 1) / 100 : 0) +
            (CP  > 100 ? (CP  - 1) / 100 : 0);

        // ── Computed: total gear slots used (gear + coins) ─────────────────
        public int GearSlotsUsed => _gearItems.Where(g => !g.IsFreeCarry).Sum(g => g.Slots) + CoinSlots;

        // ── Load character ─────────────────────────────────────────────────
        public void LoadCharacter(Character character)
        {
            _character = character;
            Name = character.Name;
            Class = character.Class;
            Ancestry = character.Ancestry;
            Level = character.Level;
            Title = character.Title;
            Alignment = character.Alignment;
            Background = character.Background;
            Deity = character.Deity;
            Languages = character.Languages;
            XP = character.XP;

            BaseSTR = character.BaseSTR;
            BaseDEX = character.BaseDEX;
            BaseCON = character.BaseCON;
            BaseINT = character.BaseINT;
            BaseWIS = character.BaseWIS;
            BaseCHA = character.BaseCHA;

            MaxHP = character.MaxHP;
            CurrentHP = character.CurrentHP;

            GP = character.GP;
            SP = character.SP;
            CP = character.CP;

            Bonuses = character.Bonuses;

            _gearItems = [
                ..character.Gear.Select(g => new GearItemVM(g)),
                ..character.MagicItems.Select(m => new GearItemVM(m)),
            ];
        }

        // ── Helper ─────────────────────────────────────────────────────────
        private int BonusSum(string statPrefix)
        {
            return Bonuses
                .Where(b => b.BonusTo.StartsWith(statPrefix + ":"))
                .Sum(b => int.Parse(b.BonusTo.Split(':')[1]));
        }

        // Minimal gear item wrapper used inside the VM
        private class GearItemVM
        {
            public string Name { get; }
            public int Slots { get; }
            public string ItemType { get; }
            public string Note { get; }
            public bool IsFreeCarry { get; }

            public GearItemVM(GearItem g)
            {
                Name = g.Name;
                Slots = g.Slots;
                ItemType = g.ItemType;
                Note = g.Note;
                IsFreeCarry = g.IsFreeCarry;
            }

            public GearItemVM(MagicItem m)
            {
                Name = m.Name;
                Slots = m.Slots;
                ItemType = "";
                Note = m.Note;
                IsFreeCarry = m.IsFreeCarry;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Stat modifier tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Unit")]
    public void StatModifier_BaseStatOf10_Returns0()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { BaseSTR = 10 });
        Assert.Equal(0, vm.ModSTR);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void StatModifier_BaseStatOf9_ReturnsNegative1()
    {
        // Floor division: (9 - 10) / 2 = -0.5 → floor = -1
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { BaseSTR = 9 });
        Assert.Equal(-1, vm.ModSTR);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void StatModifier_BaseStatOf8_ReturnsNegative1()
    {
        // (8 - 10) / 2 = -1.0 → floor = -1
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { BaseSTR = 8 });
        Assert.Equal(-1, vm.ModSTR);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void StatModifier_BaseStatOf14_Returns2()
    {
        // (14 - 10) / 2 = 2.0 → 2
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { BaseSTR = 14 });
        Assert.Equal(2, vm.ModSTR);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Bonus filtering tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Unit")]
    public void BonusFilter_STRBonusIncluded()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character
        {
            BaseSTR = 10,
            Bonuses = [new BonusSource { BonusTo = "STR:+2" }]
        });
        Assert.Equal(12, vm.TotalSTR);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BonusFilter_ACBonusExcluded()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character
        {
            BaseSTR = 10,
            Bonuses = [new BonusSource { BonusTo = "AC:+1" }]
        });
        // AC bonus must not affect TotalSTR
        Assert.Equal(10, vm.TotalSTR);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Gear slot total tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Unit")]
    public void GearSlotTotal_STRAbove10_UsesSTR()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { BaseSTR = 14 });
        Assert.Equal(14, vm.GearSlotTotal);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GearSlotTotal_STRBelow10_UsesFloor10()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { BaseSTR = 6 });
        Assert.Equal(10, vm.GearSlotTotal);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Coin slot tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Unit")]
    public void CoinSlots_AllBelowFreeThreshold_Returns0()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { GP = 50, SP = 30, CP = 99 });
        Assert.Equal(0, vm.CoinSlots);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CoinSlots_Exactly100EachFree_Returns0()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { GP = 100, SP = 100, CP = 100 });
        Assert.Equal(0, vm.CoinSlots);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CoinSlots_200GP_Returns1()
    {
        // 200 coins: first 100 are free, 100 remaining → ceiling(100/100) = 1 slot
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { GP = 200, SP = 0, CP = 0 });
        Assert.Equal(1, vm.CoinSlots);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CoinSlots_101GP_Returns1()
    {
        // 101 coins: first 100 are free, 1 remaining → ceiling(1/100) = 1 slot
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { GP = 101, SP = 0, CP = 0 });
        Assert.Equal(1, vm.CoinSlots);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CoinSlots_201GP_Returns2()
    {
        // 201 coins: first 100 are free, 101 remaining → ceiling(101/100) = 2 slots
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { GP = 201, SP = 0, CP = 0 });
        Assert.Equal(2, vm.CoinSlots);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CoinSlots_Mixed_Correct()
    {
        // GP=200: ceil(100/100)=1, SP=300: ceil(200/100)=2, CP=0 → total 3
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { GP = 200, SP = 300, CP = 0 });
        Assert.Equal(3, vm.CoinSlots);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Negative HP test
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Unit")]
    public void NegativeHP_AllowedBelowZero()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character { MaxHP = 10, CurrentHP = 5 });
        vm.CurrentHP = -5;
        Assert.Equal(-5, vm.CurrentHP);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // LoadCharacter population test
    // ─────────────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────────────
    // Free-carry gear slot exclusion tests (GEAR-01)
    // ─────────────────────────────────────────────────────────────────────────

    // GEAR-01: Free-carry item is excluded from GearSlotsUsed
    [Fact]
    [Trait("Category", "Unit")]
    public void GearSlotsUsed_FreeCarryItemExcluded()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character
        {
            Gear = [new GearItem { Name = "Backpack", Slots = 1, IsFreeCarry = true }]
        });
        Assert.Equal(0, vm.GearSlotsUsed);
    }

    // GEAR-01: Mixed items — only regular gear counted
    [Fact]
    [Trait("Category", "Unit")]
    public void GearSlotsUsed_MixedItems_OnlyRegularCounted()
    {
        var vm = new TestCharacterVM();
        vm.LoadCharacter(new Character
        {
            Gear =
            [
                new GearItem { Name = "Sword", Slots = 1, IsFreeCarry = false },
                new GearItem { Name = "Backpack", Slots = 1, IsFreeCarry = true },
            ]
        });
        // Only Sword counts
        Assert.Equal(1, vm.GearSlotsUsed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void LoadCharacter_PopulatesAllIdentityFields()
    {
        var vm = new TestCharacterVM();
        var character = new Character
        {
            Name = "Brim",
            Class = "Fighter",
            Ancestry = "Human",
            Level = 3,
            Title = "Warrior",
            Alignment = "Lawful",
            Background = "Soldier",
            Deity = "Madeya",
            Languages = "Common",
            XP = 450,
            BaseSTR = 14,
            BaseDEX = 12,
            BaseCON = 13,
            BaseINT = 10,
            BaseWIS = 9,
            BaseCHA = 8,
            MaxHP = 18,
            CurrentHP = 14,
            GP = 25,
            SP = 10,
            CP = 5,
        };

        vm.LoadCharacter(character);

        Assert.Equal("Brim", vm.Name);
        Assert.Equal("Fighter", vm.Class);
        Assert.Equal("Human", vm.Ancestry);
        Assert.Equal(3, vm.Level);
        Assert.Equal("Warrior", vm.Title);
        Assert.Equal("Lawful", vm.Alignment);
        Assert.Equal("Soldier", vm.Background);
        Assert.Equal("Madeya", vm.Deity);
        Assert.Equal("Common", vm.Languages);
        Assert.Equal(450, vm.XP);
        Assert.Equal(14, vm.BaseSTR);
        Assert.Equal(12, vm.BaseDEX);
        Assert.Equal(13, vm.BaseCON);
        Assert.Equal(10, vm.BaseINT);
        Assert.Equal(9, vm.BaseWIS);
        Assert.Equal(8, vm.BaseCHA);
        Assert.Equal(18, vm.MaxHP);
        Assert.Equal(14, vm.CurrentHP);
        Assert.Equal(25, vm.GP);
        Assert.Equal(10, vm.SP);
        Assert.Equal(5, vm.CP);
    }
}
