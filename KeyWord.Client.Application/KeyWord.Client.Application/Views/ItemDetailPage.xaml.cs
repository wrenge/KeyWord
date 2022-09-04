using KeyWord.Client.Application.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace KeyWord.Client.Application.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}