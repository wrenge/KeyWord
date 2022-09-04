using System;
using System.Diagnostics;
using KeyWord.Client.Storage;
using KeyWord.Credentials;
using Xamarin.Forms;

namespace KeyWord.Client.Application.ViewModels
{
    public class EditCredentialsViewModel : BaseViewModel
    {
        public ICredentialsStorage Storage => DependencyService.Get<ICredentialsStorage>();

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
        
        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        public EditCredentialsViewModel ()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => SaveCommand.ChangeCanExecute();
        }
        
        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
        
        private bool ValidateSave()
        {
            return !string.IsNullOrWhiteSpace(_identifier);
        }

        private void LoadItem(int value)
        {
            if(value <= 0)
                return;

            try
            {
                var item = Storage.FindInfo(value);
                if (item == null)
                    throw new NullReferenceException();
                Id = item.Id;
                Identifier = item.Identifier;
                Name = item.Name;
                Password = item.Password;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private async void OnSave()
        {
            try
            {
                var item = new ClassicCredentialsInfo()
                {
                    Id = Id,
                    Identifier = Identifier,
                    Name = string.IsNullOrEmpty(Name) ? Identifier : Name,
                    Login = Login,
                    Password = Password
                };
                if (Id <= 0)
                {
                    item.CreationTime = DateTime.Now;
                    Storage.SaveInfo(item);
                }
                else
                {
                    item.ModificationTime = DateTime.Now;
                    Storage.UpdateInfo(Id, item);
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}