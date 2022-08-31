using KeyWord.Client.Storage;

namespace KeyWord.Client.Application
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App(ICredentialsStorage storage)
        {
            const string password = "testPassword"; // TODO
            InitializeComponent();
            if (!storage.HasPassword()) 
                storage.ChangePassword(password);
            storage.Password = password;
            
            MainPage = new AppShell();
        }
    }
}