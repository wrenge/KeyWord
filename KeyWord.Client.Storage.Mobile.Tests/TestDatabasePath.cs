using System.IO;

namespace KeyWord.Client.Storage.Mobile.Tests;

public class TestDatabasePath : IDatabasePath
{
    private readonly string _path;

    public TestDatabasePath(string path)
    {
        _path = path;
    }

    public string GetPath(string fileName)
    {
        return Path.Combine(_path, fileName);
    }
}