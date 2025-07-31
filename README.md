# Shorterlink – Minimal URL Shortener in .NET 8

A LiteDB-powered link shortener with basic analytics – no authorization, no external DBMS, no heavy dependencies.

---

## Features

* **Create short links** — `POST /api/shorten`
* **Instant redirect** — `GET /{code}`
* **Click analytics**  
  * total / unique clicks  
  * country breakdown  
  * day-by-day chart — `GET /api/{code}/analytics`
* **QR-code & details** — `GET /api/{code}/details`
* **Delete a link** — `DELETE /api/{code}`
* Data stored in a single `shorturls.db` file (LiteDB)

---

## Quick start

```bash
git clone https://github.com/your-user/ShorterURL.git
cd ShorterURL
dotnet run -p ShorterURL.API
