using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using KeyWord.ClientApplication.Models;
using KeyWord.Credentials;

namespace KeyWord.ClientApplication.ViewModels;

public partial class CredentialsViewModel : ObservableObject
{
    private readonly IList<CredentialsListElement> _source;
    public ObservableCollection<CredentialsGroup> CredentialsGroups { get; private set; }

    public ICommand SearchCommand => new Command<string>(SearchElement);

    public CredentialsViewModel ()
    {
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

        CredentialsGroups = new ObservableCollection<CredentialsGroup>(ExtractGroups(_source));
    }

    private void SearchElement(string obj)
    {
        CredentialsGroups.Clear();
        if (string.IsNullOrEmpty(obj))
        {
            foreach (var element in ExtractGroups(_source))
            {
                CredentialsGroups.Add(element);
            }
        }
        else
        {
            var dataSource = _source.Where(x => x.Identifier.Contains(obj) || x.Login.Contains(obj));
            foreach (var element in ExtractGroups(dataSource))
            {
                CredentialsGroups.Add(element);
            }
        }
    }

    private static IEnumerable<CredentialsGroup> ExtractGroups(IEnumerable<CredentialsListElement> elements)
    {
        return elements
            .OrderBy(x => x.Identifier)
            .GroupBy(x => char.ToUpperInvariant(x.Identifier[0]))
            .Select(x => new CredentialsGroup(x.Key.ToString(), x));
    }
}