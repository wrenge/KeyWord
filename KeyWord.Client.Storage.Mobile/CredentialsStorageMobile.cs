using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeyWord.Credentials;
using KeyWord.Crypto;

namespace KeyWord.Client.Storage.Mobile
{
    public class CredentialsStorageMobile : ICredentialsStorage
    {
        public string Password
        {
            set => SetPassword(value);
        }

        public int Count
        {
            get
            {
                using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
                    return dbContext.ClassicCredentialsInfos.Count();
            }
        }

        public string DbFileName { get; }
        private ByteText? _savedPasswordHash;
        private ByteText? _checkPasswordHash;
        private ByteText? _enteredPasswordHash;
        private const string SavedPasswordHashKey = "PasswordHash";
        private IDatabasePath _databasePath;

        public CredentialsStorageMobile(IDatabasePath databasePath, string dbFileName)
        {
            DbFileName = dbFileName;
            _databasePath = databasePath;
            var savePasswordHash = FindKeyValue(SavedPasswordHashKey)?.Value;
            _savedPasswordHash = savePasswordHash == null ? (ByteText?) null : new ByteText(Convert.FromBase64String(savePasswordHash));
        }

        /// <summary>
        /// Returns list of credentials identities.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PasswordInvalidException">Password null or incorrect.</exception>
        public IReadOnlyList<CredentialsIdentity> GetIdentities()
        {
            if (_enteredPasswordHash == null || !IsPasswordCorrect())
                throw new PasswordInvalidException();

            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                return dbContext.ClassicCredentialsInfos
                    .Where(x => x.RemoveTime == null)
                    .Select(x => x.GetClassicDecrypted(_enteredPasswordHash.Value).ToCredentialsIdentity())
                    .ToArray();
            }
        }

