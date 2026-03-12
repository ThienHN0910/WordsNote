# AGENT CONTEXT - WordsNote

This file is a compact handoff so the next agent can continue coding without scanning the entire repo.

## 1) Project Snapshot

- Monorepo with 3 apps:
  - `src/backend`: ASP.NET Core Web API (.NET 8, DDD-ish layers)
  - `src/frontend`: Vue 3 + Vite + Pinia + Tailwind
  - `src/extension`: Chrome extension (Vue + Vite)
- Solution file for backend: `src/backend/WordsNote.slnx`

## 2) Backend Structure (important)

- `WordsNote.API`: controllers, auth, startup wiring
- `WordsNote.Application`: CQRS (MediatR), DTOs, mappings
- `WordsNote.Domain`: entities, domain events, interfaces
- `WordsNote.Infrastructure`: MongoDB context/repositories, infra services

## 3) Commands You Actually Need

### Build backend

```powershell
cd src/backend
dotnet build .\WordsNote.slnx -c Debug
```

### Run backend (local HTTP)

```powershell
cd src/backend
dotnet run --project .\WordsNote.API -c Debug --urls "http://localhost:5251"
```

- Swagger: `http://localhost:5251/swagger`

### Frontend

```powershell
cd src/frontend
npm install
npm run dev
```

### Extension

```powershell
cd src/extension
npm install
npm run build
```

## 4) Runtime + Config Facts

- Launch profile ports in `WordsNote.API/Properties/launchSettings.json`:
  - HTTP: `http://localhost:5251`
  - HTTPS: `https://localhost:7137`
- Backend auth is Supabase-only (`SupabaseAuth.Enabled=true`).
- Authority format must be: `https://<project-ref>.supabase.co/auth/v1`.
- Audience should be `authenticated` for standard Supabase access tokens.
- `MongoDb.ConnectionString` is currently hardcoded in `appsettings.json` (security risk for production).

## 5) Known Fixed Bug (do not regress)

- File: `src/backend/WordsNote.Infrastructure/Extensions/InfrastructureServiceExtensions.cs`
- Problem that happened:
  - MongoDB serialization threw:
  - `The memberInfo argument must be for class Card, but was for class BaseEntity`
- Root cause:
  - `Id` belongs to `BaseEntity` but was mapped again in derived class maps (`Card`, `Deck`).
- Fix applied:
  - Register class map for `BaseEntity` and map `Id` there.
  - Keep `Card` / `Deck` maps without remapping `Id`.

## 6) API Behavior Notes

- `401 Unauthorized` on protected endpoints is expected when no JWT is provided.
- Root path `/` returns `404` (normal because only API routes + swagger are mapped).
- Frontend now signs out Supabase session on `401` to prevent redirect flicker loop.

## 7) Security Notes (must address soon)

- `src/backend/WordsNote.API/appsettings.json` is in `.gitignore`, but if it was committed in history, credentials are still exposed in git history.
- Recommended next action:
  - Rotate MongoDB credential and Supabase credentials if leaked.
  - Move secrets to env vars / user-secrets / secret manager.

## 8) High-Value Next Tasks

1. Replace hardcoded MongoDB values with placeholders in repo-safe config.
2. Add startup validation for required settings (`SupabaseAuth:Authority`, `MongoDb:ConnectionString`, etc.).
3. Add a simple health endpoint (`/health`) and database connectivity check.
4. Add integration test for auth flow (`/api/auth/me` + protected endpoint).
5. Keep this file updated whenever architecture/config changes.

## 9) Fast Read Order For Next Agent

If time/token is limited, read in this order:

1. `AGENT_CONTEXT.md` (this file)
2. `src/backend/WordsNote.API/Program.cs`
3. `src/backend/WordsNote.Infrastructure/Extensions/InfrastructureServiceExtensions.cs`
4. `src/backend/WordsNote.API/appsettings.Development.json`
5. `README.md`
