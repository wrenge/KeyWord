using System;

namespace KeyWord.Credentials
{
    public class ClassicCredentialsInfo : ICredentialsInfo, IEquatable<ICredentialsInfo>
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Identifier { get; set; } = "";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public DateTime CreationTime { get; set; }
        public DateTime? ModificationTime { get; set; }
        public DateTime? RemoveTime { get; set; }

        public ClassicCredentialsInfo() { }

        public ClassicCredentialsInfo(ICredentialsInfo info)
        {
            Id = info.Id;
            Identifier = info.Identifier;
            Login = info.Login;
            Password = info.Password;
            CreationTime = info.CreationTime;
            ModificationTime = info.ModificationTime;
            RemoveTime = info.RemoveTime;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Identifier, Login, Password, CreationTime, ModificationTime, RemoveTime);
        }

        public static bool operator ==(ClassicCredentialsInfo? left, ClassicCredentialsInfo? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClassicCredentialsInfo? left, ClassicCredentialsInfo? right)
        {
            return !Equals(left, right);
        }

        public bool Equals(ICredentialsInfo? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id
                   && Name == other.Name
                   && Identifier == other.Identifier
                   && Login == other.Login
                   && Password == other.Password
                   && CreationTime.Equals(other.CreationTime)
                   && Nullable.Equals(ModificationTime, other.ModificationTime) &&
                   Nullable.Equals(RemoveTime, other.RemoveTime);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is ClassicCredentialsInfo other && Equals(other);
        }
    }
}