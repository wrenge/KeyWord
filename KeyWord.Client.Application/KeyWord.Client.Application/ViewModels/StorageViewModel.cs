using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using KeyWord.Client.Application.Models;
using KeyWord.Client.Application.Views;
using KeyWord.Client.Storage;
using Xamarin.Forms;

namespace KeyWord.Client.Application.ViewModels
{
    public class StorageViewModel : BaseViewModel
    {
        private StorageItem _selectedItem;

        public StorageItem SelectedItem
        {
            get => _selectedItem;
            private set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        public ObservableCollection<StorageItem> Items { get; }
        public ICredentialsStorage Storage => DependencyService.Get<ICredentialsStorage>();
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<StorageItem> ItemTapped { get; }

        public StorageViewModel ()
        {
            Title = "Storage";
            Items = new ObservableCollection<StorageItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            
            ItemTapped = new Command<StorageItem>(OnItemSelected);
            AddItemCommand = new Command(OnAddItem);
        }

        private async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            try
            {
                if (Storage.HasPassword() && Storage.IsPasswordCorrect())
                {
                    Items.Clear();
                    var items = Storage.GetIdentities();
                    foreach (var identity in items)
                    {
                        var item = new StorageItem()
                        {
                            Id = identity.Id,
                            Identifier = identity.Identifier,
                            Name = identity.Name,
                            Login = identity.Login
                        };
                        Items.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }
        
        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(EditCredentialsPage));
        }

        private async void OnItemSelected(StorageItem value)
        {
            if (value == null)
                return;
            
            await Shell.Current.GoToAsync($"{nameof(CredentialsDetailPage)}?{nameof(CredentialsDetailViewModel.Id)}={value.Id}");
        }
    }
}