using LiteDB;

namespace ShorterURL.API.Entities;

public class ClickLog
{
    [BsonId] public Guid Id { get; init; } = Guid.NewGuid();

    [BsonField] public required Guid ShortUrlId { get; set; }
    [BsonField] public required string IpAddress { get; set; }
    [BsonField] public string? Country { get; set; }
    [BsonField] public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
