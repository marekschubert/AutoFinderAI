# AutoFinderAI

Fullstack recruitment-task app: crawls car listings from otomoto.pl into SQLite, then lets a user
describe what they're looking for in a chat UI. An LLM (via OpenRouter) turns that into structured
search criteria; the backend (EF Core, deterministic C#) filters/ranks the local dataset and returns
the results. The LLM never touches the database and never invents listings.

Stack: **.NET 10 / ASP.NET Core** (Clean Architecture + CQRS/MediatR, EF Core + SQLite, JWT auth,
Serilog) and **Angular 20** (standalone components, signals, Angular Material).

## Prerequisites

Only one of the following is required to run the app:

- **Docker Desktop** (recommended — no other tooling needed), *or*
- **.NET 10 SDK** + **Node.js 22+** to run backend and frontend directly.

You also need a free **OpenRouter** API key (https://openrouter.ai/keys) — the app uses free-tier
models by default, so no paid credits are required.

## Option A — Run with Docker (recommended)

This is the easiest way to run the whole app with one command; both containers talk to each other
over the Docker network and are exposed on `localhost`.

1. Copy the environment template and fill in the two required secrets:
   ```powershell
   Copy-Item .env.example .env
   ```
   Edit `.env` and set:
   - `OPENROUTER_API_KEY` — your OpenRouter key
   - `Jwt__Key` — any random string, **min. 32 characters** (used to sign login tokens)

2. Build and start everything:
   ```powershell
   docker compose up --build
   ```
   First run downloads base images and installs npm/NuGet packages, so it can take a few minutes.
   Subsequent runs are much faster thanks to Docker layer caching.

3. Open the app:
   - Frontend (UI): **http://localhost:4200**
   - Backend API / Swagger: **http://localhost:8080/swagger**
   - Backend health check: **http://localhost:8080/health**

4. Stop everything:
   ```powershell
   docker compose down
   ```
   The SQLite database lives in a named Docker volume (`autofinderai-data`), so data survives
   `docker compose down`. To wipe it too, run `docker compose down -v`.

## Option B — Run without Docker (manual dev mode)

Useful if Docker isn't available. Requires .NET 10 SDK and Node.js 22+.

**Backend** (from repo root):
```powershell
Copy-Item .env.example .env   # then fill in OPENROUTER_API_KEY and Jwt__Key
cd src/AutoFinderAI.Api
dotnet run
```
API starts on **http://localhost:5066** (see `Properties/launchSettings.json`) and applies EF Core
migrations automatically on startup. `.env` at the repo root is picked up automatically.

**Frontend** (in a second terminal, from `frontend/`):
```powershell
npm install
npm start
```
This runs `ng serve` on **http://localhost:4200**, which proxies `/api/*` to `http://localhost:5066`
via `proxy.conf.json` — no CORS issues, no extra config needed.

Open **http://localhost:4200** in a browser.

## Configuration reference

| Setting | Where | Purpose |
|---|---|---|
| `OPENROUTER_API_KEY` | `.env` | Required. LLM gateway key. |
| `Jwt__Key` | `.env` | Required. Min 32 chars, signs auth tokens. |
| `Ai:DefaultModel` / `Ai:Models` | `src/AutoFinderAI.Api/appsettings.json` | Free-tier model catalogue; edit and restart, no rebuild needed. |
| `Cors:AllowedOrigins` | `appsettings.json` | Must include the frontend origin (`http://localhost:4200` by default). |

## Troubleshooting

- **API returns 500 on every request** — `Jwt__Key` is missing/empty. Check `.env`.
- **Port already in use** — something else is using 4200/8080/5066; stop it or edit the port
  mappings in `docker-compose.yml` (Docker) or `launchSettings.json` / `angular.json` (manual mode).
- **Frontend can't reach the API** — make sure both containers are running
  (`docker compose ps`) and that you're browsing to `http://localhost:4200`, not `:8080`.