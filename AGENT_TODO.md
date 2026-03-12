# AGENT TODO (Backlog)

## Auth Migration (Supabase + Google)

- [x] Create Supabase free project and enable Google provider.
- [x] Fill frontend env: `VITE_SUPABASE_URL`, `VITE_SUPABASE_ANON_KEY`, `VITE_SUPABASE_REDIRECT_URL`.
- [x] Fill backend config: `SupabaseAuth:Enabled=true`, `SupabaseAuth:Authority`, `SupabaseAuth:Audience`.
- [ ] Verify `/api/auth/me` works with Supabase Bearer token.
- [x] Remove old local JWT login path in backend/client/docs.

## Security / Config

- [ ] Rotate leaked MongoDB credentials and JWT secrets.
- [ ] Rotate leaked MongoDB credentials and Supabase keys if exposed.
- [ ] Move secrets to env/user-secrets/secret manager.
- [ ] Add startup config validation for required auth/db settings.
- [ ] Add secret scanning in CI.

## Backend Quality

- [ ] Add `/health` endpoint and Mongo connectivity check.
- [ ] Add integration tests for auth and protected endpoints.
- [ ] Add structured error format for auth failures.
- [ ] Add rate limiting on auth-sensitive routes.

## Frontend Quality

- [ ] Add auth callback/loading state UX.
- [ ] Handle session refresh and logout edge cases.
- [ ] Add e2e test for login + protected route access.

## Docs / Ops

- [ ] Keep `AGENT_CONTEXT.md` updated after auth/config changes.
- [ ] Update `README.md` for Supabase setup and local run matrix.
- [ ] Add troubleshooting section (401, wrong issuer/audience, CORS).
