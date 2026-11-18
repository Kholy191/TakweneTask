# Takwene — Music Distribution & Catalog Management Platform

Takwene lets record-label admins and artists manage a music catalog and distribute tracks to Digital Service
Providers (DSPs) such as Spotify, Anghami and Apple Music. Admins onboard artists and approve deliveries; artists
manage only their own tracks. Every request goes through role and permission checks and ownership rules.

---

## Table of Contents

1. [Tech Stack & Architecture](#tech-stack--architecture)
2. [Getting Started](#getting-started)
3. [Database Migrations](#database-migrations)
4. [How to Obtain a JWT Token](#how-to-obtain-a-jwt-token)
5. [Key API Endpoints](#key-api-endpoints)

---

## Tech Stack & Architecture

### Backend — .NET 10 (Clean Architecture)

| Layer | Project | Responsibility |
| :--- | :--- | :--- |
| **Domain** | `Takwene.Domain` | Entities, enums, business rules (status transitions, ISRC), repository and specification contracts |
| **Application** | `Takwene.Application` | Services, DTOs, FluentValidation validators, specifications, Result pattern, permissions |
| **Infrastructure** | `Takwene.Infrastructure` | EF Core + PostgreSQL, ASP.NET Core Identity, JWT/refresh tokens, repositories, seeding |
| **API** | `Takwene` (`Takwene.API.csproj`) | Thin controllers, permission policies, ProblemDetails error handling, OpenAPI + Swagger UI |

- **PostgreSQL 17** with `pg_trgm` GIN indexes for fast, case-insensitive partial-match search.
- **Authentication:** ASP.NET Core Identity, JWT access tokens and rotating refresh tokens with reuse detection.
- **Authorization:** permission-based policies (`[HasPermission]`) plus fail-closed artist ownership checks.
- **Logging:** Serilog, sent to the console and to **Seq**.
- **Patterns:** Repository + Unit of Work, Specification (with projection and search), Result, Guid v7 keys.

### Frontend — Angular 21

- **Angular 21:** standalone components, signals, zoneless change detection.
- **PrimeNG 21** with the Aura theme and **PrimeIcons**.
- **Tailwind CSS 4**, using CSS layers so it works alongside PrimeNG.
- **NSwag:** typed API clients generated from the OpenAPI document, split per controller
  (`AuthClient`, `ArtistsClient`, `TracksClient`, `DspsClient`).
- **HTTP interceptors** attach the JWT, refresh it silently and map errors to ProblemDetails. Route guards cover
  authenticated, guest-only and admin-only pages.

### Repositories

```
repos/
├── Takwene/            # Backend (.NET solution, Dockerfile, docker-compose.yml)
└── TakweneFrontEnd/    # Frontend (Angular app, Dockerfile, nginx.conf)
```

---

## Getting Started

### Prerequisites

| Option | Requirements |
| :--- | :--- |
| **A — Docker** | Docker Desktop (Compose v2) |
| **B — Local** | Docker (for PostgreSQL and Seq), .NET 10 SDK, Node.js 22+, npm 11+ |

### Option A — Docker (one command, full stack)

Clone both repositories into the same parent folder. Then, from the backend folder, run:

```bash
cd Takwene
docker compose up --build -d
```

This builds and starts four containers:

| Service | Description | URL |
| :--- | :--- | :--- |
| `web` | Angular app served by **Nginx**, which reverse-proxies `/api` and `/openapi` to the API | http://localhost:8080 |
| `api` | .NET 10 API (plain HTTP on 8080 inside the network) + Swagger UI | http://localhost:5243 |
| `postgres` | PostgreSQL 17 (persistent volume `takwene-pgdata`) | `localhost:5432` |
| `seq` | Structured log viewer (ingestion on port 5341) | http://localhost:8081 |

> The browser only talks to Nginx, so this setup needs no CORS or HTTPS certificates.
> If the frontend lives somewhere else, set `FRONTEND_PATH`, for example
> `FRONTEND_PATH=../path/to/TakweneFrontEnd docker compose up --build -d`.

Useful commands:

```bash
docker compose logs -f api     # follow API logs
docker compose down            # stop the stack
docker compose down -v         # stop and reset the database (re-seeds on next start)
```

### Option B — Local development

**1. Start the databases with Docker**

```bash
cd Takwene
docker compose up -d postgres seq
```

**2. Run the backend**

```bash
dotnet run --project Takwene --launch-profile https
```

The API listens on https://localhost:7000 and http://localhost:5243. Swagger UI is served at the root URL.

**3. Run the frontend**

```bash
cd ../TakweneFrontEnd
npm ci
npm start
```

Open http://localhost:4200. In development the app calls `https://localhost:7000` directly; the API allows this
origin through `Cors:AllowedOrigins`.

> After any API contract change, regenerate the typed clients with `npm run generate:api`.

---

## Database Migrations

> **The API applies pending migrations and seeds data automatically on startup.** Running the commands below by
> hand is optional unless you change the model.

### .NET CLI

Install the tool once with `dotnet tool install --global dotnet-ef`. Then run these from the `Takwene` solution folder:

```bash
# Create a migration
dotnet ef migrations add InitialCreate -p Takwene.Infrastructure -s Takwene -o Persistence/Migrations

# Apply migrations
dotnet ef database update -p Takwene.Infrastructure -s Takwene
```

### Visual Studio — Package Manager Console

Set **Default project** to `Takwene.Infrastructure` and the startup project to `Takwene.API`. Then run:

```powershell
# Create a migration
Add-Migration InitialCreate -OutputDir Persistence/Migrations

# Apply migrations
Update-Database
```

### Seed data

On first startup the app seeds roles and permissions, users, artists, DSPs, tracks and distributions from
`Takwene.Infrastructure/Persistence/Seeding/Data/*.json`.

---

## How to Obtain a JWT Token

### Seeded roles

| Role | Capabilities |
| :--- | :--- |
| **Admin** | Full access: create and list artists, manage all tracks, approve or reject DSP deliveries |
| **Artist** | Create, view, distribute and update the status of **their own** tracks only |

### Seeded accounts (test credentials)

| Role | Email | Password |
| :--- | :--- | :--- |
| Admin | `admin@takwene.com` | `Admin@12345` |
| Artist | `amr@takwene.com` | `Artist@12345` |
| Artist | `cairokee@takwene.com` | `Artist@12345` |
| Artist | `wegz@takwene.com` | `Artist@12345` |

### Log in

```bash
curl -X POST http://localhost:5243/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{ "email": "admin@takwene.com", "password": "Admin@12345" }'
```

Response:

```json
{
  "userId": "10000000-0000-0000-0000-000000000001",
  "email": "admin@takwene.com",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "accessTokenExpiresAt": "2026-11-18T16:15:00Z",
  "refreshToken": "q8V3...",
  "refreshTokenExpiresAt": "2026-11-25T16:00:00Z"
}
```

Send the access token on protected requests:

```bash
curl http://localhost:5243/api/tracks -H "Authorization: Bearer <accessToken>"
```

In Swagger UI, click **Authorize** and paste the access token.

### New artists and one-time passwords

When an **Admin** creates an artist (in the **Artists** page of the UI or with `POST /api/artists`), the API also
creates the artist's login. If no password is supplied, it generates a secure one-time password and returns it
**once** in the `temporaryPassword` field. The UI shows it with a copy button. Pass the email and temporary password
to the artist so they can log in for the first time.

---

## Key API Endpoints

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Public | Authenticate and receive JWT + Refresh Token |
| `POST` | `/api/auth/refresh` | Public | Rotate refresh token and obtain a new JWT |
| `POST` | `/api/auth/logout` | Authenticated | Revoke the current refresh token |
| `GET` | `/api/auth/me` | Authenticated | Return current user profile, roles, permissions, and `artistId` |
| `POST` | `/api/artists` | Admin | Create a new artist & provision Identity credentials |
| `GET` | `/api/artists` | Admin | List all artists (paginated) |
| `POST` | `/api/tracks` | Admin / Artist | Create a new track in `Draft` status (validates unique ISRC) |
| `GET` | `/api/tracks` | Admin / Artist | List tracks with status filter, artist filter, and 3+ char `pg_trgm` smart search |
| `GET` | `/api/tracks/{id}` | Admin / Owner | Get track details including DSP distribution history |
| `POST` | `/api/tracks/{id}/distribute` | Admin / Owner | Distribute a track to selected DSPs (`Pending`); moves `Draft` → `Submitted` |
| `PATCH` | `/api/tracks/{id}/status` | Admin / Owner | Transition track status (`Submitted` → `Distributed` or `Submitted` → `Draft`) |
| `PATCH` | `/api/tracks/{id}/distributions/{distributionId}/status` | Admin | Update DSP delivery status (`Pending` → `Live` or `Rejected`) |
| `GET` | `/api/dsps` | Authenticated | List all available Digital Service Providers |

> `Submitted` → `Distributed` requires at least one `Live` distribution. `Submitted` → `Draft` is allowed only while
> no distribution is `Live`. All errors are returned as RFC 7807 **ProblemDetails**.