using KeyWord.Credentials;

namespace KeyWord.Server.Storage;

public class ClassicCredentialsStorageElement : ClassicCredentialsInfo, ICredentialsStorageElement
{
    public string AuthId { get; set; } = "";

    public ClassicCredentialsStorageElement() { }

    public ClassicCredentialsStorageElement(ICredentialsInfo info) : base(info) { }

    public ClassicCredentialsStorageElement(ICredentialsInfo info, string authId) : this(info)
    {
        AuthId = authId;
    }

    public ClassicCredentialsStorageElement(string authId) : this()
    {
        AuthId = authId;
    }
}