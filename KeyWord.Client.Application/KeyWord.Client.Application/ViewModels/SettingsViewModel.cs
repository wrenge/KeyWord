using KeyWord.Client.Storage;
using Xamarin.Forms;

namespace KeyWord.Client.Application.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private bool _syncButtonEnabled;
        public ICredentialsStorage Storage => DependencyService.Get<ICredentialsStorage>();
        public string Token { get; set; }
        public bool IsSyncing { get; set; }

        public bool SyncButtonEnabled
        {
            get => _syncButtonEnabled;
            set => SetProperty(ref _syncButtonEnabled, value);
        }
    }
}