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
    public partial class CredentialsDetailPage : ContentPage
    {
        public CredentialsDetailPage()
        {
            InitializeComponent();
            BindingContext = new CredentialsDetailViewModel();
        }
    }
}