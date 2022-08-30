using CommunityToolkit.Maui;
using KeyWord.Storage;
using KeyWord.Storage.Mobile;

namespace KeyWord.ClientApplication
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
                .UseMauiCommunityToolkit();
            builder.Services.AddSingleton<ICredentialsStorage>(_ => new CredentialsStorageMobile(databasePath, "storage.db3"));
            return builder.Build();
        }
    }
}