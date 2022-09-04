using KeyWord.Credentials;
using KeyWord.Crypto;
using Microsoft.Data.Sqlite;

namespace KeyWord.Client.Storage.Mobile.Tests
{
    public class Tests
    {
        private const string Password = "mySecr3tP@$$w0rd!!!";
        private const string WrongPassword = "anotherPassword";

        [SetUp]
        public void Setup()
        {
            var dbPath = GetDbPath();
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }

        [TearDown]
        public void TearDown()
        {
            // Hacks to force SQLite release a file
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var dbPath = GetDbPath();
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }

        private static string GetDbPath()
        {
            var currentContext = TestContext.CurrentContext;
            var dbPath = $"{currentContext.Test.ID}.db3";
            return Path.Combine(currentContext.TestDirectory, dbPath);
        }

        [Test]
        public void TestSave()
        {
            var storage = new CredentialsStorageMobile(new TestDatabasePath(""), GetDbPath());
            storage.Password = Password;
            storage.ChangePassword(Password);
            Assert.AreEqual(storage.Count, 0);

            var a = new ClassicCredentialsInfo
            {
                Id = 0,
                Identifier = "www.google.com",
                Login = "Test1",
                Password = "MyS3cretP@$$w0rD!!!"
            };
            storage.SaveInfo(a);

            var identities = storage.GetIdentities();
            Assert.Greater(identities.Count, 0);
            Assert.Greater(identities[0].Id, 0);
            a.Id = identities[0].Id;
            Assert.AreEqual(storage.FindInfo(identities[0].Id), a);
            Assert.Throws<ElementExistsException>(() => storage.SaveInfo(a));
        }

        [Test]
        public void TestUpdate()
        {
            var storage = new CredentialsStorageMobile(new TestDatabasePath(""), GetDbPath());
            storage.Password = Password;
            storage.ChangePassword(Password);
            Assert.AreEqual(storage.Count, 0);

            var a = new ClassicCredentialsInfo
            {
                Id = 0,
                Identifier = "www.google.com",
                Login = "Test1",
                Password = "MyS3cretP@$$w0rD!!!"
            };
            storage.SaveInfo(a);

            var b = new ClassicCredentialsInfo
            {
                Id = 5,
                Identifier = "www.yahoo.com",
                Login = "Test2",
                Password = "MyS3cretP@$$w0rD!!!"
            };

            var identities = storage.GetIdentities();
            storage.UpdateInfo(identities[0].Id, b);
            Assert.AreNotEqual(b.Id, storage.GetIdentities()[0].Id);
            b.Id = identities[0].Id;
            Assert.AreEqual(b, storage.FindInfo(identities[0].Id));
        }

        [Test]
        public void TestDelete()
        {
            var storage = new CredentialsStorageMobile(new TestDatabasePath(""), GetDbPath());
            storage.Password = Password;
            storage.ChangePassword(Password);
            Assert.AreEqual(storage.Count, 0);

            var a = new ClassicCredentialsInfo
            {
                Id = 0,
                Identifier = "www.google.com",
                Login = "Test1",
                Password = "MyS3cretP@$$w0rD!!!"
            };

            var b = new ClassicCredentialsInfo
            {
                Id = 0,
                Identifier = "www.yahoo.com",
                Login = "Test2",
                Password = "MyS3cretP@$$w0rD!!!"
            };

            var c = new ClassicCredentialsInfo
            {
                Id = 0,
                Identifier = "www.yandex.com",
                Login = "Test3",
                Password = "MyS3cretP@$$w0rD!!!"
            };

            storage.SaveInfo(a);
            storage.SaveInfo(b);
            storage.SaveInfo(c);
            Assert.Greater(storage.Count, 0);
            Assert.IsTrue(storage.GetIdentities().Any(x => x.Login == b.Login));

            storage.DeleteInfo(storage.GetIdentities().First(x => x.Login == b.Login).Id);
            Assert.IsFalse(storage.GetIdentities().Any(x => x.Login == b.Login));
            Assert.IsTrue(storage.GetIdentities().Any(x => x.Login == a.Login));
            Assert.IsTrue(storage.GetIdentities().Any(x => x.Login == c.Login));
        }

