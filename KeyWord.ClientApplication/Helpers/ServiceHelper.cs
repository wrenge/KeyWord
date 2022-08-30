namespace KeyWord.ClientApplication.Helpers;

public static class ServiceHelper
{
    public static TService GetService<TService>() => Current.GetService<TService>();

    private static IServiceProvider Current =>
#if ANDROID
        MauiApplication.Current.Services;
#elif IOS || MACCATALYST
        MauiUIApplicationDelegate.Current.Services;
#elif WINDOWS
        MauiWinUIApplication.Current.Services;
#else
        throw new NotSupportedException();
#endif
}