﻿namespace KeyWord.Server.Storage;

public class Device
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Token { get; set; } = "";
    public DateTime RegisterDate { get; set; }
}