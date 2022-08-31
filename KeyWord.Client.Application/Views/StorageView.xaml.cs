using KeyWord.Client.Application.Models;
using KeyWord.Client.Application.ViewModels;

namespace KeyWord.Client.Application.Views;

public partial class StoragePage : ContentPage
{
    private readonly StorageViewModel _storageViewModel;

    public StoragePage()
    {
        InitializeComponent();
        _storageViewModel = new StorageViewModel();
        BindingContext = _storageViewModel;
    }

    private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _storageViewModel.SearchCommand.Execute(e.NewTextValue);
    }

    private async void AddButton_OnClicked(object sender, EventArgs e)
    {
        var page = Activator.CreateInstance<EditCredentialsView>();
        page.CredentialsChanged += OnCredentialsChanged;
        await Navigation.PushAsync(page);
    }

    private async void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        var page = Activator.CreateInstance<InspectCredentialsView>();
        page.CredentialsChanged += OnCredentialsChanged;
        page.BindingContext = (CredentialsListElement) e.Item;
        await Navigation.PushAsync(page);
    }

    private void OnCredentialsChanged()
    {
        _storageViewModel.RefreshLocalStorage();
    }
}