# AI-Assisted Development Workflow

## Tools and models

Primary environment: **GitHub Copilot in VS Code** (chat + agent mode), which I chose over
alternatives because of my prior hands-on experience with the tool and its strong support for
repository-scoped instructions, custom chat modes/agents, and terminal-integrated agent execution.

Subscription: **Copilot Pro** (a small personal add-on), which gives me access to stronger
reasoning models without the cost of a full enterprise/API-metered setup — a deliberate
cost-vs-capability choice for a time-boxed recruitment project.

Models I used, matched to task difficulty:

| Model | Used for |
|---|---|
| **Claude Sonnet 5** | The large majority of the work: architecture, backend (CQRS/EF Core/crawler), frontend (Angular), and the AI subsystem itself. I chose it for its context window, reasoning quality on multi-file changes, and a favourable cost/performance promotion at the time of the project |
| **GPT-5.3-Codex** | The reviewer agent — adversarial diff review and test authoring, where a second, differently-trained model reduces the chance of the same blind spots as the authoring model |
| **GPT-5 mini / Claude Haiku 4.5** | Cheaper, simpler tasks: this documentation, minor fixes, and general debugging where I didn't need large context/deep reasoning |

## Setup: repository instructions and 6 specialised agents

Before writing any code, I analyzed the assignment text and turned it into a plan split across
**six specialised agent roles**, each with its own VS Code chat-mode file under
[.github/agents/](../.github/agents/) (`architect`, `backend-engineer`, `frontend-engineer`,
`ai-engineer`, `reviewer`, `scribe`), all bound by one shared, authoritative rulebook I wrote:
[.github/copilot-instructions.md](../.github/copilot-instructions.instructions.md).

Given the limited timebox and the breadth of the assignment (full-stack app + crawler + LLM
integration + tests + docs), I deliberately split the work this way rather than using one
general-purpose assistant: each agent gets a narrow, explicit scope, an explicit "you do not own"
list, its own model (matched to task difficulty/cost), and a restricted toolset, which keeps every
change reviewable and prevents one session from silently touching another layer.

I used AI itself to help me draft the initial version of each agent's prompt, so that every
agent's scope, responsibilities, available tools, hard rules and definition-of-done were specified
as precisely as possible before any implementation work started — I then read, adjusted and locked
in the prompts by hand.

Role boundaries (see the linked files for full detail):

- **architect** — solution scaffolding, project files, DI composition root, `Program.cs`,
  middleware pipeline, domain entities/enums, Docker, and any cross-cutting decision not owned by
  another agent. Final technical authority below me.
- **backend-engineer** — CQRS handlers/validators, EF Core persistence and migrations, JWT
  issuing/password hashing, the otomoto crawler, and controllers.
- **ai-engineer** — the OpenRouter client, prompt + JSON schema, criteria sanitizing, ranking/
  scoring, and the degraded-mode fallback.
- **frontend-engineer** — everything under `/frontend` (Angular auth, chat shell, results UI).
- **reviewer** — adversarial review of diffs against a fixed checklist (layering, security,
  correctness, data access, over-engineering, explainability) plus test authoring; write access
  limited to `tests/**`, everything else is proposed as a patch, never silently applied.
- **scribe** — README and `/docs/**` only; never allowed to change logic, signatures or config.

## Planned scope

I intentionally planned a broad feature scope (see `copilot-instructions.md` §2), to demonstrate
familiarity with the concerns a real production .NET + Angular project needs, not just the minimum
to satisfy the assignment text:

- **Backend:** Clean Architecture (Api/Infrastructure/Application/Domain), CQRS + MediatR,
  dependency injection, SQLite persistence, JWT auth, middleware, Serilog logging, Swagger, a
  deterministic filter/ranking engine over LLM-derived criteria, and the otomoto crawler
  (list discovery → detail fetch/parse → domain mapping → persistence).
- **Frontend:** Angular with login/registration, a session list, a chat window, a selectable AI
  model, and clear, readable result rendering.
- **AI integration:** OpenRouter-based chat completion behind an abstraction, natural-language
  criteria extraction with validation.
- **Architecture/deployment:** Docker containerization, clear separation of concerns, distinct
  dev/prod configuration.
- **Tests:** unit and integration tests, which I treated as a first-class concern specifically
  *because* AI-generated code needs a safety net, not less testing than hand-written code.

Because most of this scope is standard, well-established .NET + Angular boilerplate that doesn't
need to be reinvented per project, I considered it an intentionally good fit for AI-assisted
generation: my value here was in picking the right established pattern and catching deviations,
not in typing out DI registrations or controller scaffolding by hand.

## Human/AI split and where I concentrated my own effort

The **crawler** required by far the most direct involvement from me, because it is the one part of
the system where AI has no ability to verify itself against real-world facts (otomoto's actual
page structure) and where a plausible-looking wrong answer would silently corrupt the dataset.
Two concrete examples where I rejected or significantly changed AI's first pass:

1. **Database engine.** Left to its own defaults for a ".NET backend", Copilot's first proposal
   was SQL Server. I overrode that deliberately: for a local, single-user recruitment app — and
   realistically even in a plausible future for this project, which is not going to become an
   ERP-scale system — SQL Server is unjustified operational weight. I chose SQLite instead, which
   cascaded into a second, related correction I had to make: SQLite's EF Core provider does not
   sort `DateTimeOffset` columns correctly, which matters because the whole "published in the last
   24h" filter depends on correct chronological ordering. I changed all timestamp fields from
   `DateTimeOffset` to `DateTime` (UTC-only convention) specifically to fix this.

