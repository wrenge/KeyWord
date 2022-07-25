using System;

namespace KeyWord.Credentials
{
    public class CredentialsIdentity : IEquatable<CredentialsIdentity>
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = "";
        public string Login { get; set; } = "";

        public bool Equals(CredentialsIdentity? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Identifier == other.Identifier && Login == other.Login;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CredentialsIdentity) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Identifier, Login);
        }

        public static bool operator ==(CredentialsIdentity? left, CredentialsIdentity? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CredentialsIdentity? left, CredentialsIdentity? right)
        {
            return !Equals(left, right);
        }
    }
}