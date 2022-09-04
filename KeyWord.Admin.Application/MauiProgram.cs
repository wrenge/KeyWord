using KeyWord.Admin.Application.Data;
using KeyWord.Admin.Services;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace KeyWord.Admin.Application;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.Services.AddSingleton<WeatherForecastService>();
        builder.Services.AddSingleton(_ => new DevicesService(new HttpClient {BaseAddress = new Uri("http://localhost:7078")}));
        builder.Services.AddSingleton(_ => new RegisterService(new HttpClient {BaseAddress = new Uri("http://localhost:7078")}));

        return builder.Build();
    }
}