2. **Recency detection in the otomoto crawler.** otomoto's search-results list has no explicit
   "published date" filter or field — sorting can only be set to "newest first", and each list
   item shows a human-friendly relative string ("Published 5 hours ago", "Published yesterday").
   Two problems made this unsafe to let an AI agent guess at: (a) that relative text is only
   populated client-side after the page loads, so it is not reliably present in the HTML the
   crawler actually fetches; (b) it needs to be converted into a precise cutoff to decide whether
   a listing is inside the last 24 hours. Rather than let the model invent a parsing strategy, I
   specified the approach myself, by hand: fetch every listing's own detail page (which carries
   the exact publish date and time, in Polish, down to the minute), and only then parse and
   convert it to UTC. I provided concrete sample date strings up front so the model implemented
   against real data instead of guessing at a format — because a subtly wrong date parser would be
   very hard for me to notice later (it fails silently, by just returning stale or empty results,
   not by throwing an error).

Beyond the crawler, my role throughout was: setting the architecture and locking the domain model,
writing/curating the agent prompts and shared instructions, deciding scope boundaries (what *not*
to build), reviewing every terminal command an agent proposed before letting it run (especially
anything in PowerShell that could affect my machine), and — as described above — making the
explicit technology and data-modelling calls that AI would not have made on its own.

Two further examples of my own deliberate steering, on top of the two above:

- **I stated forward-extensibility requirements explicitly, not left implicit.** I required the
  domain model (`Vehicle`/`Car` via TPH) and the AI client abstraction (`IChatCompletionClient`) to
  support future growth without a redesign — respectively, adding other vehicle categories (e.g.
  motorcycles) with only a new subtype + adapter, and swapping/adding LLM providers without
  touching the rest of the app.
- **I specified secret handling as a hard requirement, not an afterthought.** I required the
  OpenRouter API key and JWT signing key to live only in a git-ignored `.env` file
  (`.env.example` committed instead), never in `appsettings.json` or in agent-visible config that
  gets committed.

## Verification

I did not accept AI output on trust; I checked each layer in its own way:

- **Build/compile as a gate.** Every agent's definition-of-done requires `dotnet build` (backend)
  or `npm run build` / `ng build` (frontend) to succeed before I accept a change as done.
- **Automated tests.** I targeted unit tests at exactly the highest-risk, least-obvious-to-verify-
  by-eye logic: the otomoto HTML parsers (against saved fixture pages, no live HTTP), criteria
  sanitizing/validation (valid input, malformed JSON, unknown enums, reversed ranges, limit
  clamping), and the deterministic ranking/scoring logic — see
  [tests/AutoFinderAI.UnitTests](../tests/AutoFinderAI.UnitTests).
- **A dedicated reviewer agent**, running on a different model than the one that wrote the code,
  performs an adversarial pass against a fixed checklist (layering violations, security gaps such
  as a missing `[Authorize]` or an unfiltered user-scoped query, correctness issues like unvalidated
  LLM output or swallowed exceptions, N+1/full-table-scan data access, over-engineering, and
  explainability) before I consider the code final.
- **My own manual read-through** of the parts that matter most if wrong: the criteria sanitizer,
  the ranking/scoring logic, and the crawler's parsing/date logic — precisely the areas where a
  subtly wrong AI answer would look correct at first glance but silently produce bad data or bad
  search results.
- **Manual approval of terminal/PowerShell commands.** I read and approved every command an agent
  proposed to run locally before execution, specifically to avoid an agent taking an action with a
  real, potentially irreversible effect on my machine.

## Limitations, privacy and cost

**Known limitations of the current AI approach:**

- Chat search is only as good as the configured free-tier OpenRouter models; I did not use a
  paid/stronger model, by choice, to keep the AI cost of the project at effectively zero.
- Without a working OpenRouter connection (missing key, network issue, provider outage), the chat
  degrades to a narrow keyword search (Title/Make/Model only) and does not attempt any smarter
  local fallback — an intentional simplification on my part, and a clear candidate for future
  improvement (see the README's known-limitations list for more).
- The criteria schema, ranking weights and soft-preference rules are fixed and hand-reviewed by me;
  the LLM cannot expand them at runtime, which is a deliberate safety trade-off, not an oversight.

**Privacy:**

- The only data sent to the LLM (via OpenRouter) is the user's own free-text chat message plus a
  short recent-turn history from the same session. No vehicle/listing data, no other users' data,
  and no database content is ever included in a prompt.
- I don't collect any personally identifying information beyond an email/password pair used purely
  for authentication; that data never leaves the local database and is never sent to the LLM.
- The OpenRouter API key lives only in an environment variable (`.env`, git-ignored, or the
  container environment) — never committed, never logged, never returned by any API response.

**Cost assumptions:**

- Exactly one LLM call per user chat message (no agent loop, no multi-step tool calling), at
  `temperature: 0` for reproducibility, against OpenRouter's free-tier model catalogue.
- I log token usage, model, and request duration (never the raw prompt content) so I can review
  cost/latency after the fact without exposing user text in logs.
- Given the small, single-call-per-message footprint and my use of free-tier models, the marginal
  AI runtime cost of running this app is effectively zero; the only real cost I incurred was the
  Copilot Pro subscription used to build it, which — for the amount and quality of code produced in
  the available time — was a negligible expense to me.
