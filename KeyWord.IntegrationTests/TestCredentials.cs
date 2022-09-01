using KeyWord.Credentials;

namespace KeyWord.IntegrationTests;

public class TestCredentials
{
    public int Count { get; }
    public readonly ClassicCredentialsInfo[] AddedCredentialsInfos;
    public readonly ClassicCredentialsInfo[] ModifiedCredentialsInfos;
    public readonly ClassicCredentialsInfo[] RemovedCredentialsInfos;

    public TestCredentials (int count)
    {
        Count = count;
        AddedCredentialsInfos = GenerateCredentialsInfos(count, TimeSpan.FromDays(1)).ToArray();
        ModifiedCredentialsInfos = GenerateCredentialsInfos(count, TimeSpan.FromDays(1), modified: true).ToArray();
        RemovedCredentialsInfos = GenerateCredentialsInfos(count, TimeSpan.FromDays(1), removed: true).ToArray();
    }

    private static IEnumerable<ClassicCredentialsInfo> GenerateCredentialsInfos(int count, TimeSpan delay,
        bool modified = false, bool removed = false)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new ClassicCredentialsInfo ()
            {
                Id = i + 1,
                Identifier = "www.google.com",
                Login = $"Login{i + 1}",
                Password = "Password",
                CreationTime = new DateTime(2022, 1, 1) + delay * i * (modified || removed ? 0 : 1),
                ModificationTime = modified ? new DateTime(2022, 1, 1) + delay * i : null,
                RemoveTime = removed ? new DateTime(2022, 1, 1) + delay * i : null,
            };
        }
    }
}