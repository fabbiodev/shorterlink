using System.ComponentModel.DataAnnotations;

namespace ShorterURL.API.Models;

public class UrlRequest
{
    [Url]
    public string Url { get; set; }

    [StringLength(20, MinimumLength = 4)]
    public string? CustomCode { get; set; }

    public int? ExpirationDurationInDays { get; set; }
}
