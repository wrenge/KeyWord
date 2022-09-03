using CommunityToolkit.Maui;
using KeyWord.Client.Storage.Mobile;
using KeyWord.Client.Storage;
using ZXing.Net.Maui;

namespace KeyWord.Client.Application
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp(IDatabasePath databasePath)
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("ionicons.ttf", "IonIcons");
                })
                .UseBarcodeReader()
                .UseMauiCommunityToolkit();
            builder.Services.AddSingleton<ICredentialsStorage>(_ => new CredentialsStorageMobile(databasePath, "storage.db3"));
            return builder.Build();
        }
    }
}