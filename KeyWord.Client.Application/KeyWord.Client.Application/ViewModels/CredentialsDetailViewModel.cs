using System;
using System.Diagnostics;
using System.Threading.Tasks;
using KeyWord.Client.Application.Views;
using KeyWord.Client.Storage;
using Xamarin.Forms;

namespace KeyWord.Client.Application.ViewModels
{
    [QueryProperty(nameof(Id), nameof(Id))]
    public class CredentialsDetailViewModel : BaseViewModel
    {
        private string _identifier;
        private string _name;
        private string _login;
        private string _password;
        private int _id;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                LoadItem(value);
            }
        }

        public ICredentialsStorage Storage => DependencyService.Get<ICredentialsStorage>();

        public string Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public Command RemoveCommand { get; }
        public Command EditCommand { get; }

        public CredentialsDetailViewModel ()
        {
            RemoveCommand = new Command(RemoveAsync);
            EditCommand = new Command(EditAsync);
        }

        private async void RemoveAsync()
        {
            try
            {
                Storage.DeleteInfo(Id);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private async void EditAsync()
        {
            await Shell.Current.GoToAsync($"{nameof(EditCredentialsPage)}?{nameof(EditCredentialsViewModel.Id)}={Id}");
        }

        public async void LoadItem(int id)
        {
            try
            {
                var item = Storage.FindInfo(id);
                if (item == null)
                    throw new NullReferenceException();

                Identifier = item.Identifier;
                Name = item.Name;
                Password = item.Password;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}