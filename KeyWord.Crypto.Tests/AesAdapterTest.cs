using NUnit.Framework;

namespace KeyWord.Crypto.Tests;

public class Tests
{
    private const string Text = "Зашифруй меня";
    private const string Password = "Я - ключ!";
    private const string Salt = "aselrias38490a32"; // Random
    private const string Vector = "8947az34awl34kjq"; // Random
    
    [Test]
    public void TestAes()
    {
        var cipherAlg1 = new AesCbc(new ByteText(Vector));
        var kdAlg1 = new Pbkdf2(10, 16);
        var key1 = kdAlg1.ComputeKey(new ByteText(Password), new ByteText(Salt));
        var encrypted = cipherAlg1.Encrypt(new ByteText(Text), key1);
        
        var cipherAlg2 = new AesCbc(new ByteText(Vector));
        var kdAlg2 = new Pbkdf2(10, 16);
        var key2 = kdAlg2.ComputeKey(new ByteText(Password), new ByteText(Salt));
        
        Assert.AreEqual(Text, cipherAlg2.Decrypt(encrypted, key2).ToString());
    }
}