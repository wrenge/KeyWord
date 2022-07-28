using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace KeyWord.Storage.Mobile;

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