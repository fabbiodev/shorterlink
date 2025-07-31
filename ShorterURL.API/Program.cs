using ShorterURL.API.Data;
using ShorterURL.API.Endpoints;
using ShorterURL.API.Services;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(AppContext.BaseDirectory, "shorturls.db");
builder.Services.AddSingleton(new ApplicationDbContext(dbPath));
builder.Services.AddSingleton<UrlShortenerService>();

builder.Services.AddHttpClient<GeoService>();
builder.Services.AddSingleton<GeoService>();

builder.Services.AddCors(o =>
    o.AddPolicy("allowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();  // index.html
app.UseStaticFiles();   // analytics.html и пр.

app.UseHttpsRedirection();
app.UseCors("allowAll");

app.MapShortUrlEndpoints();

app.MapGet("/analytics", () => Results.Redirect("/analytics.html"));

app.Run();
