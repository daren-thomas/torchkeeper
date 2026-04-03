using Microsoft.Maui.Storage;
using TorchKeeper.Models;
using TorchKeeper.Services;

namespace TorchKeeper.Services;

/// <summary>
/// MAUI-specific import service: opens a JSON file picker, then delegates
/// to ShadowdarklingsImportService for deserialization.
/// </summary>
public class MauiImportFileService
{
    private readonly ShadowdarklingsImportService _importService;

    private static readonly FilePickerFileType JsonFileType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS,     new[] { "public.json" } },
            { DevicePlatform.macOS,   new[] { "public.json" } },
            { DevicePlatform.Android, new[] { "application/json" } },
            { DevicePlatform.WinUI,   new[] { ".json" } },
        });

    private static readonly PickOptions JsonPickOptions = new()
    {
        PickerTitle = "Import Shadowdarklings character",
        FileTypes = JsonFileType,
    };

    public MauiImportFileService(ShadowdarklingsImportService importService)
    {
        _importService = importService;
    }

    public async Task<Character?> ImportAsync(CancellationToken ct = default)
    {
#if MACCATALYST
        var path = await MacFilePickerHelper.PickAsync(["json"]);
        if (path is null) return null;
        await using var stream = File.OpenRead(path);
        return await _importService.ImportAsync(stream);
#else
        var result = await MainThread.InvokeOnMainThreadAsync(
            () => FilePicker.Default.PickAsync(JsonPickOptions));
        if (result is null) return null;
        await using var stream = await result.OpenReadAsync();
        return await _importService.ImportAsync(stream);
#endif
    }
}
