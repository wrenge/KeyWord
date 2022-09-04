using System;
using System.IO;
using KeyWord.Client.Application.Droid;
using KeyWord.Client.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidDatabasePath))]
namespace KeyWord.Client.Application.Droid
{
    public class AndroidDatabasePath : IDatabasePath
    {
        public string GetPath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);

        }
    }
}