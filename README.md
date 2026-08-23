# Calendary

Custom AI-generated photo calendar ordering app. A thin, end-to-end vertical slice built from the
`Calendary.dc.html` design ("Broadsheet" design system): docker + .NET + MSSQL + Angular.

## Flow implemented

Landing → passwordless start (email/phone) → photo upload → style + personal dates → generation
(live progress) → cover pick → month-by-month reveal (regenerate/failure/retry) → review →
delivery + payment (Nova Poshta + Apple/Google Pay/monobank/card) → order status (auto-progressing
Paid → Printing → Shipped → Delivered, with cancellation while unpaid).

## Stack

- **backend/** — ASP.NET Core (.NET 10) Web API, EF Core + SQL Server, split into
  `Calendary.Domain` / `Calendary.Infrastructure` / `Calendary.Api`.
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
- **Auth** — passwordless "magic link" tokens are minted and hidden in the API response itself
  (no real email/SMS/Google OAuth). Session tokens are an in-memory opaque bearer store
  (`DevAuthService`), which resets on backend restart — fine for a demo, not for production.
- **Order fulfillment timing** — `FulfillmentBackgroundService` advances Paid → Printing → Shipped
  → Delivered purely by elapsed wall-clock time (8s / 10s / 20s), not real print/courier events.

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

`GITHUB_TOKEN` (built-in) handles both pushing images to GHCR and the droplet's `docker login`
during deploy — no extra registry secret needed.

## Known gaps vs. the full design doc

- The design explored several layout **variants** per screen (mobile/desktop, alternate copy) —
  one variant was implemented per screen, not all of them.
- Refunds/returns ("повернення") are out of scope: cancellation is only allowed before payment,
  since payment is mocked and there's no real money to refund.
- The cover step here is a single generated image + confirm/regenerate, rather than the
  four-candidate picker grid shown in some design variants.
