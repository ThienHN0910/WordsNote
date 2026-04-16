# API Reference

Base path: `/api`

## Product Access Model

- Public learning experience (`/learn` in frontend) does not require authentication.
- Learn reads collections/cards via public read endpoints.
- Collection and card write operations for management require JWT authentication.

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
