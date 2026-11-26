using System.Text.Json;

namespace ChatCommon;

public record ChatMessage(
    string Type,
    string Nick,
    string Text,
    DateTime Time,
    string? To = null)
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string ToJson() => JsonSerializer.Serialize(this, Opts);

    public static ChatMessage FromJson(string line) =>
        JsonSerializer.Deserialize<ChatMessage>(line, Opts)
        ?? throw new JsonException("пустая строка");
}