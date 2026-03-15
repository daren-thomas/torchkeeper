using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using SdCharacterSheet.Services;
using SdCharacterSheet.Storage;
using SdCharacterSheet.ViewModels;

namespace SdCharacterSheet;

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
        builder.Services.AddSingleton<IFileSaver>(_ => new CommunityToolkitFileSaverAdapter(FileSaver.Default));
        builder.Services.AddSingleton<CharacterFileService, MauiCharacterFileService>();
        builder.Services.AddSingleton<ShadowdarklingsImportService>();
        builder.Services.AddSingleton<CharacterViewModel>();

        // Tab pages — registered as Transient so Shell creates one per tab,
        // but they all receive the singleton CharacterViewModel via DI
        builder.Services.AddTransient<SdCharacterSheet.Views.SheetPage>();
        builder.Services.AddTransient<SdCharacterSheet.Views.GearPage>();
        builder.Services.AddTransient<SdCharacterSheet.Views.NotesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
