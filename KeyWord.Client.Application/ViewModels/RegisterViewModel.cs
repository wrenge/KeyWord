using CommunityToolkit.Mvvm.ComponentModel;

namespace KeyWord.Client.Application.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    [ObservableProperty] private bool _qrScannerEnabled = true;
    public bool IsRegistering { get; set; }
}