namespace KeyWord.Credentials
{
    public static class CredentialsIdentityExtensions
    {
        public static CredentialsIdentity ToCredentialsIdentity<T>(this T info) where T : ICredentialsInfo
        {
            return new CredentialsIdentity()
            {
                Id = info.Id,
                Identifier = info.Identifier,
                Login = info.Login
            };
        }
    }
}