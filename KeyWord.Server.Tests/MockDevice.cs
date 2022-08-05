using System;
using System.Collections.Generic;
using KeyWord.Credentials;

namespace KeyWord.Server.Tests;

public class MockDevice
{
    public string DeviceId = "";
    public string Token = "";
    public List<ClassicCredentialsInfo> AddedCredentials { get; set; } = new();
    public List<ClassicCredentialsInfo> ModifiedCredentials { get; set; } = new();
    public List<int> DeletedCredentialsIds { get; set; } = new();
}