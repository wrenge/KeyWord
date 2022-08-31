using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyWord.ClientApplication.ViewModels;
using KeyWord.Credentials;

namespace KeyWord.ClientApplication.Views;

public partial class EditCredentialsView : ContentPage
{
    public event Action CredentialsChanged;
    
    private readonly EditCredentialsViewModel _viewModel;

    public EditCredentialsView()
    {
        InitializeComponent();
        _viewModel = new EditCredentialsViewModel();
        BindingContext = _viewModel;
    }

    private async void SaveButton_OnClicked(object sender, EventArgs e)
    {
        try
        {
            _viewModel.SaveInfo();
            CredentialsChanged?.Invoke();
            await Navigation.PopAsync();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }

    public void SetCredentials(ClassicCredentialsInfo info)
    {
        _viewModel.CredentialsInfo = info;
    }

    public ClassicCredentialsInfo GetCredentials() => _viewModel.CredentialsInfo;
}