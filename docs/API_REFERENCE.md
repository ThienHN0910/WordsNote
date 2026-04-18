# API Reference

Base path: `/api`

## Product Access Model

- Public learning experience (`/learn` in frontend) does not require authentication.
- Learn reads collections/cards via public read endpoints.
- Collection and card write operations for management require JWT authentication.
- Public legal page is available at frontend route `/privacy-policy` with language query `?lang=vi|en`.
- Extension Cloud mode uses only public read endpoints (`GET /api/collections`, `GET /api/cards`) and syncs results to local storage client-side.
- WPF desktop app mirrors frontend feature scope:
	- Learn workspace uses public read endpoints.
	- Manage workspace supports local offline mode when not logged in.
	- After Google login, Manage can sync local data to cloud and use protected endpoints.
	- Manage also supports cloud-to-local sync for offline continuity.
	- Desktop supports Google browser login (preferred) and manual ID token input (fallback).

Protected requests must include:

```http
Authorization: Bearer <jwt_token>
```

## Auth

### Google login (primary)

- `POST /api/auth/google`

Payload:

```json
{ "idToken": "<google_id_token>" }
```

Behavior:

- Backend verifies Google token audience against `AuthProviders:Google:ClientId`.
- Backend only allows one configured email from `AuthProviders:Google:AdminEmail`.
- On success, backend returns JWT string for protected APIs.

### Username/password login (legacy, not used by frontend)

- `POST /api/auth/login`

### Register

- `POST /api/auth/register`
- Status: disabled by product policy.
- Returns `410 Gone` with message to use Google login.

## User

- `GET /api/user/me`
- `PUT /api/user/me`

## Collections

- `GET /api/collections` (public)
- `POST /api/collections`
- `PUT /api/collections/{id}`
- `DELETE /api/collections/{id}`

Payload:

- `title`: string (required)
- `description`: string (optional)

## Cards

- `GET /api/cards?collectionId={id}` (public)
- `GET /api/cards/due?collectionId={id}` (public)
- `POST /api/cards`
- `PUT /api/cards/{id}`
- `DELETE /api/cards/{id}`
- `POST /api/cards/import`

Card payload:

- `collectionId`: string (required)
- `front`: string (required)
- `back`: string (required)
- `hint`: string (optional)
- `tags`: string[] (optional)

Import payload:

- `collectionId`: string (required)
- `rawText`: string (required)

Import format:

- One card per line
- Separator `:` between front and back
- Invalid lines are skipped and counted

## Study (protected manage mode)

- `GET /api/study/quick?collectionId={id}&limit={n}`
- `POST /api/study/review`
- `GET /api/study/deep/session?collectionId={id}`
- `POST /api/study/deep/answer`

`POST /api/study/review` payload:

- `cardId`: string
- `difficulty`: `hard | medium | easy`

`POST /api/study/deep/answer` payload:

- `cardId`: string
- `answer`: string

Answer grading default:

- Diacritics-insensitive
- Case-insensitive
- Whitespace-normalized

## Tests (protected manage mode)

- `POST /api/tests/mcq/start`
- `POST /api/tests/mcq/submit`
- `POST /api/tests/written/start`
- `POST /api/tests/written/submit`

Desktop note:

- Backend Tests APIs remain active; current desktop UI does not expose a dedicated Tests tab.

MCQ start payload:

- `collectionId`: string
- `questionCount`: int (optional)
- `optionCount`: int (optional)

MCQ submit payload:

- `sessionId`: string
- `answers`: array of `{ questionId, selectedOptionIndexes[] }`

Written start payload:

- `collectionId`: string
- `questionCount`: int (optional)

Written submit payload:

- `sessionId`: string
- `answers`: array of `{ questionId, answer }`

## Compatibility Routes (temporary)

- `/api/desk` maps to collections behavior
- `/api/card` maps to cards behavior
- Legacy payload fields (`deskId`, `deckId`) are accepted during migration

## Error Semantics

- `401 Unauthorized`: missing/invalid token on protected write/management endpoints.
- `503 Service Unavailable`: backend dependent data service unavailable (commonly MongoDB connectivity/runtime configuration issue).

When troubleshooting `503` in deployment:

1. Verify backend host itself returns non-503 for `GET /api/collections`.
2. Verify backend runtime secrets are real values (not placeholders).
3. Verify backend can connect to MongoDB from that hosting environment.
