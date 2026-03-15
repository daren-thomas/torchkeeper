using SdCharacterSheet.Models;
using Xunit;

namespace SdCharacterSheet.Tests.ViewModels;

/// <summary>
/// Wave 0 test scaffold: documents the GearItemViewModel contract.
/// Tests run against a test-local stub (TestGearItemVM). In plan 02-01 Task 1, the stub
/// will be replaced by the real GearItemViewModel.
/// </summary>
public class GearItemViewModelTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Test-local stub mirroring the GearItemViewModel.
    // Wraps either a GearItem or MagicItem into a unified view model.
    // MagicItems have no ItemType, so it defaults to "".
    // ─────────────────────────────────────────────────────────────────────────
    private class TestGearItemVM
    {
        public string Name { get; }
        public int Slots { get; }
        public string ItemType { get; }
        public string Note { get; }

        public TestGearItemVM(GearItem g)
        {
            Name = g.Name;
            Slots = g.Slots;
            ItemType = g.ItemType;
            Note = g.Note;
        }

        public TestGearItemVM(MagicItem m)
        {
            Name = m.Name;
            Slots = m.Slots;
            ItemType = "";   // MagicItem has no ItemType field
            Note = m.Note;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Unit")]
    public void GearItemViewModel_FromGearItem_MapsAllFields()
    {
        var gear = new GearItem
        {
            Name = "Sword",
            Slots = 1,
            ItemType = "weapon",
            Note = ""
        };

        var vm = new TestGearItemVM(gear);

        Assert.Equal("Sword", vm.Name);
        Assert.Equal(1, vm.Slots);
        Assert.Equal("weapon", vm.ItemType);
        Assert.Equal("", vm.Note);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GearItemViewModel_FromMagicItem_MapsAllFields()
    {
        var magic = new MagicItem
        {
            Name = "Ring",
            Slots = 1,
            Note = "glows"
        };

        var vm = new TestGearItemVM(magic);

        Assert.Equal("Ring", vm.Name);
        Assert.Equal(1, vm.Slots);
        Assert.Equal("", vm.ItemType);   // Magic items have no ItemType
        Assert.Equal("glows", vm.Note);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GearItemViewModel_SlotsContribute()
    {
        // Verify slot counts are correctly propagated from both source types
        var gear = new GearItem { Name = "Chain Mail", Slots = 2, ItemType = "armor", Note = "" };
        var magic = new MagicItem { Name = "Cloak of Shadows", Slots = 3, Note = "heavy" };

        var vmGear = new TestGearItemVM(gear);
        var vmMagic = new TestGearItemVM(magic);

        Assert.Equal(2, vmGear.Slots);
        Assert.Equal(3, vmMagic.Slots);
    }
}
