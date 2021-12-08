using System.Text.Json;

namespace ImageGlass.Base;

public partial class Helpers
{
    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        WriteIndented = true,
    };


    /// <summary>
    /// Parse JSON string to object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T? ParseJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }


    /// <summary>
    /// Write value to JSON file
    /// </summary>
    /// <param name="jsonFilePath"></param>
    /// <param name="value"></param>
    public static void WriteJson(string jsonFilePath, object? value)
    {
        using var fs = File.Create(jsonFilePath);
        using var writter = new Utf8JsonWriter(fs, new JsonWriterOptions()
        {
            Indented = true,
        });


        JsonSerializer.Serialize(writter, value, JsonOptions);
    }
}
