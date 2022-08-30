using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using KeyWord.ClientApplication.Helpers;
using KeyWord.ClientApplication.Models;
using KeyWord.Credentials;
using KeyWord.Storage;

namespace KeyWord.ClientApplication.ViewModels;

public partial class CredentialsViewModel : ObservableObject
{
    private readonly IList<CredentialsListElement> _source;
    public ObservableCollection<CredentialsGroup> CredentialsGroups { get; private set; }

    public ICommand SearchCommand => new Command<string>(SearchElement);
    public ICommand RefreshCommand => new Command(async () => await RefreshStorageAsync());

    private readonly ICredentialsStorage _storage;
    private string _searchString;
    [ObservableProperty] private bool _isRefreshing;

    public CredentialsViewModel()
    {
        _storage = ServiceHelper.GetService<ICredentialsStorage>();
        _source = new List<CredentialsListElement>();
        CreateCredentialsIdentityList();
    }

    private void CreateCredentialsIdentityList()
    {
        _source.Add(new CredentialsListElement()
        {
            Id = 1,
            Identifier = "github.com",
            Login = "username@email.com"
        });

        _source.Add(new CredentialsListElement()
        {
            Id = 2,
            Identifier = "yahoo.com",
            Login = "username@email.com"
        });

        _source.Add(new CredentialsListElement()
        {
            Id = 3,
            Identifier = "yandex.com",
            Login = "username@email.com"
        });

        _source.Add(new CredentialsListElement()
        {
            Id = 4,
            Identifier = "google.com",
            Login = "username@email.com"
        });

        var groups = ExtractGroups(_source);
        // var groups = ExtractGroups(ExtractElements(_storage.GetIdentities()));
        CredentialsGroups = new ObservableCollection<CredentialsGroup>(groups);
    }

    private void SearchElement(string obj)
    {
        _searchString = obj;
        RefreshList(_source);
    }

    private async Task RefreshStorageAsync()
    {
        // TODO
        IsRefreshing = true;
        await Task.Delay(1000);
        RefreshList(_source);
        IsRefreshing = false;
    }

    private void RefreshList(IEnumerable<CredentialsListElement> elements)
    {
        CredentialsGroups.Clear();
        var dataSource = FilterElements(elements, _searchString);
        foreach (var element in ExtractGroups(dataSource))
        {
            CredentialsGroups.Add(element);
        }
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
            .GroupBy(x => char.ToUpperInvariant(x.Identifier[0]))
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
}