using SdCharacterSheet.ViewModels;

namespace SdCharacterSheet;

public partial class AppShell : Shell
{
    public AppShell(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
