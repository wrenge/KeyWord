using CommunityToolkit.Mvvm.ComponentModel;
using KeyWord.ClientApplication.Helpers;
using KeyWord.Credentials;
using KeyWord.Storage;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace KeyWord.ClientApplication.ViewModels;

public class EditCredentialsViewModel : ObservableObject
{
    public ClassicCredentialsInfo CredentialsInfo { get; set; } = new ();
    public string AvatarCharacter => GetAvatarCharacter(CredentialsInfo.Identifier).ToString();

    public string Identifier
    {
        get => CredentialsInfo.Identifier;
        set
        {
            OnPropertyChanging(nameof(Title));
            OnPropertyChanging(nameof(AvatarCharacter));
            OnPropertyChanging();
            CredentialsInfo.Identifier = value;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(AvatarCharacter));
            OnPropertyChanged();
        }
    }

    public string Title => string.IsNullOrWhiteSpace(Identifier) ? "New login info" : Identifier;

    public string Login
    {
        get => CredentialsInfo.Login;
        set
        {
            OnPropertyChanging();
            CredentialsInfo.Login = value;
            OnPropertyChanged();
        }
    }
    
    public string Password
    {
        get => CredentialsInfo.Password;
        set
        {
            OnPropertyChanging();
            CredentialsInfo.Password = value;
            OnPropertyChanged();
        }
    }

    private static char GetAvatarCharacter(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return '-';

        return char.ToUpperInvariant(identifier.Replace("www.", "")[0]);
    }

    public void SaveInfo()
    {
        var storage = ServiceHelper.GetService<ICredentialsStorage>();
        if (CredentialsInfo.Id > 0)
        {
            storage.UpdateInfo(CredentialsInfo.Id, CredentialsInfo);
        }
        else
        {
            storage.SaveInfo(CredentialsInfo);
        }
    }
}