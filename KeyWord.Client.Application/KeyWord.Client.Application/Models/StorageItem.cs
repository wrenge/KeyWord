using System;
using Xamarin.Forms;

namespace KeyWord.Client.Application.Models
{
    public class StorageItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; }
        public string Login { get; set; }
        public string AvatarCharacter => GetAvatarCharacter(Identifier).ToString();
        public Color AvatarColor => GetAvatarColor(Id);

        private static char GetAvatarCharacter(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return '-';

            return char.ToUpperInvariant(identifier.Replace("www.", "")[0]);
        }

        private static Color GetAvatarColor(int id)
        {
            var random = new Random(id);
            return new Color(random.Next(256), random.Next(256), random.Next(256)).WithSaturation(1).WithLuminosity(0.5f);
        }
    }
}