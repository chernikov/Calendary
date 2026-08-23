# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Calendary — a Ukrainian custom AI-generated photo-calendar ordering app. The UI/UX comes from a
Claude Design doc (`Calendary.dc.html`, "Broadsheet" design system); the implementation is a thin,
end-to-end vertical slice: Docker + ASP.NET Core (.NET 10) + EF Core/MSSQL + Angular 18.

Flow: landing → register/login (email+password or Google) → photo upload → style + personal dates
→ generation (live progress) → cover confirm → month-by-month reveal (regenerate/failure/retry) →
review → delivery + payment → order status (auto-progressing Paid → Printing → Shipped →
Delivered).

Auth is real (email+password via `PasswordHasher<User>`, and Google Sign-In via ID-token
verification — see "Backend architecture" below), not mocked. Transactional email (currently just
a welcome email on registration) is also real, via `IEmailService`/`ResendEmailService`. **Two
integrations are still deliberately mocked** behind `Calendary.Domain.Abstractions` interfaces —
swap the DI registration in `Program.cs` to go live with a real provider:
- `IImageGenerationService` — AI image generation (currently returns picsum.photos placeholders).
  A real implementation already exists (`AiImageGenerationService`, backed by the `Calendary.AI`
  project) but isn't wired in by default — see the README's "Calendary.AI" section for the
  three-step switch-over.
- `IPaymentService` — payment charging (currently always succeeds)
- `INovaPoshtaService` — delivery branch lookup (currently a small static city/warehouse list)

## Commands

**Full stack, local dev** (from repo root):
```bash
docker compose up --build
```
Frontend on :4200, backend/Swagger on :5080 (`/swagger`, Development only), MSSQL on :1433
(sa / see `docker-compose.yml`). EF Core migrations apply automatically on backend startup
(`Program.cs` calls `db.Database.Migrate()`).

**Backend only** (from `backend/`):
```bash
dotnet build                              # build the solution (Calendary.slnx)
dotnet run --project src/Calendary.Api    # runs on http://localhost:5128 (see launchSettings.json)
```
Needs `ConnectionStrings:Default` reachable (defaults to `localhost,1433` in
`src/Calendary.Api/appsettings.json` — point it at a running MSSQL, e.g. the one from
`docker compose up mssql`).

**EF Core migrations** (from `backend/`):
```bash
dotnet ef migrations add <Name> \
  --project src/Calendary.Infrastructure/Calendary.Infrastructure.csproj \
  --startup-project src/Calendary.Api/Calendary.Api.csproj \
  --output-dir Migrations
```

**Frontend only** (from `frontend/`):
```bash
npm install
npm start        # ng serve on :4200, proxies /api/* to http://localhost:5080 (proxy.conf.json)
npm run build    # production build -> dist/calendary/browser
```

There are no automated tests in this repo yet.

## Backend architecture (`backend/src/`)

Four-project split:
- **Calendary.Domain** — entities (`User`, `Order`, `Sheet`, `StyleCategory`, `PersonalDate`,
  `Payment`, `Delivery`), enums, and the `Abstractions/` interfaces listed above. No EF/ASP.NET
  dependency.
- **Calendary.Infrastructure** — `Data/AppDbContext.cs` (+ `Migrations/`), and `Services/`:
  the `Mock*` implementations of the Domain interfaces, `AiImageGenerationService` (the real,
  not-wired-in-by-default `IImageGenerationService` — see README), and two `BackgroundService`s
  that drive the app's async state machines purely by elapsed time:
  - `GenerationBackgroundService` — progresses up to 3 `Sheet`s per order concurrently
    (`Pending` → `Generating` → `Ready`, ~4s each), cover (index 0) first, then months 1–12.
    Only meant to run when `MockImageGenerationService` is active — see README before enabling
    `AiImageGenerationService` alongside it.
  - `FulfillmentBackgroundService` — advances `Order.Status` `Paid` → `Printing` → `Shipped`
    (assigns a fake ТТН) → `Delivered` at fixed intervals after payment.
  Both key off `Order.StatusUpdatedAtUtc`, which `Order.SetStatus()` keeps in sync — always call
  `SetStatus()` rather than assigning `.Status` directly, or the background services' (and
  `AiImageGenerationService`'s own `OrderProgressionHelper`) timing/transition logic breaks.
- **Calendary.AI** — standalone (no reference to the other three projects): `Options/AiOptions.cs`
  (binds the `AI` appsettings section), `Clients/IAiImageClient.cs` + `OpenAiImageClient` +
  `GeminiImageClient` (real HTTP calls; `ServiceCollectionExtensions.AddCalendaryAi()` registers
  whichever `AiOptions.Provider` selects), `Prompts/CalendarPrompts.cs` (the actual prompt text
  per style category / month, English by design).
