using KeyWord.ClientApplication.Models;
using KeyWord.ClientApplication.ViewModels;

namespace KeyWord.ClientApplication.Views;

public partial class StoragePage : ContentPage
{
    private readonly CredentialsViewModel _credentialsViewModel;

    public StoragePage()
    {
        InitializeComponent();
        _credentialsViewModel = new CredentialsViewModel();
        BindingContext = _credentialsViewModel;
    }

    private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _credentialsViewModel.SearchCommand.Execute(e.NewTextValue);
    }

    private async void AddButton_OnClicked(object sender, EventArgs e)
    {
        var page = (Page) Activator.CreateInstance<InspectCredentialsView>();
        await Navigation.PushAsync(page);
    }

    private async void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        var page = (Page) Activator.CreateInstance<InspectCredentialsView>();
        page.BindingContext = (CredentialsListElement) e.Item;
        await Navigation.PushAsync(page);
    }
}