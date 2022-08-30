using KeyWord.Storage;

namespace KeyWord.ClientApplication
{
    public partial class App : Application
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