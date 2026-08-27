# BlogApi — Development Log

**Date**: 2026-08-14
**Objective**: Build personal blog API for my own knowledge base / personal logs
**Deliverable**: A personal blog site backed by ASP.NET Core 10 + EF Core + SQLite, with a Vite + React + TypeScript frontend

---

## 1. Issues Encountered

### Environment / Versioning
- **Misidentified the LTS version**: Started on .NET 8 assuming it was the current LTS. It was actually scheduled for end-of-support in November 2026, with .NET 10 being the current LTS (supported through 2028). Confirmed via search, then reinstalled the SDK and rebuilt the project on .NET 10.
- **NuGet package/target framework mismatch**: `dotnet add package` pulled the latest package version (net10.0-only) into a project still targeting net8.0, producing an `NU1202` error.
- **Swagger/OpenAPI tooling migration**: Starting with .NET 9, the default template ships `Microsoft.AspNetCore.OpenApi` instead of `Swashbuckle`, so `/swagger` no longer renders a UI out of the box. Had to install `Swashbuckle.AspNetCore` separately — and its v10 release depends on `Microsoft.OpenApi` v2, which changed `AddSecurityRequirement` to a delegate-based API, breaking the commonly referenced example code for wiring up API key auth in Swagger UI.

### Code-Level Mistakes
- **Missing route attribute parameters**: Used `[HttpGet]` without a route template (`[HttpGet("{id}")]`) on a single-item lookup, which collided with the list endpoint on the same route (`Swashbuckle.AspNetCore.SwaggerGen.SwaggerGeneratorException`).
- **Missing HTTP verb attribute entirely**: A `DeletePost` method had no `[HttpDelete]` attribute at all. Compiled fine, but silently never got mapped to a route — a class of bug that has no compile-time signal in C#.
- **Missing `.Include()`**: The list endpoint didn't eager-load the `Author` navigation property, causing a `NullReferenceException` inside the entity-to-DTO mapping function.
- **Return type / actual object mismatch**: A method was declared as `ActionResult<AuthorResponse>` but internally constructed and returned a `CreateAuthorResponse` (which includes the API key). Because ASP.NET Core serializes according to the declared return type, the extra field was silently dropped from the response.
- **Circular reference in entity serialization**: Serializing EF Core entities directly (before DTOs were introduced) produced `Post.Author.Posts.Author.Posts...` cycles and threw `System.Text.Json.JsonException`. Recurred multiple times until entities were fully replaced with DTOs at the controller boundary.
- **415 Unsupported Media Type from a typo**: A malformed `Content-Type` header value in a frontend `fetch` call caused the server to reject a well-formed JSON body.

### Infrastructure
- **Port conflicts**: A previous `dotnet run` process didn't fully terminate between sessions, repeatedly producing `Address already in use`. Resolved with `lsof -ti :PORT | xargs kill -9` / `pkill -9 -f dotnet`.
- **SQLite UNIQUE constraint violations**: A unique index on `Post.Slug` correctly rejected duplicate-slug test requests (`SqliteException`), surfacing during manual Swagger testing.

---

## 2. What Was Learned

### EF Core / Data Modeling
- Modeling one-to-many (Author–Post) and many-to-many (Post–Tag) relationships via navigation properties on the entity classes.
- EF Core's explicit eager-loading model (`.Include()` / `.ThenInclude()`) — conceptually similar to Prisma's `include`, but with stricter default behavior (no automatic loading of related data without an explicit call).
- Migration-driven schema evolution (`dotnet ef migrations add`, `dotnet ef database update`) as the standard workflow for schema changes.

### DTO Pattern
- Directly exposing EF Core entities in API responses causes concrete, reproducible problems: circular reference exceptions, accidental exposure of sensitive fields, and clients being able to set fields (e.g. `Id`, `CreatedAt`) that should only ever be server-controlled.
- Ended up with a request/response DTO split per resource, plus a special case: a one-time-only response DTO (`CreateAuthorResponse`) that includes the API key exclusively on the creation response — mirroring how services like GitHub reveal a personal access token only once, at creation time.
- Recognized this as conceptually identical to FastAPI's Pydantic schema separation (`PostCreate` / `PostResponse`), just expressed through plain C# classes instead of a validation library.

### Authentication
- Implemented a custom authentication scheme (API key) by subclassing `AuthenticationHandler<TOptions>`, registered through the standard ASP.NET Core authentication pipeline (`AddAuthentication().AddScheme<...>()`) rather than as ad hoc middleware.
- Learned the `Claims` / `ClaimsIdentity` / `ClaimsPrincipal` model — the framework's standard representation of "who is making this request." Designed so that a future switch to JWT would only change how claims are populated (token parsing vs. DB lookup), leaving `[Authorize]` usage in controllers untouched.
- Used `RandomNumberGenerator` (not `System.Random`) for cryptographically secure API key generation, with server-side generation rather than client-supplied keys.

