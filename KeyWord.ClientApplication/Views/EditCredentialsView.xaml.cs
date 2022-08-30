using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyWord.ClientApplication.ViewModels;

namespace KeyWord.ClientApplication.Views;

public partial class EditCredentialsView : ContentPage
{
    private readonly EditCredentialsViewModel _viewModel;

    public EditCredentialsView()
    {
        InitializeComponent();
        _viewModel = new EditCredentialsViewModel();
        BindingContext = _viewModel;
    }

    private async void SaveButton_OnClicked(object sender, EventArgs e)
    {
        _viewModel.SaveInfo();
        await Navigation.PopAsync();
    }
}