        [Test]
        public void TestPassword()
        {
            var storage = new CredentialsStorageMobile(new TestDatabasePath(""), GetDbPath());
            Assert.Catch<PasswordInvalidException>(() =>
            {
                var a = new ClassicCredentialsInfo
                {
                    Id = 0,
                    Identifier = "www.google.com",
                    Login = "Test1",
                    Password = "MyS3cretP@$$w0rD!!!"
                };
                storage.SaveInfo(a);
            });

            storage.Password = Password;
            Assert.IsFalse(storage.IsPasswordCorrect());
            storage.ChangePassword(Password);
            Assert.IsTrue(storage.IsPasswordCorrect());
            storage.Password = WrongPassword;
            Assert.IsFalse(storage.IsPasswordCorrect());
        }

        [Test]
        public void TestPasswordExistence()
        {
            var storage = new CredentialsStorageMobile(new TestDatabasePath(""), GetDbPath());
            Assert.IsFalse(storage.HasPassword());
            storage.ChangePassword(Password);
            Assert.IsTrue(storage.HasPassword());
        }

        [Test]
        public void TestBulkEdit()
        {
            var storage = new CredentialsStorageMobile(new TestDatabasePath(""), GetDbPath());
            storage.Password = Password;
            storage.ChangePassword(Password);
            var df = new Pbkdf2(CryptoConstants.KdIterations, CryptoConstants.KdLength);
            var passwordHash = df.ComputeKey(new ByteText(Password), new ByteText(CryptoConstants.KdSalt1));
            var added = new[]
            {
                new ClassicCredentialsInfo
                {
                    Id = 1,
                    Identifier = "www.google.com",
                    Login = "Test1",
                    Password = "MyS3cretP@$$w0rD!!!",
                    CreationTime = new DateTime(2022, 09, 04)
                }.EncryptClassic(passwordHash),
                new ClassicCredentialsInfo
                {
                    Id = 2,
                    Identifier = "www.yahoo.com",
                    Login = "Test2",
                    Password = "MyS3cretP@$$w0rD!!!",
                    CreationTime = new DateTime(2022, 09, 04)
                }.EncryptClassic(passwordHash),
                new ClassicCredentialsInfo
                {
                    Id = 4,
                    Identifier = "www.yandex.com",
                    Login = "Test3",
                    Password = "MyS3cretP@$$w0rD!!!",
                    CreationTime = new DateTime(2022, 09, 04)
                }.EncryptClassic(passwordHash)
            };
            storage.SetAddedCredentials(added);
            Assert.AreEqual(3, storage.GetAddedCredentials(new DateTime()).Count());

            var modified = new[]
            {
                new ClassicCredentialsInfo
                {
                    Id = 1,
                    Identifier = "www.google.com",
                    Login = "Test1_Modified",
                    Password = "MyS3cretP@$$w0rD!!!",
                    CreationTime = new DateTime(2022, 09, 03),
                    ModificationTime = new DateTime(2022, 09, 04)
                }.EncryptClassic(passwordHash),
                new ClassicCredentialsInfo
                {
                    Id = 2,
                    Identifier = "www.yahoo.com",
                    Login = "Test2_Modified",
                    Password = "MyS3cretP@$$w0rD!!!",
                    CreationTime = new DateTime(2022, 09, 03),
                    ModificationTime = new DateTime(2022, 09, 04)
                }.EncryptClassic(passwordHash),
                new ClassicCredentialsInfo
                {
                    Id = 4,
                    Identifier = "www.yandex.com",
                    Login = "Test3_Modified",
                    Password = "MyS3cretP@$$w0rD!!!",
                    CreationTime = new DateTime(2022, 09, 03),
                    ModificationTime = new DateTime(2022, 09, 04)
                }.EncryptClassic(passwordHash)
            };

            storage.SetModifiedCredentials(modified);
            Assert.AreEqual(3, storage.GetModifiedCredentials(new DateTime(2022, 09, 03)).Count());

            storage.SetDeletedCredentials(new[] {1, 2, 4});
            Assert.AreEqual(3, storage.GetDeletedCredentials(new DateTime()).Count());
        }
    }
}