using KeyWord.Client.Application.Services;
using KeyWord.Client.Storage;

namespace KeyWord.Client.Application
{
    public partial class App
    {
        public App(ICredentialsStorage storage)
        {
            var passwordService = new PasswordStorageService();
            var password = passwordService.Get();
            if (string.IsNullOrEmpty(password))
            {
                password = passwordService.Generate();
                passwordService.Set(password);
            }
            
            if (!storage.HasPassword()) 
                storage.ChangePassword(password);
            storage.Password = password;

            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}