using SdCharacterSheet.ViewModels;

namespace SdCharacterSheet.Views;

public partial class NotesPage : ContentPage
{
    public NotesPage(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
