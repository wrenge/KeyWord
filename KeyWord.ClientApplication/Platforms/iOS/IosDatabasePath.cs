﻿using KeyWord.Storage;

namespace KeyWord.ClientApplication.iOS;

public class IosDatabasePath : IDatabasePath
{
    public string GetPath(string fileName)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", fileName);
    }
}