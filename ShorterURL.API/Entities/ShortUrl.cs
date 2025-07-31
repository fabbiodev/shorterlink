using LiteDB;
using System.Text.Json.Serialization;

namespace ShorterURL.API.Entities;

public class ShortUrl
{
    [BsonId] public Guid Id { get; init; } = Guid.NewGuid();
    [BsonField] public required string OriginalUrl { get; set; }
    [BsonField] public required string Code { get; set; }
    [BsonField] public string ShortenedUrl { get; set; } = string.Empty;
    [BsonField] public int ClickCount { get; set; }
    [BsonField] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [BsonField] public DateTime? ExpirationDate { get; set; }

    [BsonIgnore]
    public bool IsExpired => ExpirationDate.HasValue && DateTime.UtcNow > ExpirationDate.Value;
}