using System.Text.Json.Serialization;
namespace Momentary.Infrastructure.Dto;

// Firebase RTDB のデータ構造にマッピングするDTO
public class PostDto
{
    [JsonPropertyName("author")]
    public AuthorDto Author { get; set; } = new();

    [JsonPropertyName("imageBase64")]
    public string ImageBase64 { get; set; } = string.Empty;

    [JsonPropertyName("lastModified")]
    public long LastModified { get; set; }
    
    [JsonPropertyName("timestamp")]
    public object Timestamp { get; set; } = new Dictionary<string, string> { { ".sv", "timestamp" } };
}

public class AuthorDto
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("photoUrl")]
    public string PhotoUrl { get; set; } = string.Empty;
}