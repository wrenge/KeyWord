using System.Text.Json;

namespace KeyWord.Client.Storage.Mobile;

public class KeyValueEntry
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";

    public T GetValue<T>()
    {
        return JsonSerializer.Deserialize<T>(Value) ?? throw new JsonException();
    }
    
    public void SetValue<T>(T value)
    {
        Value = JsonSerializer.Serialize<T>(value);
    }
}