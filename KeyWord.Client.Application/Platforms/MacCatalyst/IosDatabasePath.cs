using KeyWord.Client.Storage;

namespace KeyWord.Client.Application.MacCatalyst;

public class MacDatabasePath : IDatabasePath
{
    public string GetPath(string fileName)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", fileName);
    }
}