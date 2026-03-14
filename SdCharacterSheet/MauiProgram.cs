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

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
