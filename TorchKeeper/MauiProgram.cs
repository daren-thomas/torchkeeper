using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using TorchKeeper.Services;
using TorchKeeper.Storage;
using TorchKeeper.ViewModels;

namespace TorchKeeper;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<TorchKeeper.Services.IFileSaver>(_ => new CommunityToolkitFileSaverAdapter(FileSaver.Default));
        builder.Services.AddSingleton<CharacterFileService, MauiCharacterFileService>();
        builder.Services.AddSingleton<ShadowdarklingsImportService>();
        builder.Services.AddSingleton<MauiImportFileService>();
        builder.Services.AddSingleton<MarkdownExportService>();
        builder.Services.AddSingleton<CharacterViewModel>(sp =>
            new CharacterViewModel(
                sp.GetRequiredService<MarkdownExportService>(),
                (MauiCharacterFileService)sp.GetRequiredService<CharacterFileService>(),
                sp.GetRequiredService<MauiImportFileService>()));
        builder.Services.AddSingleton<AppShell>();

        // Tab pages — registered as Transient so Shell creates one per tab,
        // but they all receive the singleton CharacterViewModel via DI
        builder.Services.AddTransient<TorchKeeper.Views.SheetPage>();
        builder.Services.AddTransient<TorchKeeper.Views.GearPage>();
        builder.Services.AddTransient<TorchKeeper.Views.NotesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
