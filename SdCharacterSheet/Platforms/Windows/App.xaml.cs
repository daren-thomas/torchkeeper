// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SdCharacterSheet.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        // File type filter for .sdchar is declared at the FilePicker call site in CharacterFileService
        // using FilePickerFileType with DevicePlatform.WinUI entry. No manifest changes needed for
        // unpackaged Windows apps (WindowsPackageType=None).
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
