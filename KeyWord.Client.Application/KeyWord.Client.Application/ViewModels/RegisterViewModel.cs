using System.Threading.Tasks;
using KeyWord.Communication;
using Xamarin.Forms;

namespace KeyWord.Client.Application.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private bool _qrScannerEnabled = true;

        public bool QrScannerEnabled
        {
            get => _qrScannerEnabled;
            set => SetProperty(ref _qrScannerEnabled, value);
        }
        public bool IsRegistering { get; set; }
    }
}