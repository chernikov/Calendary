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

## Known gaps vs. the full design doc

- The design explored several layout **variants** per screen (mobile/desktop, alternate copy) —
  one variant was implemented per screen, not all of them.
- Refunds/returns ("повернення") are out of scope: cancellation is only allowed before payment,
  since payment is mocked and there's no real money to refund.
- The cover step here is a single generated image + confirm/regenerate, rather than the
  four-candidate picker grid shown in some design variants.
