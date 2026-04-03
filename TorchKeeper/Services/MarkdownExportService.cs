using System.Text;
using TorchKeeper.Core.Export;
using TorchKeeper.Models;
using TorchKeeper.ViewModels;

namespace TorchKeeper.Services;

public class MarkdownExportService
{
    private readonly IFileSaver _fileSaver;

    public MarkdownExportService(IFileSaver fileSaver)
        => _fileSaver = fileSaver;

    public async Task ExportAsync(CharacterViewModel vm, CancellationToken ct = default)
    {
        var exportData = MapToExportData(vm);
        var markdown = MarkdownBuilder.BuildMarkdown(exportData);
        var fileName = MarkdownBuilder.BuildFileName(exportData);

        if (DeviceInfo.Platform == DevicePlatform.iOS ||
            DeviceInfo.Platform == DevicePlatform.Android)
            await ShareAsync(markdown, fileName, ct);
        else
            await SaveAsync(markdown, fileName, ct);
    }

    private static CharacterExportData MapToExportData(CharacterViewModel vm)
    {
        // Map StatRows to StatExportData
        var stats = vm.StatRows
            .Select(row => new StatExportData(
                row.StatName,
                row.TotalScore,
                row.ModifierDisplay,
                row.BonusSources
                    .Select(b => new BonusExportData(
                        b.Label,
                        ExtractBonusValue(b.Source.BonusTo)))
                    .ToList()))
            .ToList();

        // Compute AC from Character.Bonuses where BonusTo starts with "AC:"
        var acBonuses = vm.Character.Bonuses
            .Where(b => b.BonusTo.StartsWith("AC:"))
            .ToList();

        var acBonusExports = acBonuses
            .Select(b => new BonusExportData(b.Label, ExtractBonusValue(b.BonusTo)))
            .ToList();

        // Sum AC bonus values — start from 0 (base AC comes from armor entries)
        var acTotal = acBonuses.Sum(b =>
        {
            var val = ExtractBonusValue(b.BonusTo);
            return int.TryParse(val, out var n) ? n : 0;
        });

        // Map GearItems — split into regular and free-carry (D-07, D-08)
        var gearItems = vm.GearItems
            .Where(g => !g.IsFreeCarry)
            .Select(g => new GearExportItem(g.Name, g.Slots))
            .ToList();

        var freeCarryItems = vm.GearItems
            .Where(g => g.IsFreeCarry)
            .Select(g => new GearExportItem(g.Name, g.Slots, true))
            .ToList();

        return new CharacterExportData
        {
            Name = vm.Name,
            Class = vm.Class,
            Ancestry = vm.Ancestry,
            Level = vm.Level,
            Title = vm.Title,
            Alignment = vm.Alignment,
            Background = vm.Background,
            Deity = vm.Deity,
            Languages = vm.Languages,
            XP = vm.XP,
            MaxXP = vm.MaxXP,
            CurrentHP = vm.CurrentHP,
            MaxHP = vm.MaxHP,
            Stats = stats,
            ACTotal = acTotal,
            ACBonuses = acBonusExports,
            GP = vm.GP,
            SP = vm.SP,
            CP = vm.CP,
            GearItems = gearItems,
            FreeCarryItems = freeCarryItems,
            GearSlotTotal = vm.GearSlotTotal,
            GearSlotsUsed = vm.GearSlotsUsed,
            CoinSlots = vm.CoinSlots,
            Attacks = vm.Attacks.ToList(),
            Talents = vm.Talents,
            SpellsKnown = vm.SpellsKnown,
            Notes = vm.Notes,
        };
    }

    private static string ExtractBonusValue(string bonusTo)
    {
        // BonusTo format: "STAT:+2" or "AC:11" — return the part after the colon
        var colonIndex = bonusTo.IndexOf(':');
        if (colonIndex >= 0 && colonIndex < bonusTo.Length - 1)
            return bonusTo[(colonIndex + 1)..];
        return "";
    }

    private static async Task ShareAsync(string markdown, string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(filePath, markdown, Encoding.UTF8, ct);
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Export character sheet",
            File = new ShareFile(filePath)
        });
    }

    private async Task SaveAsync(string markdown, string fileName, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(markdown);
        using var stream = new MemoryStream(bytes);
        await _fileSaver.SaveAsync(fileName, stream, ct);
    }
}
