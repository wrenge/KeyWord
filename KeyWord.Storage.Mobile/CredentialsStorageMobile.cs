﻿using System;
using System.Collections.Generic;
using System.Linq;
using KeyWord.Credentials;
using KeyWord.Crypto;

namespace KeyWord.Storage.Mobile;

public class CredentialsStorageMobile : ICredentialsStorage
{
    public string? Password { set => SetPassword(value); }
    public int Count
    {
        get
        {
            using var dbContext = new CredentialsContext(DbFilePath);
            return dbContext.ClassicCredentialsInfos.Count();
        }
    }

    public string DbFilePath { get; }
    private ByteText? _savedPasswordHash;
    private ByteText? _checkPasswordHash;
    private ByteText? _enteredPasswordHash;
    private const string SavedPasswordHashKey = "PasswordHash";

    public CredentialsStorageMobile(string dbFilePath)
    {
        DbFilePath = dbFilePath;
        var savePasswordHash = FindKeyValue(SavedPasswordHashKey)?.Value;
        _savedPasswordHash = savePasswordHash == null ? null : new ByteText(savePasswordHash);
    }

    public IReadOnlyList<CredentialsIdentity> GetIdentities()
    {
        if (_enteredPasswordHash == null)
            throw new ArgumentNullException(null, nameof(Password));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        return dbContext.ClassicCredentialsInfos
            .Select(x => x.GetClassicDecrypted(_enteredPasswordHash.Value).ToCredentialsIdentity())
            .ToArray();
    }

    public ICredentialsInfo? FindInfo(int id)
    {
        if (_enteredPasswordHash == null)
            throw new ArgumentNullException(null, nameof(Password));
        if (id <= 0)
            throw new ArgumentException(null, nameof(id));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var info = dbContext.ClassicCredentialsInfos
            .FirstOrDefault(x => x.Id == id);
        
        return info?.GetClassicDecrypted(_enteredPasswordHash.Value);
    }

    public bool SaveInfo(ICredentialsInfo info)
    {
        if (_enteredPasswordHash == null)
            throw new ArgumentNullException(null, nameof(Password));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var exists = dbContext.ClassicCredentialsInfos
            .AsEnumerable()
            .Select(x => x.GetClassicDecrypted(_enteredPasswordHash.Value))
            .Any(x => x.Identifier == info.Identifier && x.Login == info.Login);
        
        if (exists)
            return false;

        if (info is not ClassicCredentialsInfo classicInfo)
            return false;

        dbContext.ClassicCredentialsInfos.Add(classicInfo.GetClassicEncrypted(_enteredPasswordHash.Value));
        dbContext.SaveChanges();
        return true;
    }

    public bool UpdateInfo(int id, ICredentialsInfo info)
    {
        if (_enteredPasswordHash == null)
            throw new ArgumentNullException(null, nameof(Password));
        if (id <= 0)
            throw new ArgumentException(null, nameof(id));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var oldInfo = dbContext.ClassicCredentialsInfos.FirstOrDefault(x => x.Id == id);
        if (oldInfo == null)
            return false;
        if (info is not ClassicCredentialsInfo classicInfo)
            return false;

        var newInfo = new ClassicCredentialsInfo(classicInfo);
        newInfo.Id = id;
        dbContext.Entry(oldInfo).CurrentValues.SetValues(newInfo.GetClassicEncrypted(_enteredPasswordHash.Value));
        dbContext.SaveChanges();
        
        return true;
    }

    public bool DeleteInfo(int id)
    {
        if (_enteredPasswordHash == null)
            throw new ArgumentNullException(null, nameof(Password));
        if (id <= 0)
            throw new ArgumentException(null, nameof(id));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var info = dbContext.ClassicCredentialsInfos
            .FirstOrDefault(x => x.Id == id);
        
        if (info == null)
            return false;

        dbContext.ClassicCredentialsInfos.Remove(info);
        dbContext.SaveChanges();
        
        return true;
    }

    public bool IsPasswordCorrect()
    {
        if (_checkPasswordHash == null)
            throw new ArgumentNullException(null, nameof(Password));
        
        if (_savedPasswordHash == null)
            return false;
        
        return _savedPasswordHash.Value.ToBase64() == _checkPasswordHash.Value.ToBase64();
    }

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
            if (_checkPasswordHash == null)
                throw new ArgumentNullException(null, nameof(Password));

            if (!IsPasswordCorrect())
                throw new ArgumentException("Password is not correct");

            var enteredPasswordHash = _enteredPasswordHash!.Value;
            var df = new Pbkdf2(CryptoConstants.KdIterations, CryptoConstants.KdLength);
            var newPasswordHash = df.ComputeKey(new ByteText(newPassword), new ByteText(CryptoConstants.KdSalt2));
            using var dbContext = new CredentialsContext(DbFilePath);
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

    private KeyValueEntry? FindKeyValue(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        var result = dbContext.KeyValues.FirstOrDefault(x => x.Key == key);
        return result;
    }
    
    private void SetKeyValue(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        
        using var dbContext = new CredentialsContext(DbFilePath);
        dbContext.KeyValues.Update(new KeyValueEntry() {Key = key, Value = value});
    }

    private void SetPassword(string? value)
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
}