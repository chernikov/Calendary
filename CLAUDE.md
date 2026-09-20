# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Calendary — a Ukrainian custom AI-generated photo-calendar ordering app. The UI/UX comes from a
Claude Design doc (`Calendary.dc.html`, "Broadsheet" design system); the implementation is a thin,
end-to-end vertical slice: Docker + ASP.NET Core (.NET 10) + EF Core/MSSQL + Angular 18.

**Deliberately UA-only, no i18n** (see #306): delivery (Nova Poshta), payment (Monobank), currency
(₴), and every prompt/UX string are Ukraine-market-specific by design, not an accident of not
having gotten to localization yet. Frontend strings stay hardcoded Ukrainian in component
templates — don't introduce ngx-translate/Angular i18n or resource files speculatively. Revisit
only if the product actually expands to another market.

Flow: landing → register/login (email+password or Google) → photo upload → per-sheet prompt+style
plan (prompt library: themes → prompts, image styles) + personal dates
→ generation (live progress) → cover confirm → month-by-month reveal (regenerate/failure/retry) →
review → delivery + payment → order status (auto-progressing Paid → Printing → Shipped →
Delivered).

Auth is real (email+password via `PasswordHasher<User>`, and Google Sign-In via ID-token
verification — see "Backend architecture" below), not mocked. Transactional email (currently just
a welcome email on registration) is also real, via `IEmailService`/`ResendEmailService`. Three
integrations have a real implementation wired in, each falling back to something local-dev-friendly
when unconfigured (so local dev needs no API key/account):
- `IPaymentService` — payment. `MonobankPaymentService` calls the real Monobank Acquiring API
  (invoice creation + webhook-verified confirmation, see "Payments" below) when
  `Monobank__MerchantToken` is configured; otherwise it settles the order as Paid immediately (no
  real redirect), so local dev needs no merchant account.
- `IImageGenerationService` — AI image generation. A real implementation exists
  (`AiImageGenerationService`, backed by the `Calendary.AI` project); which one runs is a runtime
  DB setting via the admin panel (`/admin/settings`), not a DI swap — see the README's
  "Calendary.AI" section for the three-step switch-over, and picsum.photos placeholders otherwise.
- `INovaPoshtaService` — delivery branch lookup. `NovaPoshtaService` calls Nova Poshta's real
  public Address API when `NovaPoshta__ApiKey` is configured, otherwise falls back to the same
  small static city/warehouse list it always used.

### Payments

Checkout only offers Monobank now (`checkout.component.ts`) — Apple Pay/Google Pay/Card were
placeholder UI with no real provider behind them and were dropped rather than left half-wired.
The flow is redirect-based, not a synchronous charge: `POST /api/orders/{id}/pay` calls
`MonobankPaymentService.CreateInvoiceAsync`, which creates a Monobank invoice and returns a hosted
`pageUrl`; the frontend hard-navigates the browser there (`window.location.href`, not a router
link — the SPA is left entirely). Monobank later POSTs the outcome to
`POST /api/payments/monobank/webhook` (`PaymentsController`, anonymous — Monobank has no bearer
token for this app), which `HandleWebhookAsync` verifies via the `X-Sign` header (ECDSA-SHA256
over the raw body, checked against the merchant's public key fetched once from
`/api/merchant/pubkey` and cached for the process lifetime) before applying `Paid`/`Failed` to the
matching `Payment` (looked up by `Payment.ProviderInvoiceId`, Monobank's `invoiceId`) and calling
`Order.SetStatus`. The customer's browser is *also* redirected back to `/order/{id}/status` via
Monobank's `redirectUrl`, but that redirect is UX only — the status page's existing 2s poll is what
actually picks up the webhook-driven `Paid` transition, since the webhook can arrive slightly after
the redirect.

Building `redirectUrl`/`webHookUrl` needs the app's own public origin, which can't be reliably
inferred from the request (no forwarded-headers middleware, and nginx→backend is plain HTTP
internally regardless of the public scheme) — so it's the explicit `Monobank__PublicBaseUrl` env
var (`https://${DOMAIN}` / `https://${STAGING_DOMAIN}` in the prod/staging compose files, empty
locally, matching `Cors__AllowedOrigins__0`'s existing convention). No new GH secret was needed for
this — `DOMAIN`/`STAGING_DOMAIN` already exist in the droplet's `.env` for Caddy.

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
Needs `ConnectionStrings:Default` set via `dotnet user-secrets` (the committed
`src/Calendary.Api/appsettings.json` value is intentionally blank — see README's "Environment
variables" section, #296) pointing at a running MSSQL, e.g. the one from `docker compose up mssql`
(password from your own `.env`, see `.env.example`).

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

Six-project split:
- **Calendary.Common** — zero-dependency, framework-agnostic primitives shared across layers:
  `CalendarYear` (the "next year" calendar-year calc), `AppOperationException` (see below). No
  project references of its own.
- **Calendary.Domain** — entities (`User`, `Order`, `Sheet`, `PromptTheme`, `Prompt`,
  `ImageStyle`, `PersonalDate`,
  `Payment`, `Delivery`), enums, and the `Abstractions/` interfaces listed above. No EF/ASP.NET
  dependency.
- **Calendary.Application** — Commands/Queries + their MediatR `IRequestHandler<,>`s, one file per
  operation, co-located (vertical-slice style) rather than split across layers. References `Domain`
  and `Common` only — **never** `Infrastructure`, so handlers can't depend on the concrete EF
  `AppDbContext`. Instead they depend on `IAppDbContext` (`Application/Common/IAppDbContext.cs`,
  mirrors every `DbSet<T>` the real `AppDbContext` declares), which the real `AppDbContext` just
  implements (`: DbContext, IAppDbContext`) — this is why `IAppDbContext` lives in Application
  rather than Domain: it needs the `DbSet<T>` type from EF Core, and Domain is kept free of any
  EF/ASP.NET dependency. Currently covers `OrdersController`'s logic only (`Application/Orders/`);
  other controllers still inject `AppDbContext` directly pending the same treatment. Error
  signaling: a handler returns `null` for "not found or not owned" (the caller does
  `is null ? NotFound() : Ok(...)`), or throws `AppOperationException(message, statusCode)` (in
  `Calendary.Common`) for a validation (400) / conflict (409) failure — caught by a
  controller-scoped `[TypeFilter(typeof(OrderOperationExceptionFilter))]` (not global middleware),
  never a bare ASP.NET `BadRequest(...)`/`Conflict(...)` inside a handler. Per-feature shared
  helpers (order loading + ownership filtering + business-rule predicates) live in one static class
  per feature (e.g. `Orders/OrderAccess.cs`) that every handler in that feature calls into — this
  is the single point that must enforce "you can only touch your own order," replacing what used
  to be two near-duplicate copies (one per controller).
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
- **Calendary.AI** — standalone (no reference to any other project in the solution): `Options/AiOptions.cs`
  (binds the `AI` appsettings section), `Clients/IAiImageClient.cs` + `OpenAiImageClient` +
  `GeminiImageClient` (real HTTP calls; `ServiceCollectionExtensions.AddCalendaryAi()` registers
  whichever `AiOptions.Provider` selects), `Prompts/CalendarPrompts.cs` (the actual prompt text
  per style category / month, English by design).
- **Calendary.Api** — Controllers, `Auth/BearerTokenAuthenticationHandler` (a custom
  `AuthenticationHandler` for opaque bearer tokens, scheme `"Bearer"` — **not** JWT/`JwtBearer`;
  resolves tokens via `ISessionTokenService`), and `Dtos/` (record DTOs + `DtoMapping.cs` extension
  methods, e.g. `order.ToDto()`). `OrdersController` is thin (auth + request→Command/Query mapping
  + `sender.Send(...)` + `.ToDto()`, no `AppDbContext`) — see the Application bullet above for the
  pattern; other controllers (`AdminController` in particular) haven't been migrated to it yet and
  still inject `AppDbContext` directly. `AuthController` has `register`/`login`/`google`/`me`, backed by
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
pipelines are also where `GOOGLE_CLIENT_ID`/`GOOGLE_CLIENT_SECRET`, `RESEND_API_KEY`,
`NOVA_POSHTA_API_KEY` (same value both stacks — no sandbox/live split for a read-only lookup),
`DO_SPACES_KEY`/`DO_SPACES_SECRET`/`DO_SPACES_REGION`/`RESTIC_PASSWORD` (same values both stacks),
`DO_SPACES_BUCKET`/`DO_SPACES_BUCKET_STAGING` (separate buckets per stack — `deploy/backup.sh`
also scopes each stack to its own restic repo path within its bucket regardless, see #305), and
`MONOBANK_MERCHANT_TOKEN`/`MONOBANK_MERCHANT_TOKEN_STAGING` (separate prod/sandbox
tokens, same `.env`/`.env.staging` variable name), and `ADMIN_PASSWORD`/`ADMIN_PASSWORD_STAGING`
(separate passwords, same `.env`/`.env.staging` variable name — see `AdminSeeder` below) GH
secrets get threaded into the droplet's `.env`/`.env.staging`
on every deploy (see README's "Auth" section). The AI provider keys are the one exception: staging
threads them too, but prod's are a manual one-off `.env` edit (see issue #330) — worth checking
before assuming any given secret is deploy-automated.

The current `NOVA_POSHTA_API_KEY` secret (set 2026-09-19) expires **2027-09-19** — Nova Poshta
deactivates keys yearly. Regenerate it in the business account
(`new.novaposhta.ua/dashboard/settings/developers` → Security → Create key) and
`gh secret set NOVA_POSHTA_API_KEY` before then, or delivery branch lookup silently falls back to
the static mock dataset (see `NovaPoshtaService`).

**Restoring from backup** (see #305): full step-by-step is `deploy/RESTORE.md` — in short, `restic
-r <repo> snapshots` to see what's there, `restic restore latest --tag db|media --target <dir>` to
pull a snapshot out, then `docker cp` the `.bak` into the `mssql` container and `RESTORE DATABASE
... WITH REPLACE` for the DB, or untar into the media volume with the `backend` service stopped.
Four one-click, parameter-free `workflow_dispatch`-only workflows cover manual backup/restore, so
triggering the right one from the GitHub UI needs no memorized flags:
- `backup-prod.yml` / `backup-staging.yml` — out-of-band full backup (DB + media) of one stack,
  between the daily `calendary-backup.timer` ticks, no droplet SSH needed.
- `restore-staging.yml` — runs the restore procedure above end-to-end against staging. Safe to
  trigger anytime to rehearse it, since it only restores staging's own latest backup over itself —
  no confirmation gate.
- `restore-prod.yml` — same procedure against the **live** prod stack. Requires typing the exact
  phrase `restore-prod` into the `confirm` input or the job aborts before touching the droplet —
  this overwrites real customer data and briefly interrupts the site (`SINGLE_USER` during
  `RESTORE DATABASE`, backend stopped while media is untarred).

**Admin login**: `AdminSeeder` (`backend/src/Calendary.Infrastructure/Services/AdminSeeder.cs`)
runs at startup, right after `db.Database.Migrate()`, and unconditionally re-hashes/writes
`admin@calendary.com.ua` with `Role = Admin` from `AdminSeed:Password` (the `ADMIN_PASSWORD`/
`ADMIN_PASSWORD_STAGING` env vars) — config is the source of truth every single startup, so
rotating the GH secret and redeploying changes the live password with no DB access needed. If the
password env var is unset, seeding is skipped (logged warning) rather than creating an account
with an empty/guessable password — meaning a fresh environment with no `ADMIN_PASSWORD` set has
*no* admin account at all until the secret is provided.
