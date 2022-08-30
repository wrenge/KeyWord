using KeyWord.Storage;

namespace KeyWord.ClientApplication.Windows;

public class WindowsDatabasePath : IDatabasePath
{
    public string GetPath(string fileName)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyWord");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return Path.Combine(path, fileName);
    }
}