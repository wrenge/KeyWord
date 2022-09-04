using System;
using System.IO;
using KeyWord.Client.Application.iOS;
using KeyWord.Client.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(IosDatabasePath))]
namespace KeyWord.Client.Application.iOS
{
    public class IosDatabasePath : IDatabasePath
    {
        public string GetPath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", fileName);
        }
    }
}