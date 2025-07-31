using Microsoft.AspNetCore.Mvc;          // для [FromServices]
using ShorterURL.API.Data;
using ShorterURL.API.Entities;
using ShorterURL.API.Models;
using ShorterURL.API.Services;

namespace ShorterURL.API.Endpoints;

public static class ShortUrlEndpoints
{
    public static void MapShortUrlEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapPost("/shorten", CreateShortUrl);
        api.MapGet("/{code}/details", GetShortUrlDetails);
        api.MapDelete("/{code}", DeleteShortUrl);
        api.MapGet("/{code}/analytics", GetAnalytics);

        // редирект
        app.MapGet("/{code:regex(^[a-zA-Z0-9]+$)}", RedirectShortUrl);
    }

    private static async Task<IResult> CreateShortUrl(
        UrlRequest request,
        UrlShortenerService urlService,
        ApplicationDbContext db,
        HttpContext ctx)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
            return Results.BadRequest("Url is invalid.");

        var expireOn = request.ExpirationDurationInDays switch
        {
            null or 0 => DateTime.UtcNow.AddDays(10),
            int days => DateTime.UtcNow.AddDays(days)
        };

        var code = await urlService.GenerateUniqueCodeAsync(request.CustomCode);

        var entity = new ShortUrl
        {
            OriginalUrl = request.Url,
            Code = code,
            ShortenedUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{code}",
            ExpirationDate = expireOn
        };

        db.Urls.Insert(entity);
        return Results.Ok(new { entity.ShortenedUrl });
    }

    private static async Task<IResult> RedirectShortUrl(
        string code,
        ApplicationDbContext db,
        [FromServices] GeoService geo,
        HttpContext ctx)
    {
        var url = db.Urls.FindOne(u => u.Code == code);
        if (url is null) return Results.NotFound("Url not found.");
        if (url.IsExpired) return Results.NotFound("Url has expired.");

        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var country = await geo.GetCountryAsync(ip);
        db.Clicks.Insert(new ClickLog { ShortUrlId = url.Id, IpAddress = ip, Country = country });

        url.ClickCount++;
        db.Urls.Update(url);

        return Results.Redirect(url.OriginalUrl, false);
    }
    private static IResult GetAnalytics(string code, ApplicationDbContext db)
    {
        var url = db.Urls.FindOne(u => u.Code == code);
        if (url is null) return Results.NotFound("Url not found.");

        var clicks = db.Clicks.Find(c => c.ShortUrlId == url.Id).ToList();

        var resp = new
        {
            TotalClicks = clicks.Count,
            UniqueClicks = clicks.Select(c => c.IpAddress).Distinct().Count(),
            CountryStats = clicks
                .GroupBy(c => c.Country ?? "Unknown")
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count),
            DailyStats = clicks
                .GroupBy(c => c.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
        };
        return Results.Ok(resp);
    }

    private static IResult GetShortUrlDetails(string code, ApplicationDbContext db)
    {
        var url = db.Urls.FindOne(u => u.Code == code);
        return url is null
            ? Results.NotFound("Url not found.")
            : Results.Ok(new
            {
                url.ShortenedUrl,
                url.OriginalUrl,
                url.ClickCount,
                url.ExpirationDate,
                QrCodeUrl =
                      $"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={url.ShortenedUrl}"
            });
    }

    private static IResult DeleteShortUrl(string code, ApplicationDbContext db)
    {
        var removed = db.Urls.DeleteMany(u => u.Code == code);
        db.Clicks.DeleteMany(c => c.ShortUrlId == Guid.Empty);
        return removed == 0
            ? Results.NotFound("Url not found.")
            : Results.Ok("Url has been deleted.");
    }
}
