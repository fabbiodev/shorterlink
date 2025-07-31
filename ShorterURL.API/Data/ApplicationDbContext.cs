using LiteDB;
using ShorterURL.API.Entities;

namespace ShorterURL.API.Data;

public sealed class ApplicationDbContext : IDisposable
{
    private readonly LiteDatabase _db;

    public ILiteCollection<ShortUrl> Urls => _db.GetCollection<ShortUrl>("urls");
    public ILiteCollection<ClickLog> Clicks => _db.GetCollection<ClickLog>("clicks");

    public ApplicationDbContext(string dbPath)
    {
        _db = new LiteDatabase(dbPath);

        // индексы
        Urls.EnsureIndex(x => x.Code, unique: true);
        Urls.EnsureIndex(x => x.OriginalUrl);

        Clicks.EnsureIndex(c => c.ShortUrlId);
        Clicks.EnsureIndex(c => c.IpAddress);
    }

    public void Dispose() => _db.Dispose();
}
