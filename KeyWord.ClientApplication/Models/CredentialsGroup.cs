namespace KeyWord.ClientApplication.Models;

public class CredentialsGroup : List<CredentialsListElement>
{
    public string Name { get; private set; }

    public CredentialsGroup(string name, IEnumerable<CredentialsListElement> animals) : base(animals)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}