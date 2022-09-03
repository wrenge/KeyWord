namespace KeyWord.Client.Application.Views;

public partial class SettingsView : ContentPage
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void RegisterCell_OnTapped(object sender, EventArgs e)
    {
        var page = Activator.CreateInstance<RegisterView>();
        await Navigation.PushAsync(page);
    }
}