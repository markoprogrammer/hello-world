# hello-world

ASP.NET Core Web API with OAuth2 authentication (JWT Bearer tokens).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Quick Start

```bash
cd src
dotnet restore
dotnet run
```

The API will start at `https://localhost:5001` (or `http://localhost:5000`).
Swagger UI is available at `/swagger`.

## OAuth2 Endpoints

### Get Token (Password Grant)

```bash
curl -X POST http://localhost:5000/oauth/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=admin123"
```

### Get Token (Client Credentials Grant)

```bash
curl -X POST http://localhost:5000/oauth/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=my-client&client_secret=my-secret"
```

### Refresh Token

```bash
curl -X POST http://localhost:5000/oauth/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token&refresh_token=YOUR_REFRESH_TOKEN"
```

## Protected API Endpoints

All require `Authorization: Bearer <token>` header.

| Endpoint | Role Required | Description |
|---|---|---|
| `GET /api/weather/forecast` | Any authenticated | Weather forecast |
| `GET /api/weather/admin-stats` | admin | Admin statistics |
| `GET /api/weather/me` | Any authenticated | Current user info |

## Default Users

| Username | Password | Roles |
|---|---|---|
| admin | admin123 | admin, user |
| user | user123 | user |

## Default OAuth2 Client

| Client ID | Client Secret | Scopes |
|---|---|---|
| my-client | my-secret | api.read |
