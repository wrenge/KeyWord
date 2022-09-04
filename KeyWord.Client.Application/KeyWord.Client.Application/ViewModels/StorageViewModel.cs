using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using KeyWord.Client.Application.Models;
using KeyWord.Client.Storage;
using Xamarin.Forms;

namespace KeyWord.Client.Application.ViewModels
{
    public class StorageViewModel : BaseViewModel
    {
        public ObservableCollection<StorageItem> Items { get; }
        public Command LoadItemsCommand { get; }
        public ICredentialsStorage Storage => DependencyService.Get<ICredentialsStorage>();

        public StorageViewModel ()
        {
            Title = "Storage";
            Items = new ObservableCollection<StorageItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        private async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            try
            {
                // TODO password
                var password = "password";
                if (!Storage.HasPassword())
                {
                    Storage.ChangePassword(password);
                }

                Storage.Password = password;
                
                Items.Clear();
                var items = Storage.GetIdentities();
                foreach (var identity in items)
                {
                    var item = new StorageItem()
                    {
                        Id = identity.Id,
                        Identifier = identity.Identifier,
                    };
                    Items.Add(item);
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
            
        }
    }
}