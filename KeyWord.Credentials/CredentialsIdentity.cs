using System;
using System.Collections.Generic;

namespace KeyWord.Credentials
{
    public class CredentialsIdentity : IEquatable<CredentialsIdentity>
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = "";
        public string Login { get; set; } = "";
        public string Name { get; set; } = "";

        public bool Equals(CredentialsIdentity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Identifier == other.Identifier && Login == other.Login && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CredentialsIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Identifier != null ? Identifier.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Login != null ? Login.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(CredentialsIdentity left, CredentialsIdentity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CredentialsIdentity left, CredentialsIdentity right)
        {
            return !Equals(left, right);
        }
    }
}