using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using TorchKeeper.ViewModels;
using TorchKeeper.Views.Popups;

namespace TorchKeeper.Views;

public partial class GearPage : ContentPage
{
    private readonly CharacterViewModel _vm;

    public GearPage(CharacterViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    private async void OnItemTapped(object sender, TappedEventArgs e)
    {
        var view = (View)sender;
        var item = (GearItemViewModel)view.BindingContext;
        await this.ShowPopupAsync(new GearItemPopup(_vm, item), new PopupOptions());
    }

    private async void OnAddItemClicked(object sender, EventArgs e)
    {
        await this.ShowPopupAsync(new GearItemPopup(_vm), new PopupOptions());
    }
}
