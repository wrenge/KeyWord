using System;

namespace KeyWord.Credentials
{
    public class ClassicCredentialsInfo : ICredentialsInfo, IEquatable<ClassicCredentialsInfo>
    {
        public int Id { get; set; }
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
        }

        public bool Equals(ClassicCredentialsInfo? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id
                   && Identifier == other.Identifier 
                   && Login == other.Login 
                   && Password == other.Password 
                   && CreationTime.Equals(other.CreationTime) 
                   && Nullable.Equals(ModificationTime, other.ModificationTime);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClassicCredentialsInfo) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Identifier, Login, Password, CreationTime, ModificationTime);
        }

        public static bool operator ==(ClassicCredentialsInfo? left, ClassicCredentialsInfo? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClassicCredentialsInfo? left, ClassicCredentialsInfo? right)
        {
            return !Equals(left, right);
        }
    }
}