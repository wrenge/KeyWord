using NUnit.Framework;

namespace KeyWord.Credentials.Test;

public class Tests
{
    [Test]
    public void TestCredentialsInfoEquality()
    {
        var a = new ClassicCredentialsInfo
        {
            Id = 1,
            Identifier = "www.google.com",
            Login = "Test1",
            Password = "MyS3cretP@$$w0rD!!!"
        };
        
        var b = new ClassicCredentialsInfo
        {
            Id = 1,
            Identifier = "www.google.com",
            Login = "Test1",
            Password = "MyS3cretP@$$w0rD!!!"
        };
        
        var c = new ClassicCredentialsInfo
        {
            Id = 1,
            Identifier = "www.yahoo.com",
            Login = "Test2",
            Password = "MyS3cretP@$$w0rD!!!"
        };
        
        Assert.AreEqual(a, b);
        Assert.AreNotEqual(a, c);
    }
    
    [Test]
    public void TestCredentialsIdentityEquality()
    {
        var a = new CredentialsIdentity()
        {
            Id = 1,
            Identifier = "www.google.com",
            Login = "Test1",
        };
        
        var b = new CredentialsIdentity()
        {
            Id = 1,
            Identifier = "www.google.com",
            Login = "Test1",
        };
        
        var c = new CredentialsIdentity()
        {
            Id = 1,
            Identifier = "www.yahoo.com",
            Login = "Test1",
        };
        
        Assert.AreEqual(a, b);
        Assert.AreNotEqual(a, c);
    }
}