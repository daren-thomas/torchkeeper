using UIKit;
using UniformTypeIdentifiers;

namespace SdCharacterSheet.Services;

/// <summary>
/// Mac Catalyst file picker that bypasses MAUI's FilePicker (which uses the
/// deprecated UIApplication.KeyWindow and silently returns null on macOS 15+).
/// Uses ConnectedScenes to locate the presenting view controller reliably.
/// </summary>
internal static class MacFilePickerHelper
{
    /// <summary>
    /// Shows a UIDocumentPickerViewController for the given file extensions
    /// and returns the selected file path, or null if cancelled.
    /// </summary>
    internal static async Task<string?> PickAsync(string[] extensions)
    {
        var tcs = new TaskCompletionSource<string?>();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var vc = GetTopViewController();
            if (vc is null)
            {
                tcs.TrySetResult(null);
                return;
            }

            var types = extensions
                .Select(UTType.CreateFromExtension)
                .Where(t => t is not null)
                .Cast<UTType>()
                .ToArray();
            if (types.Length == 0)
                types = [UTType.CreateFromIdentifier("public.item")!];

            var picker = new UIDocumentPickerViewController(types, false)
            {
                AllowsMultipleSelection = false
            };

            picker.DidPickDocumentAtUrls += (_, e) =>
            {
                var url = e.Urls.FirstOrDefault();
                if (url is null) { tcs.TrySetResult(null); return; }
                url.StartAccessingSecurityScopedResource();
                tcs.TrySetResult(url.Path);
                url.StopAccessingSecurityScopedResource();
            };
            picker.WasCancelled += (_, _) => tcs.TrySetResult(null);

            vc.PresentViewController(picker, true, null);
        });

        return await tcs.Task;
    }

    private static UIViewController? GetTopViewController()
    {
        var scene = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .FirstOrDefault();

        var root = (scene?.Windows.FirstOrDefault(w => w.IsKeyWindow)
                    ?? scene?.Windows.FirstOrDefault())
                   ?.RootViewController;

        while (root?.PresentedViewController is { } presented)
            root = presented;

        return root;
    }
}
