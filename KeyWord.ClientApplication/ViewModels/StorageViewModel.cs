using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using KeyWord.ClientApplication.Helpers;
using KeyWord.ClientApplication.Models;
using KeyWord.Credentials;
using KeyWord.Storage;

namespace KeyWord.ClientApplication.ViewModels;

public partial class StorageViewModel : ObservableObject
{
    public IEnumerable<CredentialsListElement> Identities { get; set; }

    public ICommand SearchCommand => new Command<string>(SearchElement);
    public ICommand RefreshCommand => new Command(async () => await RefreshStorageAsync());
    public ICommand DeleteCommand => new Command<CredentialsListElement>(DeleteCredentials);
    public ICommand CopyPasswordCommand => new Command<CredentialsListElement>(async x => await CopyPasswordAsync(x));
    public ICommand CopyLoginCommand => new Command<CredentialsListElement>(async x => await CopyLoginAsync(x));

    private readonly ICredentialsStorage _storage;
    private string _searchString;
    [ObservableProperty] private IEnumerable<CredentialsGroup> _credentialsGroups;
    [ObservableProperty] private bool _isRefreshing;

    public StorageViewModel()
    {
        _storage = ServiceHelper.GetService<ICredentialsStorage>();
        RefreshLocalStorage();
    }

    private void SearchElement(string obj)
    {
        _searchString = obj;
        RefreshList(Identities);
    }

    private async Task RefreshStorageAsync()
    {
        try
        {
            await Task.Delay(1000);
            RefreshLocalStorage();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    public void RefreshLocalStorage()
    {
        Identities = ExtractElements(_storage.GetIdentities());
        RefreshList(Identities);
    }

    private void RefreshList(IEnumerable<CredentialsListElement> elements)
    {
        var dataSource = FilterElements(elements, _searchString);
        CredentialsGroups = ExtractGroups(dataSource);
    }

    private static IEnumerable<CredentialsListElement> FilterElements(IEnumerable<CredentialsListElement> list, string searchString)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            return list;
        }

        return list.Where(x => x.Identifier.Contains(searchString) || x.Login.Contains(searchString));
    }

    private static IEnumerable<CredentialsGroup> ExtractGroups(IEnumerable<CredentialsListElement> elements)
    {
        return elements
            .OrderBy(x => x.Identifier)
            .GroupBy(x => x.Identifier.Length > 0 ? char.ToUpperInvariant(x.Identifier[0]) : '#')
            .Select(x => new CredentialsGroup(x.Key.ToString(), x));
    }

    private static IEnumerable<CredentialsListElement> ExtractElements(IEnumerable<CredentialsIdentity> list)
    {
        return list.Select(x => new CredentialsListElement
        {
            Id = x.Id,
            Identifier = x.Identifier,
            Login = x.Login
        });
    }
    
    private void DeleteCredentials(CredentialsListElement info)
    {
        var storage = ServiceHelper.GetService<ICredentialsStorage>();
        storage.DeleteInfo(info.Id);
        RefreshLocalStorage();
    }
    
    private async Task CopyPasswordAsync(CredentialsListElement credentials)
    {
        var storage = ServiceHelper.GetService<ICredentialsStorage>();
        var info = storage.FindInfo(credentials.Id);
        if(info == null)
            return;

        await Clipboard.SetTextAsync(info.Password);
    }
    
    private async Task CopyLoginAsync(CredentialsListElement credentials)
    {
        await Clipboard.SetTextAsync(credentials.Login);
    }
}