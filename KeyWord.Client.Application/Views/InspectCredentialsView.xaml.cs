﻿using KeyWord.Client.Application.Helpers;
using KeyWord.Client.Application.Models;
using KeyWord.Credentials;
using KeyWord.Client.Storage;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace KeyWord.Client.Application.Views;

public partial class InspectCredentialsView : ContentPage
{
    public event Action CredentialsChanged;
    private ICredentialsInfo _credentials;
    private bool _passwordVisible = false;
    
    public InspectCredentialsView()
    {
        InitializeComponent();
    }

    private async void Url_OnTapped(object sender, EventArgs e)
    {
        try
        {
            var info = (CredentialsListElement) BindingContext;
            var uri = new UriBuilder("http", info.Identifier);
            await Browser.OpenAsync(uri.Uri);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }

    private async void Login_OnTapped(object sender, EventArgs e)
    {
        var info = (CredentialsListElement) BindingContext;
        await Clipboard.SetTextAsync(info.Login);
        await DisplayAlert("Done!", "Login copied to clipboard", "OK");
    }

    private async void Password_OnTapped(object sender, EventArgs e)
    {
        if (_credentials != null)
        {
            await Clipboard.SetTextAsync(_credentials.Password);
            await DisplayAlert("Done!", "Password copied to clipboard", "OK");
            return;
        }

        await TryFillCredentialsInfoAsync();
        if (_credentials != null)
        {
            await Clipboard.SetTextAsync(_credentials.Password);
            await DisplayAlert("Done!", "Password copied to clipboard", "OK");
        }
    }

    private async Task TryFillCredentialsInfoAsync()
    {
        var info = (CredentialsListElement) BindingContext;
        var isAuthenticated = false;
        if(await CrossFingerprint.Current.IsAvailableAsync(true))
        {
            var request =
                new AuthenticationRequestConfiguration("Prove you have fingers!",
                    "Because without it you can't have access");
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            isAuthenticated = result.Authenticated;
        }
        else
        {
            isAuthenticated = true;
        }

        var storage = ServiceHelper.GetService<ICredentialsStorage>();
        if (isAuthenticated)
        {
            _credentials = storage.FindInfo(info.Id);
        }
    }

    private async void PasswordEyeButton_OnTapped(object sender, EventArgs e)
    {
        if (_passwordVisible)
        {
            PasswordField.Text = "************";
            _passwordVisible = false;
            return;
        }
        
        if (_credentials != null)
        {
            PasswordField.Text = _credentials.Password;
            _passwordVisible = true;
            return;
        }

        await TryFillCredentialsInfoAsync();
        if (_credentials != null)
        {
            PasswordField.Text = _credentials.Password;
            _passwordVisible = true;
        }
    }

    private async void RemoveButton_OnTapped(object sender, EventArgs e)
    {
        var info = (CredentialsListElement) BindingContext;
        var storage = ServiceHelper.GetService<ICredentialsStorage>();
        storage.DeleteInfo(info.Id);
        CredentialsChanged?.Invoke();
        await Navigation.PopAsync();
    }

    private async void EditButton_OnClicked(object sender, EventArgs e)
    {
        await TryFillCredentialsInfoAsync();
        if(_credentials is ClassicCredentialsInfo info)
        {
            var page = Activator.CreateInstance<EditCredentialsView>();
            page.SetCredentials(info);
            page.CredentialsChanged += () =>
            {
                var credentials = page.GetCredentials();
                BindingContext = new CredentialsListElement()
                {
                    Id = credentials.Id,
                    Identifier = credentials.Identifier,
                    Login = credentials.Login
                };
                CredentialsChanged?.Invoke();
            };
            await Navigation.PushAsync(page);
        }
    }
}