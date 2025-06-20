namespace Momentary.Domain.ValueObjects;

public record Photo
{
    public const int MAX_SIZE_IN_BYTES = 1024 * 160;
    private const int EXPIRATION_HOURS = 24;

    public string Base64Content { get; }
    public DateTime LastModified { get; }

    private Photo(string base64Content, DateTime lastModified)
    {
        Base64Content = base64Content;
        LastModified = lastModified;
    }

    public static Photo Create(byte[] content, DateTime lastModified)
    {
        if (content.Length > MAX_SIZE_IN_BYTES)
            throw new ArgumentException($"写真のサイズは {MAX_SIZE_IN_BYTES} バイト以下にする必要があります。");
        
        return new Photo(Convert.ToBase64String(content), lastModified);
    }
    
    public bool IsExpiredForPosting() => LastModified < DateTime.UtcNow.AddHours(-EXPIRATION_HOURS);
}