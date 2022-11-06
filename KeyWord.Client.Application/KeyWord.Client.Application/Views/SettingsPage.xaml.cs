using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KeyWord.Client.Application.Services;
using KeyWord.Client.Application.ViewModels;
using KeyWord.Client.Services;
using KeyWord.Communication;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KeyWord.Client.Application.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new SettingsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Token = _viewModel.Storage.GetKeyOr(ApplicationConstants.StorageTokenKey);
            _viewModel.SyncButtonEnabled = !string.IsNullOrEmpty(_viewModel.Token);
        }

        private async void RegisterCell_OnTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        private async void SynchronizeCell_OnTapped(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_viewModel.Token) || _viewModel.IsSyncing)
                return;

            var serverIp = _viewModel.Storage.GetKeyOr(ApplicationConstants.ServerHostKey);
            _viewModel.IsSyncing = true;
            var serverUri = new UriBuilder(ApplicationConstants.ServerProtocol, serverIp, ApplicationConstants.ServerPort).Uri;
            var sync = new SynchronizationService(serverUri);
            SyncResponse resp = null;
            var deviceUid = DependencyService.Get<IDeviceUidService>().GetUid();
            var authId = SyncUtilities.GetAuthId(ApplicationConstants.StoragePassword);
            var lastSyncTimeString = _viewModel.Storage.GetKeyOr(ApplicationConstants.LastSyncTimeKey);
            var lastSyncTime = new DateTime();
            if (!string.IsNullOrEmpty(lastSyncTimeString))
                lastSyncTime = DateTime.Parse(lastSyncTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            var added = _viewModel.Storage.GetAddedCredentials(lastSyncTime);
            var modified = _viewModel.Storage.GetModifiedCredentials(lastSyncTime);
            var removed = _viewModel.Storage.GetDeletedCredentials(lastSyncTime);
            try
            {
                resp = await sync.TrySync(deviceUid, _viewModel.Token, authId.ToBase64(), lastSyncTime, added, modified, removed);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }

            if (resp == null)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread (async () => {
                    await DisplayAlert("Fail", "Unable to sync with server", "OK");
                });
                return;
            }
            
            _viewModel.Storage.SetKey(ApplicationConstants.LastSyncTimeKey, DateTime.UtcNow.ToString("O"));
            _viewModel.Storage.SetAddedCredentials(resp.SyncData.AddedCredentials);
            _viewModel.Storage.SetModifiedCredentials(resp.SyncData.ModifiedCredentials);
            _viewModel.Storage.SetDeletedCredentials(resp.SyncData.DeletedCredentialsIds);
            Xamarin.Forms.Device.BeginInvokeOnMainThread (async () => {
                await DisplayAlert("Success", "Synced with server!", "OK");
            });
        }

        private async void PlaySound_OnTapped(object sender, EventArgs e)
        {
//             var player = DependencyService.Get<IMediaPlayer>();
//             var filepath = @"KeyWord.Client.Application." + "honk.wav";
// // #if __IOS__
// //             filepath = "KeyWord.Client.Application.iOS." + filepath;
// // #elif __ANDROID__
// //             filepath = "KeyWord.Client.Application.Droid." + filepath;
// // #endif
//             await player.SetDataSource(filepath);
//             await player.Prepare();
//             player.Start();
            var player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            player.Load(GetStreamFromFile("honk.wav"));
            player.Play();
        }
        
        private Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("KeyWord.Client.Application." + filename);
            return stream;
        }
    }
}