using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

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

        // Services registered in later plans:
        // builder.Services.AddSingleton<CharacterViewModel>();
        // builder.Services.AddSingleton<CharacterFileService>();
        // builder.Services.AddSingleton<ShadowdarklingsImportService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
