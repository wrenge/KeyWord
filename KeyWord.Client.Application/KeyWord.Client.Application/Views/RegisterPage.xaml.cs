using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KeyWord.Client.Application.Services;
using KeyWord.Client.Application.ViewModels;
using KeyWord.Client.Services;
using KeyWord.Communication;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;

namespace KeyWord.Client.Application.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        private RegisterViewModel _viewModel;
        
        public RegisterPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new RegisterViewModel();
            _viewModel.QrScannerEnabled = true;
        }

        private async void Handle_OnScanResult(Result result)
        {
            if(_viewModel.IsRegistering)
                return;

            _viewModel.QrScannerEnabled = false;
            
            QrCodeContent qrData = null;
            try
            {
                qrData = JsonSerializer.Deserialize<QrCodeContent>(result.Text);
            }
            catch (JsonException) { }
            
            if(qrData == null)
            {
                _viewModel.QrScannerEnabled = true;
                return;
            }
            
            _viewModel.IsRegistering = true;
            
            // Discover server ip
            IPAddress serverIp = null;
            try
            {
                var discovery = new DiscoveryService();
                serverIp = await discovery.DiscoverServer(qrData.Port, qrData.Token, TimeSpan.FromSeconds(10));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            
            if (serverIp == null)
            {
                _viewModel.IsRegistering = false;
                _viewModel.QrScannerEnabled = true;
                return;
            }
            
            var serverUri = new UriBuilder("https", serverIp.ToString(), 7078).Uri;
            var register = new RegisterService(serverUri);
            var deviceName = DeviceInfo.Name;
            var deviceUid = DependencyService.Get<IDeviceUidService>().GetUid();
            var success = await register.TryRegister(deviceUid, deviceName, qrData.Token);
        }
    }
}