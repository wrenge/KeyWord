using System;
using System.Collections.Generic;

namespace KeyWord.Credentials
{
    public class CredentialsIdentity : IEquatable<CredentialsIdentity>
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = "";
        public string Login { get; set; } = "";

        public bool Equals(CredentialsIdentity other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Identifier == other.Identifier && Login == other.Login;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CredentialsIdentity) obj);
        }

        public override int GetHashCode()
        {
            int hashCode = -2102179360;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Identifier);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Login);
            return hashCode;
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