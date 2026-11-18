### 1. What did AI generate for you, and what did you write or modify yourself?

I used AI (Claude) to handle the repetitive boilerplate, while I drove the architecture, database indexing, frontend tooling decisions, and structural refactoring.

**What AI generated (the fast grunt work):**
- Initial boilerplate for the Domain entities, `GenericRepository`, and `SpecificationEvaluator`.
- The idempotent JSON seed data files (artists, tracks, DSPs, and initial distributions).
- Initial Angular standalone component HTML/Tailwind templates and FluentValidation class scaffolding once I defined the rules.

**What I designed, enforced, and fixed myself (the engineering):**
1. **Core Architecture from Scratch:**
   - Enforced strict Clean Architecture boundaries: kept `Takwene.Domain` completely free of external packages, isolated `ApplicationUser` inside `Takwene.Infrastructure`, and linked it to the `Artist` entity using a plain `string UserId` and `IAuthService`.
   - Implemented the `Result<T>` pattern using C# `implicit operator` conversions so services can return an `Error` or a value cleanly without throwing exceptions for control flow.
   - Added an EF Core `AuditableEntitySaveChangesInterceptor` to handle `CreatedAt` and `UpdatedAt` automatically.
2. **Enforcing SRP & Strict File Structure:**
   - In its first iteration, the AI crammed multiple DTOs, validators, specifications, and error catalogs into single files and mixed track CRUD with DSP distribution inside one giant `TrackService`.
   - I stopped it and forced a complete restructure: strictly one type per file, all interfaces grouped under `/Interfaces`, and `TrackService` split into `TrackService` (track lifecycle) and `TrackDistributionService` (DSP delivery and status updates).
3. **Replacing `.Contains()` Search with PostgreSQL Trigram Indexes:**
   - I rejected the standard LINQ `.Contains()` search because it causes full table scans, and directed the implementation of PostgreSQL `pg_trgm` GIN indexes with `EF.Functions.ILike`.
4. **Frontend Tooling, NSwag Split & Missing Artist Workflow:**
   - I chose the Angular + PrimeNG + Tailwind CSS + NSwag stack and configured NSwag (`MultipleClientsFromFirstTagAndOperationId`) to generate separate injectable clients per controller (`AuthClient`, `TracksClient`, `ArtistsClient`, `DspsClient`) instead of one monolithic client class.
   - I also caught that PrimeNG v22 dropped the MIT license and displays a license warning without a key, so I directed the setup to use Angular 21 + PrimeNG 21 (the last MIT release).
   - **Catching Missing Features & Routes:** The AI completely forgot to build the Admin Artist Management panel (creating and viewing artists) and didn't even add it to `app.routes.ts`. I caught this gap, had it build the Artist panel and creation dialog, wired up the routing, and displayed the generated one-time password so reviewers can immediately log in as the new artist. (I also intentionally skipped building a "Change Temporary Password" screen for artists since password reset flows were outside the assessment scope).
5. **Fixing .NET 10 Tooling, Migrations & Swagger Startup:**
   - Resolved NuGet `NU1202` errors when `Install-Package Microsoft.EntityFrameworkCore.Design` pulled `v11.0.0` (which requires `net11.0`) by pinning `v10.0.0` to match `net10.0`.
   - Configured `Program.cs` and `launchSettings.json` (`"launchUrl": ""`) so Swagger UI opens automatically at the root URL (`/`) in both Development and Production environments.

---

### 2. What security issues did you find (or introduce) in the AI-generated code? How did you handle them?

1. **Ownership Scoping ("Null Filter" Bypass) & IDOR (`403` vs `404`):**
   - **The Issue:** In filtered queries (`GET /api/tracks?artistId=`), a `null` `artistId` normally means "return all tracks." If a user with the `Artist` role had a missing `artist_id` claim, falling back to `null` would expose every artist's tracks. Additionally, fetching a track by ID first and checking ownership in memory to return `403 Forbidden` leaks that the track ID exists (IDOR / resource enumeration).
   - **How I Handled It:** Created a centralized `ArtistScope` helper that blocks any non-Admin request lacking a valid `artist_id` claim, and pushed the ownership check directly into the SQL `WHERE` clause (`t.Id == id && (isAdmin || t.ArtistId == artistId)`). Querying another artist's track now returns `404 Not Found`.

2. **Public `/auth/register` Endpoint & Static Default Passwords for Artists:**
   - **The Issue:** The initial auth code exposed an open `/api/auth/register` endpoint and assigned a hardcoded default password whenever an Admin created an Artist.
   - **How I Handled It:** I removed `/auth/register` since artist onboarding is strictly admin-controlled, and updated `POST /api/artists` to generate a random one-time password returned once in `ArtistCreatedResponse`.
   - **Deliberate Scope Trade-off:** I intentionally did not implement a "change temporary password on first login" flow for Artists. While necessary in production, it was not requested in the prompt, so I kept the scope focused on the core track and distribution workflows.

