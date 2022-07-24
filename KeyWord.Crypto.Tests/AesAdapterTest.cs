using NUnit.Framework;
using Wrenge.KeyWord.Crypto;

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
        var cipherAlg1 = new AesCbc();
        var kdAlg1 = new Pbkdf2(10, 16);
        var cipher1 = new Cipher(cipherAlg1, kdAlg1);
        
        var encrypted = cipher1.Encrypt(Text, Password, Salt, Vector);
        
        var cipherAlg2 = new AesCbc();
        var kdAlg2 = new Pbkdf2(10, 16);
        var cipher2 = new Cipher(cipherAlg2, kdAlg2);
        
        Assert.AreEqual(Text, cipher2.Decrypt(encrypted, Password, Salt, Vector));
    }
}