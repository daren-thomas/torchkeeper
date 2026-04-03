using TorchKeeper.ViewModels;

namespace TorchKeeper.Views;

public partial class NotesPage : ContentPage
{
    public NotesPage(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
