# AutoFinderAI — Repository Instructions

## 1. Product
Local-only fullstack app for a recruitment assignment (48h limit, ~24h of work left).
Flow: crawl fresh car listings from **otomoto.pl** → store in **SQLite** → user describes needs in
natural language in a **chat UI** → an **LLM converts the text into a structured filter/criteria JSON**
→ **backend (EF Core) filters + ranks** the local dataset → results are returned as a short markdown
intro plus a structured result list rendered as cards/table.

**The LLM never queries the database, never invents results, never contains business rules.**
It only: (a) extracts structured search criteria, (b) asks for clarification, (c) produces short prose.
Filtering, ranking, scoring, limits and explanations are deterministic C# code.

## 2. Non-negotiable stack
| Area | Choice |
|---|---|
| Backend | .NET 10, C# 13, nullable enabled, `TreatWarningsAsErrors=false` (warnings visible, not blocking) |
| Patterns | Clean Architecture + CQRS via **MediatR**, **FluentValidation** |
| Data | **EF Core 9 + SQLite**, code-first, **EF migrations committed** |
| Auth | JWT bearer, own `User` entity, `Microsoft.AspNetCore.Identity.PasswordHasher<User>` for hashing (no full Identity stack) |
| HTML parsing | **AngleSharp** |
| Resilience | `Microsoft.Extensions.Http.Resilience` (or Polly) on outbound HTTP |
| Logging | **Serilog** (console + rolling file), request logging, correlation id |
| API docs | Swagger / OpenAPI (Swashbuckle), enabled in Development |
| LLM gateway | **OpenRouter** REST via typed `HttpClient`; model list **configurable**; key from `OPENROUTER_API_KEY` env var only |
| Frontend | **Angular 20**, standalone components, **signals**, Angular Material, SCSS, strict TS |
| Markdown | `ngx-markdown` (or `marked` + sanitizer) |
| Tests | xUnit, FluentAssertions, NSubstitute, `WebApplicationFactory` for integration |
| Containers | Dockerfile (API multi-stage), Dockerfile (Angular → nginx), `docker-compose.yml` |

**Do not introduce any other package** without an explicit human "approved". No AutoMapper,
no generic repository, no Semantic Kernel/LangChain, no vector DB, no Redis, no MassTransit,
no Hangfire, no NgRx, no Tailwind, no MudBlazor.

## 3. Solution layout (authoritative)
```
/AutoFinderAI.sln
/src/AutoFinderAI.Domain            → entities, enums, value objects, domain rules. ZERO dependencies.
/src/AutoFinderAI.Application       → CQRS handlers, DTOs, validators, abstractions (interfaces), ranking. Depends: Domain + MediatR + FluentValidation only.
/src/AutoFinderAI.Infrastructure    → EF Core, DbContext, migrations, repositories/queries, OpenRouter client, crawler adapters, JWT issuer, password hasher. Depends: Application + Domain.
/src/AutoFinderAI.Api              → classic, non-minimal-API endpoints in Controllers, DI composition, middleware, Serilog, Swagger. Depends: Application + Infrastructure.
/tests/AutoFinderAI.UnitTests
/tests/AutoFinderAI.IntegrationTests
/frontend                         → Angular workspace
/docs                             → ARCHITECTURE.md, DECISIONS.md, AI_WORKFLOW.md
/docker-compose.yml, /README.md, /.env.example
```
**Dependency rule:** `Api → Infrastructure → Application → Domain`. Never reversed.
`DbContext`, `HttpClient`, EF types, ASP.NET types must not appear in `Application` or `Domain`.
`Application` defines interfaces; `Infrastructure` implements them.

## 4. Domain model (locked)
Inheritance is intentional for future vehicle categories, mapped **TPH** with a
`VehicleCategory` discriminator.

```
abstract Vehicle (aggregate root)
  Id (Guid), SourceKey (string), ExternalId (string), Url, Title,
  Price (Money owned: Amount decimal, Currency string),
  Make, Model, Version?, ProductionYear (int), Mileage (int?),
  FuelType (enum), Transmission (enum), EnginePowerHp (int?), EngineCapacityCm3 (int?),
  Location (string?), ThumbnailUrl (string?),
  PublishedAt (DateTimeOffset), ScrapedAt (DateTimeOffset),
  Category (VehicleCategory, discriminator)

Car : Vehicle
  BodyType (enum), Doors (int?), Seats (int?), DriveType (enum?), Color (string?),
  IsDamaged (bool?), IsFirstOwner (bool?), CountryOfOrigin (string?)
```
Enums stored as **strings**. Unique index `(SourceKey, ExternalId)` → upsert/dedupe key.
Also: `CrawlRun` (Id, SourceKey, Category, StartedAt, FinishedAt?, Status, ItemsFound, ItemsSaved, Error?),
`User` (Id, Email unique, PasswordHash, CreatedAt),
`ChatSession` (Id, UserId, Title, CreatedAt, LastMessageAt),
`ChatMessage` (Id, SessionId, Role enum{User,Assistant}, Content markdown, CriteriaJson?, ResultVehicleIdsJson?, ModelUsed?, CreatedAt).

