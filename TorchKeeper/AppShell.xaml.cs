using TorchKeeper.ViewModels;

namespace TorchKeeper;

public partial class AppShell : Shell
{
    public AppShell(CharacterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
