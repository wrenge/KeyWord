using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyWord.Client.Application.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KeyWord.Client.Application.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StoragePage : ContentPage
    {
        private StorageViewModel _viewModel;
        
        public StoragePage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new StorageViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}