using CommunityToolkit.Maui.Storage;
using CoreIFileSaver = TorchKeeper.Services.IFileSaver;

namespace TorchKeeper.Storage;

/// <summary>Adapts CommunityToolkit's IFileSaver to Core's IFileSaver interface.</summary>
public class CommunityToolkitFileSaverAdapter : CoreIFileSaver
{
    private readonly IFileSaver _inner;

    public CommunityToolkitFileSaverAdapter(IFileSaver inner) => _inner = inner;

    public async Task SaveAsync(string fileName, Stream stream, CancellationToken ct = default)
    {
        var result = await _inner.SaveAsync(fileName, stream, ct);
        if (!result.IsSuccessful)
            throw new IOException($"Save failed: {result.Exception?.Message}", result.Exception);
    }
}
