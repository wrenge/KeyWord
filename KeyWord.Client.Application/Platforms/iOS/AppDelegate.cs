using Foundation;

namespace KeyWord.Client.Application.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(new IosDatabasePath());
    }
}