3. **Plaintext Refresh Tokens in the Database:**
   - **The Issue:** Storing raw refresh tokens in PostgreSQL means a database leak exposes active user sessions.
   - **How I Handled It:** Refresh tokens are hashed with SHA-256 before storage, combined with token rotation and reuse detection—if a previously revoked token is used again, all refresh tokens for that user are immediately revoked.

4. **Orphaned Identity Users on Partial Failure:**
   - **The Issue:** When creating an Artist, `UserManager.CreateAsync` saves the `ApplicationUser` immediately before `UnitOfWork.CompleteAsync()` saves the `Artist` row. If the second save failed, an orphaned user account remained in `AspNetUsers`.
   - **How I Handled It:** Wrapped both operations inside `UnitOfWork.ExecuteInTransactionAsync` so Identity and domain persistence commit or roll back together.

5. **NuGet Transitive Vulnerability (`System.Security.Cryptography.Pkcs` - `NU1902` / `NU1903`):**
   - **The Issue:** Running `Update-Database` in the Package Manager Console flagged moderate and high DoS vulnerabilities in `System.Security.Cryptography.Pkcs` (`9.0.0`).
   - **How I Handled It:** I first tried upgrading `System.Security.Cryptography.Pkcs` explicitly to `10.0.0` in `Takwene.API`, but NuGet Audit still flagged `10.0.0` with the same advisories. I traced the dependency tree and confirmed it is only pulled in by `Microsoft.EntityFrameworkCore.Tools` for design-time migrations and is never exposed at runtime. I documented this and suppressed `NU1902;NU1903` via `<NoWarn>` in the `.csproj` to keep the build output clean.

---

### 3. One thing the AI got wrong that you had to fix. What was it and why was it wrong?

Here are the three major technical mistakes the AI made during development that I had to step in and fix:

#### A. Implementing Smart Search with `.ToLower().Contains(term)` on PostgreSQL
- **What it did wrong:** For the 3-character smart search on `GET /api/tracks`, the AI initially wrote `.ToLower().Contains(term.ToLower())` inside the specification while creating PostgreSQL `pg_trgm` GIN indexes on `Title` and `Isrc`.
- **Why it was wrong:**
  1. In Npgsql, `.ToLower().Contains()` translates to `lower(column) LIKE '%term%'`. Wrapping the column in `lower()` prevents PostgreSQL from using a plain `gin_trgm_ops` index, forcing a slow **Full Table Scan**.
  2. It didn't escape `%`, `_`, or `\` characters in user input, allowing wildcard injection.
  3. Using `EF.Functions.ILike` directly in the Application layer would have leaked EF Core/Npgsql dependencies into `Takwene.Application`.
- **How I fixed it:** I added a pure C# `AddSearch` method to `BaseSpecification` in `Takwene.Domain` and handled the translation inside `SpecificationEvaluator` in `Takwene.Infrastructure`—escaping wildcards and building the expression with `EF.Functions.ILike` so PostgreSQL executes `column ILIKE '%term%' ESCAPE '\'` directly against the GIN trigram index.

#### B. Breaking Single Responsibility (SRP) & Cramming Multiple Types into Single Files
- **What it did wrong:** The AI initially grouped DTOs, their FluentValidation classes, and error definitions inside the same `.cs` files, scattered interfaces across feature folders, and combined Track CRUD operations with DSP distribution logic inside a single `TrackService`.
- **Why it was wrong:** It violated SRP, made navigation and maintenance painful, and tightly coupled track metadata management with external DSP delivery rules.
- **How I fixed it:** I enforced a strict one-type-per-file architecture across all 80+ files in `Domain` and `Application`, centralized contracts in `/Interfaces`, extracted shared ownership rules into `ArtistScope`, and split `TrackService` into `TrackService` and `TrackDistributionService`.

#### C. Breaking NSwag Client Generation with .NET 10 OpenAPI Defaults & Forgetting the Artist UI Route
- **What it did wrong:**
  1. On the backend, controller actions returned untyped `IActionResult`, had no `[EndpointName]`, and only configured `JsonStringEnumConverter` on MVC options. As a result, `.NET 10`'s `/openapi/v1.json` had no DTO response schemas, no `operationId`s (causing NSwag to invent ugly method names), and exported Enums as integers instead of strings.
  2. On the frontend, the AI completely forgot to build the Admin Artist Management view and omitted it from `app.routes.ts`.
- **How I fixed it:**
  1. Changed controller actions to return `ActionResult<T>` with explicit `[EndpointName]` attributes and added `ConfigureHttpJsonOptions` with `JsonStringEnumConverter` so NSwag generated strongly-typed interfaces, string literal enums (`'Draft' | 'Submitted' | 'Distributed'`), and clean per-controller clients.
  2. Added the missing Artists page, routing, and Create Artist dialog so Admins can create artists and copy their one-time password directly from the UI.