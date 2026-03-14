using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SdCharacterSheet.Services;

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
        builder.Services.AddSingleton<CharacterFileService>();
        // builder.Services.AddSingleton<CharacterViewModel>();
        // builder.Services.AddSingleton<ShadowdarklingsImportService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
