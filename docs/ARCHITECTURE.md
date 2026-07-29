# Architecture

## Layers

```
┌─────────────────────────────────────────────────────────────┐
│ AutoFinderAI.Api            Controllers, Program.cs,        │
│                              middleware, Swagger, JWT,      │
│                              Serilog, CORS                  │
├─────────────────────────────────────────────────────────────┤
│ AutoFinderAI.Infrastructure  EF Core + SQLite, migrations,  │
│                             OpenRouter client, otomoto      │
│                              crawler, JWT issuer, hasher    │
├─────────────────────────────────────────────────────────────┤
│ AutoFinderAI.Application     CQRS (MediatR) handlers,       │
│                              FluentValidation, DTOs,        │
│                              criteria extraction, ranking,  │
│                              abstractions (interfaces)      │
├─────────────────────────────────────────────────────────────┤
│ AutoFinderAI.Domain          Vehicle/Car (TPH), Money,      │
│                              ChatSession/ChatMessage, User, │
│                              CrawlRun, enums. Zero deps.    │
└─────────────────────────────────────────────────────────────┘
```

**Dependency rule:** `Api → Infrastructure → Application → Domain`, never reversed. `DbContext`,
`HttpClient` and ASP.NET types never appear in `Application` or `Domain`; `Application` defines
interfaces (`Abstractions/*`), `Infrastructure` implements them (DI composition in
[Program.cs](../src/AutoFinderAI.Api/Program.cs), [DependencyInjection.cs](../src/AutoFinderAI.Application/DependencyInjection.cs),
[DependencyInjection.cs](../src/AutoFinderAI.Infrastructure/DependencyInjection.cs)).

Frontend (Angular 20, standalone + signals) is a separate client consuming the API only over HTTP;
it has no compile-time dependency on the backend.

## Request flow: "user message → criteria → filter+rank → response"

Entry point: [SendMessageCommandHandler](../src/AutoFinderAI.Application/Features/Chat/SendMessage/SendMessageCommandHandler.cs).

1. User types a message in the chat UI; frontend calls `POST /api/chat/sessions/{id}/messages`.
2. `ChatController` sends a `SendMessageCommand` through MediatR.
3. Handler loads the session and prior turns, persists the user's `ChatMessage`.
4. `ICriteriaExtractor` (AI subsystem) sends the message + short history to the LLM via
   `IChatCompletionClient`, requesting **strict JSON** matching the `VehicleSearchCriteria` schema
   (temperature 0).
5. The raw JSON is deserialized into `RawCriteriaDto`, then passed through
   [CriteriaSanitizer](../src/AutoFinderAI.Application/Ai/CriteriaExtraction/CriteriaSanitizer.cs):
   unknown enum values dropped, numeric ranges clamped to sane bounds, reversed ranges swapped,
   lists deduplicated/capped, `limit` clamped to `[1, MaxLimit]`. If nothing usable comes out, the
   handler returns the LLM's clarification question instead of running a search.
6. If criteria exist, `IVehicleQueries.SearchAsync` runs the **hard filters in SQL** (EF Core,
   `AsNoTracking`) against SQLite and returns a capped candidate set (never the whole table).
7. `IVehicleRanker` (deterministic, in-memory) scores the candidate set against soft preferences
   and closeness-to-range, attaching human-readable `matchReasons`, then sorts per `SortBy`.
8. `IResponseComposer` builds a short markdown intro; the handler truncates to `Limit` results.
9. Assistant `ChatMessage` is persisted (content, `CriteriaJson`, `ResultVehicleIdsJson`,
   `ModelUsed`); `SaveChangesAsync` commits user + assistant messages in one transaction.
10. API returns the assistant message plus the structured result list; the frontend renders the
    markdown intro and a Cards/Table toggle for the results.

**The LLM never touches the database, never returns vehicles, and never ranks anything** — it only
produces criteria, an optional clarification question and a short intro. All filtering, scoring,
limits and match explanations are deterministic C#.

## Data model

`Vehicle` (abstract aggregate root) is mapped with EF Core **Table-Per-Hierarchy** using a
`VehicleCategory` discriminator ([Vehicle.cs](../src/AutoFinderAI.Domain/Vehicles/Vehicle.cs)).
`Car : Vehicle` adds car-specific fields (body type, doors, seats, drive type, damage flag, etc.).
Unique index on `(SourceKey, ExternalId)` is the upsert/dedupe key for the crawler.