- **Calendary.Api** — Controllers, `Auth/BearerTokenAuthenticationHandler` (a custom
  `AuthenticationHandler` for opaque bearer tokens, scheme `"Bearer"` — **not** JWT/`JwtBearer`;
  resolves tokens via `ISessionTokenService`), and `Dtos/` (record DTOs + `DtoMapping.cs` extension
  methods, e.g. `order.ToDto()`). `AuthController` has `register`/`login`/`google`/`me`, backed by
  `IPasswordAuthService`/`IGoogleAuthService`/`ISessionTokenService` (all in Infrastructure —
  `SessionTokenService` persists sessions as `UserSession` rows, hashing the bearer token with
  SHA-256 before storage, specifically so a backend restart on deploy doesn't log everyone out).

**Order state machine** (`OrderStatus`): `Created` → `PhotoUploaded` → `DetailsSubmitted` →
`Generating` → `CoverReady` → `CoverConfirmed` → `ReviewReady` → `AwaitingPayment` → `Paid` →
`Printing` → `Shipped` → `Delivered` (or `Cancelled` / `GenerationFailed`). A `Sheet` is one image
slot: `Kind.Cover` at `Index=0`, `Kind.Month` at `Index=1..12`. Regenerations are a single shared
budget per order (`Order.RegenerationsRemaining`), decremented in `MockImageGenerationService`.

## Frontend architecture (`frontend/src/app/`)

Angular 18, **standalone components only** (no NgModules), inline templates, one component per
screen under `pages/`. New-style control flow (`@if`/`@for`) is used throughout instead of
`*ngIf`/`*ngFor`.

- `core/` — `AuthService` (signal-based; bearer token persisted to `localStorage`),
  `OrderService` (thin HTTP wrapper over every `/api/orders/*` endpoint), `auth.interceptor.ts`
  (attaches the bearer token to outgoing requests), `auth.guard.ts` (redirects to `/start` if
  unauthenticated).
- Routing (`app.routes.ts`) is order-scoped: every step after login is
  `/order/:orderId/<step>` (`upload`, `style`, `generating`, `cover`, `months/:month`, `review`,
  `checkout`, `status`). `month.component.ts` re-reads `route.paramMap` in `ngOnInit` (not just the
  constructor snapshot) because Angular reuses the component instance across `/months/1` →
  `/months/2` navigations.
- `styles.css` is the Broadsheet design system's tokens (CSS custom properties: `--color-*`,
  `--font-*`, `--space-*`, `--radius-*`, `--shadow-*`) and component classes (`.btn`, `.card`,
  `.field`/`.input`, `.tag`, `.nav`, `.dialog`) hand-ported from the design doc. Pages use these
  classes directly rather than component-scoped CSS — check `styles.css` for the available
  vocabulary before inventing new classes.
- `index.html` **must** keep `<base href="/">` — without it, deep-link page reloads (e.g. landing
  directly on `/order/.../review`) resolve the built JS/CSS asset paths relative to the current
  route instead of the app root, and the page loads blank.

## Deployment (`deploy/`, `.github/workflows/`)

One DigitalOcean droplet (`207.154.222.66`, 1 vCPU / 2GB RAM) hosts **two fully isolated stacks**
behind a single shared **edge** Caddy instance (automatic HTTPS for both domains):

| Branch | Workflow | Domain | Image tag | Compose project |
| --- | --- | --- | --- | --- |
| `main` (prod) | `deploy.yml` | calendary.com.ua | `:latest` | `calendary` |
| `develop` (staging) | `deploy-staging.yml` | staging.calendary.com.ua | `:develop` | `calendary-staging` |

Each stack has its **own** MSSQL container/volume/password (`docker-compose.prod.yml` /
`docker-compose.staging.yml`) and joins a shared external Docker network (`web`) only through its
`frontend` service (network-aliased `frontend-prod` / `frontend-staging`), which is what
`docker-compose.edge.yml` + `Caddyfile` reverse-proxy to. Only Caddy (80/443) is exposed on the
host — `mssql`/`backend` are internal-only in both stacks, same as local dev.

Given the droplet's small RAM budget: both MSSQL instances are memory-capped via
`MSSQL_MEMORY_LIMIT_MB` (768 prod / 512 staging), and a 2GB swap file provides headroom — a
demo/hobby-scale tradeoff, not a sizing template to copy elsewhere. `deploy/bootstrap.sh` is the
one-time droplet setup script (installs Docker, creates the `web` network, seeds `.env`/
`.env.staging` templates).

**Branching policy**: `develop` is the working branch (deploys to staging automatically on push);
`main` is production (deploys to prod automatically on push/merge). Both `.github/workflows/*.yml`
pipelines build images and push to GHCR (`ghcr.io/<owner>/calendary-backend` /
`calendary-frontend`) before SSHing into the droplet to `docker compose pull && up -d`. The two
pipelines are also where `GOOGLE_CLIENT_ID`/`GOOGLE_CLIENT_SECRET` GH secrets get threaded into the
droplet's `.env`/`.env.staging` on every deploy (see README's "Auth" section) — unlike the AI
provider keys, which are a manual one-off `.env` edit.
