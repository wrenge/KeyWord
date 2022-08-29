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
}