Extensibility contract: adding `Motorcycle : Vehicle` must require **only** a new entity +
enum value + a source adapter — no changes to search, ranking, API or UI shape.

## 5. Backend conventions
- Feature folders: `Application/Features/<Area>/<UseCase>/{Command|Query}.cs, Handler.cs, Validator.cs, Result.cs`.
- One handler per use case. Handlers are thin; logic lives in domain or dedicated services.
- Queries use `AsNoTracking()`, project to DTOs, never return entities from the API.
- Result pattern: `Result<T>` (or `ErrorOr`-style own minimal type) for expected failures;
  exceptions only for unexpected faults.
- Global exception middleware → RFC7807 `ProblemDetails`. Validation failures → 400 with field errors.
- `CancellationToken` propagated everywhere. `async` all the way down. No `.Result`/`.Wait()`.
- Never log secrets, tokens, passwords, or full prompts containing user PII.

## 6. Frontend conventions
- Standalone components only, `ChangeDetectionStrategy.OnPush`, signals for state,
  `resource`/`httpResource` or service + signals for async. No NgModules, no `any`, no manual
  `subscribe` in components (use `toSignal`/`async` where needed).
- Feature folders: `src/app/features/{auth,chat}`, `src/app/core/{auth,http,api}`, `src/app/shared`.
- Typed API models in `core/api/models.ts` mirroring backend DTOs exactly.
- JWT in `localStorage`, attached by an `HttpInterceptorFn`; 401 → clear + redirect to login.
- Every async view has explicit **loading / empty / error** states.
- Angular Material theming, mobile-first responsive, sensible spacing. Clean and elegant > flashy.

## 7. Configuration & secrets
- All config through `appsettings.json` + env vars. `.env.example` committed, `.env` git-ignored.
- Keys: `OPENROUTER_API_KEY`, `Ai__DefaultModel`, `Jwt__Key`, `Jwt__Issuer`, `ConnectionStrings__Default`.
- `Ai:Models` is a **configurable list** of allowed OpenRouter model ids surfaced to the UI.
- **No API key present →** app starts normally, `/api/ai/status` reports `available:false`,
  chat returns a clear degraded-mode message and falls back to a *narrow* keyword search
  (Title + Make + Model only, never a wide multi-column LIKE).

## 8. Testing policy
Test what carries risk: HTML parsing (against saved fixture files), criteria→SQL filter
translation, ranking/scoring, LLM JSON parsing incl. malformed/out-of-range output,
auth handlers, validators. Integration tests: register→login→create session→send message
(with a fake LLM client) and a crawl-persist round trip. No coverage targets, no E2E suite.

## 9. Git & documentation
- Small commits, conventional prefixes (`feat:`, `fix:`, `chore:`, `test:`, `docs:`).
- Add trailer `AI-Assisted: <agent-name>/<model>` when a commit was largely agent-authored.
- Documentation lives in exactly 4 files: `README.md`, `docs/ARCHITECTURE.md`,
  `docs/DECISIONS.md` (one-line decision table), `docs/AI_WORKFLOW.md`. No per-decision ADR files.

## 10. Scope boundary (do not build)
Charts, admin panel, refresh tokens, roles/permissions, email confirmation, embeddings/semantic
search, streaming SSE, OpenTelemetry, rate limiting, background scheduler, Kubernetes,
multi-tenancy, i18n, dark/light toggle, PWA, generic plugin loader.
Anything outside sections 1–9 requires a human "approved".

## 11. Working agreement for all agents
- **Ship working code.** Do not write essays, do not restate the plan, do not list alternatives
  unless asked. Max 5–8 lines of summary after a change.
- Prefer boring, mainstream, widely-known solutions any mid-level developer recognises instantly.
- Never leave `TODO`/`NotImplementedException` in a path the demo uses.
- Always build (and run relevant tests) before reporting done. Report actual command output.
- Never invent APIs, package versions, HTML selectors, or file paths — read the repo/fixtures first.
- If a needed input is genuinely missing (a selector, a business rule, an env value), make the
  smallest reasonable assumption, implement it, and flag it in one line as `ASSUMPTION: ...`.
- Stay inside your ownership area (see your chat mode). Cross-area needs → say
  `HANDOFF → <agent>: <one-line request>` and stop touching those files.