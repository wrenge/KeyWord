using KeyWord.Storage;

namespace KeyWord.ClientApplication.MacCatalyst;

public class MacDatabasePath : IDatabasePath
{
    public string GetPath(string fileName)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", fileName);
    }
}