### Frontend Integration
- Why CORS must be configured explicitly server-side, and the required middleware ordering (`UseCors` → `UseAuthentication` → `UseAuthorization`).
- Refactored a form with eight separate `useState` calls into `react-hook-form`, removing manual state wiring and gaining built-in `isSubmitting` / validation state.
- Used React Context to share session-scoped auth state (the API key) across the post list, detail, and form components without prop drilling.

### Debugging Habits
- Identifying the topmost exception type/message and the exact request (verb + URL) that triggered it, before reading further into the stack trace, consistently cut diagnosis time the most.
- In a statically typed language like C#, the *declared* return type directly affects runtime serialization behavior — a class of bug with no equivalent in Python/JS, where the object you construct is generally what gets returned as-is.

---

## 3. Areas for Improvement

- **No service layer**: Entity-to-DTO mapping logic (`ToResponse`) is duplicated across `PostsController` and `TagsController`. Extracting this into a `/Services` layer would reduce duplication and improve testability.
- **API keys stored in plaintext**: Currently stored as-is in the database. Production-grade practice would hash keys (e.g. SHA-256) at rest and compare hashes on lookup.
- **Undefined cascade behavior on Author deletion**: What happens to a Post when its Author is deleted (cascade delete, restrict, or null out the FK) hasn't been deliberately decided or tested yet.
- **Unauthenticated key-regeneration endpoint**: Added to recover from a lost API key, but currently has no auth guard — anyone who knows an Author's ID can reset their key. Needs a secondary verification step (e.g. email confirmation) before this is safe to expose beyond local development.
- **No PATCH support**: Only PUT (full replacement) is implemented, requiring clients to resend every field for a partial update.
- **No automated tests**: No unit or integration test coverage, making refactors risk regressions silently. xUnit is the natural next addition.
- **Environment-specific configuration incomplete**: CORS origins and connection strings are still hardcoded rather than split across `appsettings.{Environment}.json`.
- **No pagination**: `GET /api/posts` returns the full result set, which won't scale as post count grows.

---

## 4. Notable Characteristics of ASP.NET Core (vs. FastAPI / Express)

Observations from working in ASP.NET Core immediately after FastAPI (Python) and Express (TypeScript).

| Aspect | FastAPI / Express | ASP.NET Core |
|---|---|---|
| Type system | Dynamic (FastAPI mitigates via Pydantic) | Static, enforced at compile time |
| Routing | Function/decorator-based, low ceremony | Attribute-based (`[HttpGet]`); explicit, but a missing or incomplete attribute fails silently (no route mapped, no compiler warning) |
| Dependency injection | Requires a library, or manual wiring (FastAPI's `Depends` is a partial exception) | Built into the framework; resolved automatically via controller constructors |
| ORM | Prisma/SQLAlchemy — broadly similar explicit-include model | EF Core — relationship loading is explicit via `.Include()`/`.ThenInclude()`; migration tooling is tightly integrated with the framework and CLI |
| Auth extensibility | Middleware assembled by hand | Formal pipeline via `AuthenticationHandler` subclassing; swapping schemes (e.g. API key → JWT) doesn't require touching controller code |
| When errors surface | Usually immediately, at runtime | Compiles cleanly but fails at routing or serialization time for certain mistakes (missing attributes, return-type/object mismatches) |
| Pace of ecosystem change | Comparatively gradual | Default template and package composition shift noticeably between major versions (8 → 9 → 10), e.g. the OpenAPI tooling change — meaning tutorials and Stack Overflow answers go stale faster than expected |
| Convention strength | Loose, high freedom | Controller-Service-Repository is close to a de facto standard, which reduces bikeshedding in a team context |

**Summary**: The expectation going in was that static typing plus strong framework conventions would eliminate most "didn't find out until I ran it" mistakes. In practice, a different class of silent failure took their place — missing attributes and return-type mismatches that the compiler doesn't flag, because they're structurally valid C# even though they're functionally wrong. Once recognized, though, these follow a small number of recognizable patterns and are fast to check for. On balance, the areas where ASP.NET Core has an opinionated "correct" answer (DI, auth pipeline, relationship loading) narrowed the design space in a way that felt more prescriptive — and in this context, easier to reason about — than the equivalent decisions in FastAPI or Express.

---

## Appendix: Feature Scope Delivered

**Backend**
- `Post`, `Tag` (many-to-many), `Author` (one-to-many) entities and relationships
- Full CRUD for Posts and Authors, entirely DTO-mediated
- Tag listing and tag-scoped post queries
- Custom API key authentication, CORS configuration

**Frontend**
- Vite + React + TypeScript
- React Router (list, detail, create, edit views)
- React Hook Form
- Context-based auth state management
- Custom design system