**Extensibility contract:** adding a new category (e.g. `Motorcycle : Vehicle`) requires only a new
entity subclass, a new `VehicleCategory` enum value and a new `IListingSourceAdapter` — no changes
to the search, ranking, API contract or UI shape.

Supporting entities: `CrawlRun` (one row per crawl execution: status, items found/saved, error),
`User` (email/password hash), `ChatSession` and `ChatMessage` (role, markdown content, the criteria
and result-id JSON snapshot, model used).

## Crawler pipeline

`IListingSourceAdapter` is the seam for adding other listing sources without touching the rest of
the app. The only implementation today is
[OtomotoCarSourceAdapter](../src/AutoFinderAI.Infrastructure/Crawling/Otomoto/OtomotoCarSourceAdapter.cs):

1. Fetch the newest-first search results page (`IHtmlFetcher`, politeness delay + page cap from
   `CrawlerOptions`).
2. Parse listing links from the list page (`OtomotoListPageParser`, AngleSharp) — the list page is
   used only to discover detail-page URLs, because otomoto loads the human-readable "published X
   ago" text asynchronously and it isn't reliably present in the initial HTML.
3. Fetch each detail page and parse it (`OtomotoDetailPageParser`) to get the full, exact
   publication timestamp, which `OtomotoPublishedAtParser`/`OtomotoRelativeTimeParser` turn into a
   real `DateTime` (Polish month names, source time zone → UTC).
4. Keep only listings published within the configured recency window (default 24h).
5. Map the raw listing to a `Car` entity and upsert by `(SourceKey, ExternalId)`.
6. Record counts/errors on the `CrawlRun` row; a missing/unexpected field yields `null` for that
   field, never an exception that aborts the whole run.

Parser tests run against saved HTML fixtures in `tests/AutoFinderAI.UnitTests/Fixtures/otomoto/` —
no live HTTP requests happen in the test suite.

## AI subsystem boundary and safeguards

- `Application/Ai/CriteriaExtraction` (prompt building, JSON schema, sanitizing) and
  `Application/Ai/Ranking` (scoring, soft-preference rules) contain no HTTP/EF types.
- `Infrastructure/Ai/OpenRouterChatCompletionClient` is the only concrete `IChatCompletionClient`;
  a `NullChatCompletionClient` is used automatically when no API key is configured.
- Safeguards, in order: (1) strict JSON schema requested from OpenRouter at `temperature: 0`;
  (2) typed deserialization, never raw JSON forwarded to the client; (3) `CriteriaSanitizer`
  clamps/whitelists every field; (4) on unparseable/invalid output, one repair retry is attempted
  before falling back to a clear failure message; (5) **degraded mode** — with no API key or an
  unreachable OpenRouter, `GET /api/ai/status` reports `available:false`, the frontend disables the
  model picker with a banner, and chat falls back to a narrow keyword search over
  Title/Make/Model only (never a wide multi-column scan).
- User text and listing data are never interpolated into the system prompt; only the static system
  prompt (instructions + enum vocabularies) and the user's own message go to the model.

## Cross-cutting concerns

| Concern | Implementation |
|---|---|
| Validation | FluentValidation validators per command/query; failures → 400 with field errors |
| Error handling | [ProblemDetailsExceptionHandler](../src/AutoFinderAI.Api/Middleware/ProblemDetailsExceptionHandler.cs) maps unhandled exceptions and `ValidationException` to RFC 7807 `ProblemDetails`; expected failures use a `Result`/`Result<T>` type, not exceptions |
| Logging | Serilog to console + rolling daily file (`logs/log-.txt`); [CorrelationIdMiddleware](../src/AutoFinderAI.Api/Middleware/CorrelationIdMiddleware.cs) attaches/propagates `X-Correlation-Id` on every request/response |
| Auth | JWT bearer (`Microsoft.AspNetCore.Identity.PasswordHasher<User>` for hashing, no full Identity stack); `[Authorize]` by default, only `/api/auth/*`, `/health` and Swagger are anonymous; user-scoped data always filtered by the JWT `sub` claim |
| API docs | Swagger/Swashbuckle with a Bearer security scheme, enabled always (local-only app) |
| Config/secrets | `appsettings.json` + environment variables; `OPENROUTER_API_KEY` and `Jwt__Key` come from `.env` (git-ignored) or the container environment, never committed |
