﻿using KeyWord.Storage;

namespace KeyWord.ClientApplication.Android;

public class AndroidDatabasePath : IDatabasePath
{
    public string GetPath(string fileName)
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);
    }
}