        /// <summary>
        /// Tries to find credential info with id
        /// </summary>
        /// <param name="id">Id of proposed info</param>
        /// <returns>ICredentialsInfo if found, null if not found.</returns>
        /// <exception cref="PasswordInvalidException">Password null or incorrect.</exception>
        /// <exception cref="ArgumentException">passed id lesser or equal to zero.</exception>
        public ICredentialsInfo FindInfo(int id)
        {
            if (_enteredPasswordHash == null || !IsPasswordCorrect())
                throw new PasswordInvalidException();
            if (id <= 0)
                throw new ArgumentException(null, nameof(id));

            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var info = dbContext.ClassicCredentialsInfos
                    .FirstOrDefault(x => x.Id == id);

                return info?.GetClassicDecrypted(_enteredPasswordHash.Value);
            }
        }

        /// <summary>
        ///     <p>Saves credentials info.</p>
        ///     <b>Currently only supports <see cref="ClassicCredentialsInfo"/>.</b>
        /// </summary>
        /// <exception cref="PasswordInvalidException">Password null or incorrect.</exception>
        /// <exception cref="ElementExistsException">Found duplicate item while saving.</exception>
        /// <exception cref="NotSupportedException">Passed non-supported info.</exception>
        public void SaveInfo(ICredentialsInfo info)
        {
            if (_enteredPasswordHash == null || !IsPasswordCorrect())
                throw new PasswordInvalidException();

            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var exists = dbContext.ClassicCredentialsInfos
                    .AsEnumerable()
                    .Select(x => x.GetClassicDecrypted(_enteredPasswordHash.Value))
                    .Any(x => x.Identifier == info.Identifier && x.Login == info.Login);

                if (exists)
                    throw new ElementExistsException();

                if (!(info is ClassicCredentialsInfo classicInfo))
                    throw new NotSupportedException(info.GetType().ToString());

                dbContext.ClassicCredentialsInfos.Add(classicInfo.GetClassicEncrypted(_enteredPasswordHash.Value));
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        ///     <p>Replaces existing info with a new one.</p>
        ///     <b>Currently only supports <see cref="ClassicCredentialsInfo"/> </b>
        /// </summary>
        /// <param name="id">id of replaced info</param>
        /// <param name="info">new info</param>
        /// <exception cref="PasswordInvalidException">Password null or incorrect.</exception>
        /// <exception cref="ArgumentException">id is lesser or equal to zero.</exception>
        /// <exception cref="ElementNotExistException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public void UpdateInfo(int id, ICredentialsInfo info)
        {
            if (_enteredPasswordHash == null || !IsPasswordCorrect())
                throw new PasswordInvalidException();
            if (id <= 0)
                throw new ArgumentException(null, nameof(id));

            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var oldInfo = dbContext.ClassicCredentialsInfos.FirstOrDefault(x => x.Id == id);
                if (oldInfo == null)
                    throw new ElementNotExistException();
                if (!(info is ClassicCredentialsInfo classicInfo))
                    throw new NotSupportedException(info.GetType().ToString());
                // TODO: добавить проверку на ElementExistsException

                var newInfo = new ClassicCredentialsInfo(classicInfo);
                newInfo.Id = id;
                dbContext.Entry(oldInfo).CurrentValues
                    .SetValues(newInfo.GetClassicEncrypted(_enteredPasswordHash.Value));
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="PasswordInvalidException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ElementNotExistException"></exception>
        public void DeleteInfo(int id)
        {
            if (_enteredPasswordHash == null || !IsPasswordCorrect())
                throw new PasswordInvalidException();
            if (id <= 0)
                throw new ArgumentException(null, nameof(id));

            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var info = dbContext.ClassicCredentialsInfos
                    .FirstOrDefault(x => x.Id == id);

                if (info == null)
                    throw new ElementNotExistException();

                dbContext.ClassicCredentialsInfos.Remove(info);
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsPasswordCorrect()
        {
            if (_checkPasswordHash == null)
                return false;

            if (_savedPasswordHash == null)
                return false;

            return _savedPasswordHash.Value.ToBase64() == _checkPasswordHash.Value.ToBase64();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HasPassword()
        {
            return _savedPasswordHash != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newPassword"></param>
        /// <exception cref="PasswordInvalidException"></exception>
        public void ChangePassword(string newPassword)
        {
            if (_savedPasswordHash == null)
            {
                var df = new Pbkdf2(CryptoConstants.KdIterations, CryptoConstants.KdLength);
                var key = df.ComputeKey(new ByteText(newPassword), new ByteText(CryptoConstants.KdSalt2));
                SetKeyValue(SavedPasswordHashKey, key.ToBase64());
                _savedPasswordHash = key;
            }
            else
            {
                if (_enteredPasswordHash == null || !IsPasswordCorrect())
                    throw new PasswordInvalidException();

                var enteredPasswordHash = _enteredPasswordHash.Value;
                var df = new Pbkdf2(CryptoConstants.KdIterations, CryptoConstants.KdLength);
                var newPasswordHash = df.ComputeKey(new ByteText(newPassword), new ByteText(CryptoConstants.KdSalt2));
                using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
                {
                    foreach (var info in dbContext.ClassicCredentialsInfos)
                    {
                        var reEncrypted = info.GetClassicDecrypted(enteredPasswordHash).EncryptClassic(newPasswordHash);
                        dbContext.Entry(info).CurrentValues.SetValues(reEncrypted);
                    }

                    dbContext.SaveChanges();

                    SetKeyValue(SavedPasswordHashKey, newPasswordHash.ToBase64());
                    _savedPasswordHash = enteredPasswordHash;
                }
            }
        }

        public void SetKey(string key, string value)
        {
            SetKeyValue(key, value);
        }

        public string GetKeyOr(string key, string defaultValue = "")
        {
            return FindKeyValue(key)?.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private KeyValueEntry FindKeyValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var result = dbContext.KeyValues.FirstOrDefault(x => x.Key == key);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void SetKeyValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var kv = dbContext.KeyValues.FirstOrDefault(x => x.Key == key);
                if (kv != null)
                    kv.Value = value;
                else
                    dbContext.KeyValues.Add(new KeyValueEntry() {Key = key, Value = value});
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void SetPassword(string value)
        {
            if (value == null)
            {
                _checkPasswordHash = null;
                _enteredPasswordHash = null;
                return;
            }

            var df = new Pbkdf2(CryptoConstants.KdIterations, CryptoConstants.KdLength);
            _enteredPasswordHash = df.ComputeKey(new ByteText(value), new ByteText(CryptoConstants.KdSalt1));
            _checkPasswordHash = df.ComputeKey(new ByteText(value), new ByteText(CryptoConstants.KdSalt2));
        }
        
        public IEnumerable<ClassicCredentialsInfo> GetAddedCredentials(DateTime since)
        {
            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                return dbContext.ClassicCredentialsInfos
                    .Where(x => x.CreationTime > since)
                    .ToArray();
            }
        }

        public IEnumerable<ClassicCredentialsInfo> GetModifiedCredentials(DateTime since)
        {
            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                return dbContext.ClassicCredentialsInfos
                    .Where(x => x.CreationTime <= since && x.ModificationTime != null && x.ModificationTime > since)
                    .ToArray();
            }
        }

        public IEnumerable<int> GetDeletedCredentials(DateTime since)
        {
            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                return dbContext.ClassicCredentialsInfos
                    .Where(x => x.RemoveTime != null && x.RemoveTime > since)
                    .Select(x => x.Id)
                    .ToArray();
            }
        }
        
        public void SetAddedCredentials(IEnumerable<ClassicCredentialsInfo> infos)
        {
            // TODO: handle collisions
            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var infosQuery = infos.AsQueryable();
                var infosIds = infosQuery.Select(x => x.Id).ToArray();
                var existing = dbContext.ClassicCredentialsInfos
                    .Where(x => infosIds.Contains(x.Id));
                var infosToAdd = infosQuery
                    .Where(x => !existing.Any(y => y.Id == x.Id && y.CreationTime > x.CreationTime && y.ModificationTime > x.ModificationTime));
                dbContext.ClassicCredentialsInfos.AddRange(infosToAdd);
                dbContext.SaveChanges();
            }
        }

        public void SetModifiedCredentials(IEnumerable<ClassicCredentialsInfo> infos)
        {
            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var infosQuery = infos.AsQueryable();
                var infosIds = infosQuery.Select(y => y.Id).ToArray();
                var modified = dbContext.ClassicCredentialsInfos
                    .Where(x => infosIds.Contains(x.Id));
                foreach (var info in modified)
                {
                    var counterPart = infosQuery.First(x => x.Id == info.Id);
                    dbContext.Entry(info).CurrentValues.SetValues(counterPart);
                }

                dbContext.SaveChanges();
            }
        }

        public void SetDeletedCredentials(IEnumerable<int> infos)
        {
            using (var dbContext = new CredentialsContext(_databasePath, DbFileName))
            {
                var infosArray = infos.ToArray();
                var now = DateTime.UtcNow;
                var removed = dbContext.ClassicCredentialsInfos
                    .Where(x => infosArray.Contains(x.Id));
                foreach (var info in removed)
                {
                    dbContext.Entry(info).CurrentValues.SetValues(new ClassicCredentialsInfo() {Id = info.Id, RemoveTime = now});
                }

                dbContext.SaveChanges();
            }
        }
    }
}