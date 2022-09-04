using System;
using System.Collections.Generic;

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
            Name = info.Name;
            Login = info.Login;
            Password = info.Password;
            CreationTime = info.CreationTime;
            ModificationTime = info.ModificationTime;
            RemoveTime = info.RemoveTime;
        }

        public static bool operator ==(ClassicCredentialsInfo left, ClassicCredentialsInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClassicCredentialsInfo left, ClassicCredentialsInfo right)
        {
            return !Equals(left, right);
        }

        public bool Equals(ICredentialsInfo other)
        {
            if (other is null) return false;
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

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is ClassicCredentialsInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = 295467307;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Identifier);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Login);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Password);
            hashCode = hashCode * -1521134295 + CreationTime.GetHashCode();
            hashCode = hashCode * -1521134295 + ModificationTime.GetHashCode();
            hashCode = hashCode * -1521134295 + RemoveTime.GetHashCode();
            return hashCode;
        }
    }
}