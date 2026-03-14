using Microsoft.Maui.Storage;
using SdCharacterSheet.Models;
using SdCharacterSheet.Services;

namespace SdCharacterSheet.Services;

/// <summary>Extends CharacterFileService with MAUI-specific file picker (OpenAsync).</summary>
public class MauiCharacterFileService : CharacterFileService
{
    private static readonly FilePickerFileType SdCharFileType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS,     new[] { "com.sdcharactersheet.sdchar" } },
            { DevicePlatform.macOS,   new[] { "com.sdcharactersheet.sdchar" } },
            { DevicePlatform.Android, new[] { "application/octet-stream" } },
            { DevicePlatform.WinUI,   new[] { ".sdchar" } },
        });

    private static readonly PickOptions SdCharPickOptions = new()
    {
        PickerTitle = "Open character file",
        FileTypes = SdCharFileType,
    };

    public MauiCharacterFileService(IFileSaver fileSaver) : base(fileSaver) { }

    public async Task<Character?> OpenAsync(CancellationToken ct = default)
    {
        var fileResult = await FilePicker.Default.PickAsync(SdCharPickOptions);
        if (fileResult is null) return null;
        using var stream = await fileResult.OpenReadAsync();
        var dto = await LoadFromStreamAsync(stream);
        return dto is null ? null : MapFromDto(dto);
    }
}
