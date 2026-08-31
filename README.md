# Calendary

Custom AI-generated photo calendar ordering app. A thin, end-to-end vertical slice built from the
`Calendary.dc.html` design ("Broadsheet" design system): docker + .NET + MSSQL + Angular.

## Flow implemented

Landing → register/login (email+password or Google) → photo upload → style + personal dates → generation
(live progress) → cover pick → month-by-month reveal (regenerate/failure/retry) → review →
delivery + payment (Nova Poshta + Apple/Google Pay/monobank/card) → order status (auto-progressing
Paid → Printing → Shipped → Delivered, with cancellation while unpaid).

## Stack

- **backend/** — ASP.NET Core (.NET 10) Web API, EF Core + SQL Server, split into
  `Calendary.Domain` / `Calendary.Infrastructure` / `Calendary.Api` / `Calendary.AI`.
- **frontend/** — Angular 18 standalone app, styled with the Broadsheet design tokens
  (`frontend/src/styles.css`), served via nginx in production.
- **docker-compose.yml** — wires up `mssql`, `backend`, `frontend`.

## Running it

```bash
docker compose up --build
```

- Frontend: http://localhost:4200
- Backend/Swagger: http://localhost:5080/swagger
- SQL Server: localhost:1433 (sa / Your_password123 — dev only, change before any real deployment)

EF Core migrations apply automatically on backend startup.

## What's mocked (by design — see conversation scope: "thin vertical slice" + "mocked services")

- **AI image generation** — `MockImageGenerationService` + `GenerationBackgroundService` simulate
  generation server-side (up to 3 sheets in flight per order, ~4s each) and hand back placeholder
  photos from picsum.photos. Swap `IImageGenerationService` for a real provider.
- **Payment** — `MockPaymentService` always succeeds after a short delay. No real card data is
  collected. Swap `IPaymentService` for a real provider (Stripe, WayForPay, etc.).
- **Nova Poshta** — `MockNovaPoshtaService` returns a small static city/warehouse list instead of
  calling the real Nova Poshta API.
- **Order fulfillment timing** — `FulfillmentBackgroundService` advances Paid → Printing → Shipped
  → Delivered purely by elapsed wall-clock time (8s / 10s / 20s), not real print/courier events.

## Auth — email+password and Google Sign-In

Both are real (not mocked). `POST /api/auth/register` / `/login` use
`Microsoft.AspNetCore.Identity`'s standalone `PasswordHasher<User>` (no full Identity/EF
UserManager stack). `POST /api/auth/google` verifies a Google Identity Services ID token
client-side-obtained credential via `Google.Apis.Auth`'s `GoogleJsonWebSignature.ValidateAsync` —
an ID-token flow, not server-side OAuth code exchange, so no redirect URI is needed (the Client
Secret isn't actually used by this flow, but is still configured/available). Sessions are
DB-backed (`UserSession`, bearer token stored as a SHA-256 hash) rather than in-memory, so a
container restart (which happens on every deploy) doesn't log everyone out.

Config lives in the `Google` appsettings section (`ClientId`/`ClientSecret`, both blank in the
committed `appsettings.json`, same convention as the `AI` section). The frontend's
`environment.googleClientId` is a plain committed literal, not a secret — it's sent to the browser
and to Google on every sign-in regardless.

**GCP setup**: OAuth Client ID/Secret live under the `calendary-ua` GCP project
(`calendary`/`calendary-app` were already taken globally). The IAP `brands`/OAuth-client REST API
that normally scripts this requires the project to belong to a Workspace organization, which a
personal-account project doesn't — so the OAuth consent screen and Web OAuth client (Authorized
JavaScript origins: `https://calendary.com.ua`, `https://staging.calendary.com.ua`,
`http://localhost:4200`) were created manually via Cloud Console → APIs & Services.

## Transactional email — Resend

`IEmailService`/`ResendEmailService` (Infrastructure) call the Resend API directly over HTTP (no
SDK). Currently used for one thing: a welcome email on `/api/auth/register` — best-effort, wrapped
in try/catch so a Resend outage never fails registration itself. Config is the `Resend` appsettings
section (`ApiKey`/`FromEmail`/`FromName`, same blank-by-default/env-injected convention as `AI` and
`Google`); with no `ApiKey` configured, `ResendEmailService` just logs and skips sending — safe for
local dev.

`calendary.com.ua`'s sending domain (DKIM `resend._domainkey` TXT record, `send`/`rsend` CNAMEs) is
verified directly in DigitalOcean DNS. `RESEND_API_KEY` is a GitHub secret, threaded into the
droplet's `.env`/`.env.staging` on every deploy the same way as the Google OAuth secrets.

## Calendary.AI — real AI image generation

`backend/src/Calendary.AI` is a standalone project (no dependency on the other three) holding the
actual AI provider integration, built but **not wired in by default**:

- `Options/AiOptions.cs` — binds the `AI` section of `appsettings.json`: `Provider` (`OpenAI` or
  `Gemini`) plus one sub-section per provider (`ApiKey`, `BaseUrl`, `Model`). Both API keys are
  blank in the committed `appsettings.json` — set the real one via environment variable
  (`AI__OpenAI__ApiKey` / `AI__Gemini__ApiKey`, wired optionally into
  `deploy/docker-compose.{prod,staging}.yml` from `AI_OPENAI_API_KEY` / `AI_GEMINI_API_KEY` in
  `.env`), never committed.
- `Clients/` — `IAiImageClient` plus one real HTTP implementation per provider
  (`OpenAiImageClient` calls `/images/edits` when a reference photo is supplied, else
  `/images/generations`; `GeminiImageClient` calls `generateContent` with the photo as inline
  image data). `ServiceCollectionExtensions.AddCalendaryAi()` registers only the implementation
  `AiOptions.Provider` selects.
- `Prompts/CalendarPrompts.cs` — wraps the DB-stored prompt library texts (per-sheet scene from
  `Prompt.Text` + visual style from `ImageStyle.Text`) and a seasonal hint per month, composed
  into `BuildCoverPrompt` / `BuildMonthPrompt`. Prompts are in English (both
  providers follow English instructions more reliably) even though the product copy is Ukrainian.

`Calendary.Infrastructure/Services/AiImageGenerationService.cs` is a real `IImageGenerationService`
built on top of this — unlike the mock, it drives generation itself (fire-and-forget per-order/
per-sheet work using its own DI scope, throttled to 3 concurrent months) rather than relying on
`GenerationBackgroundService`'s timer-based simulation.

**Going live** (three steps, done together):
1. Set a real key in `AI:OpenAI:ApiKey` or `AI:Gemini:ApiKey` (and `AI:Provider` to match).
2. In `Program.cs`: call `builder.Services.AddCalendaryAi(builder.Configuration)` and register
   `AiImageGenerationService` instead of `MockImageGenerationService`.
3. Remove `GenerationBackgroundService`'s hosted-service registration — it would otherwise race
   the real generation calls on the same `Sheet` rows (both flip `Pending` → `Generating` →
   `Ready`). `AiImageGenerationService` doesn't need it: it advances `Order.Status` itself via
   `OrderProgressionHelper` after each sheet completes.

## Deployment — production + staging

One droplet (`207.154.222.66`, 1 vCPU / 2GB) hosts both environments behind a single shared
**edge** Caddy instance (automatic HTTPS for both hostnames):

| Branch | Workflow | Domain | Image tags | Compose project |
| --- | --- | --- | --- | --- |
| `main` | `.github/workflows/deploy.yml` | calendary.com.ua | `:latest` | `calendary` (+ `calendary-edge` for Caddy) |
| `develop` | `.github/workflows/deploy-staging.yml` | staging.calendary.com.ua | `:develop` | `calendary-staging` |

Each app stack (`deploy/docker-compose.prod.yml`, `deploy/docker-compose.staging.yml`) has its
**own** MSSQL container/volume/password — fully isolated data — and joins a shared external
Docker network (`web`) only through its `frontend` service (aliased `frontend-prod` /
`frontend-staging`), which is what the edge Caddy (`deploy/docker-compose.edge.yml` +
`deploy/Caddyfile`) reverse-proxies to. Nothing but Caddy (80/443) is exposed to the host;
`mssql`/`backend` stay internal-only in both stacks, same as local dev.

Given the droplet's small RAM budget, both MSSQL instances are memory-capped
(`MSSQL_MEMORY_LIMIT_MB`: 768 prod / 512 staging) and a 2GB swap file is provisioned as a safety
margin — this is a demo/hobby-scale box, not a sizing recommendation.

**One-time droplet setup:**

```bash
ssh root@207.154.222.66 'bash -s' < deploy/bootstrap.sh
# then edit /opt/calendary/.env (prod + edge) and /opt/calendary/.env.staging,
# setting real, *different* MSSQL_SA_PASSWORD values in each
```

**DNS** (DigitalOcean → Networking → Domains → `calendary.com.ua`): `A` records for `@` and
`staging`, both → `207.154.222.66`.

**GitHub Actions secrets** (shared by both workflows — same droplet):

| Secret | Value |
| --- | --- |
| `DEPLOY_HOST` | `207.154.222.66` |
| `DEPLOY_USER` | `root` |
| `DEPLOY_SSH_KEY` | private key matching an authorized key on the droplet |
| `GOOGLE_CLIENT_ID` | OAuth Web client ID from the `calendary-ua` GCP project |
| `GOOGLE_CLIENT_SECRET` | matching OAuth client secret |
| `RESEND_API_KEY` | Resend API key for transactional email |

`GITHUB_TOKEN` (built-in) handles both pushing images to GHCR and the droplet's `docker login`
during deploy — no extra registry secret needed. Unlike the AI provider keys (a manual one-off
`.env` edit), `GOOGLE_CLIENT_ID`/`GOOGLE_CLIENT_SECRET`/`RESEND_API_KEY` are genuinely threaded
through on every deploy: both `deploy.yml` and `deploy-staging.yml` upsert them into the droplet's
`.env`/`.env.staging` from the GH secret before `docker compose up` — rotate the secret in GH, the
next deploy picks it up automatically.

## Known gaps vs. the full design doc

- The design explored several layout **variants** per screen (mobile/desktop, alternate copy) —
  one variant was implemented per screen, not all of them.
- Refunds/returns ("повернення") are out of scope: cancellation is only allowed before payment,
  since payment is mocked and there's no real money to refund.
- The cover step here is a single generated image + confirm/regenerate, rather than the
  four-candidate picker grid shown in some design variants.
