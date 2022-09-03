using System.Text.Json;
using KeyWord.Client.Application.ViewModels;
using KeyWord.Client.Services;
using KeyWord.Communication;
using ZXing.Net.Maui;

namespace KeyWord.Client.Application.Views;

public partial class RegisterView
{
    private readonly RegisterViewModel _viewModel;

    public RegisterView()
    {
        InitializeComponent();
        _viewModel = new RegisterViewModel();
        BindingContext = _viewModel;
    }

    private async void CameraBarcodeReaderView_OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if(_viewModel.IsRegistering)
            return;
        
        QrCodeContent qrData = null;
        foreach (var qrCode in e.Results)
        {
            try
            {
                qrData = JsonSerializer.Deserialize<QrCodeContent>(qrCode.Value);
            }
            catch (JsonException) { }
        }
        if(qrData == null)
            return;

        _viewModel.IsRegistering = true;
        _viewModel.QrScannerEnabled = false;
        
        // Discover server ip
        var discovery = new DiscoveryService();
        var serverIp = await discovery.DiscoverServer(qrData.Port, qrData.Token, TimeSpan.FromSeconds(10));

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

        await Navigation.PopAsync();
        if (success)
        {
            await DisplayAlert("Success!", "You have registered device.", "OK");
        }
        else
        {
            await DisplayAlert("Fail!", "Error occured when registering device.", "OK");
        }
    }
}