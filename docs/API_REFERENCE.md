# API Reference

Base path: `/api`

## Authentication

Backend-protected APIs require JWT:

```http
Authorization: Bearer <jwt_token>
```

Error conventions:

- `401`: token missing or invalid
- `403`: insufficient permissions (if role checks are enabled)

## Health

- `GET /api/health` (recommended)

Response:

```json
{ "success": true, "status": "ok" }
```

## Auth

- `POST /api/auth/register`
- `POST /api/auth/login`

## User

- `GET /api/user/me`
- `PUT /api/user/me`

## Collections

Primary routes:

- `GET /api/collections`
- `POST /api/collections`
- `PUT /api/collections/{id}`
- `DELETE /api/collections/{id}`

Collection payload:

- `title`: string (required)
- `description`: string (optional)

## Cards

Primary routes:

- `GET /api/cards?collectionId={id}`
- `GET /api/cards/due?collectionId={id}`
- `POST /api/cards`
- `PUT /api/cards/{id}`
- `DELETE /api/cards/{id}`
- `POST /api/cards/import`
- `POST /api/cards/{id}/review`

Card upsert payload:

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

Review payload:

- `difficulty`: `hard | medium | easy`

## Study

### Quick Study

- `GET /api/study/quick?collectionId={id}&limit={n}`

Returns due cards first, optimized for fast review.

### Review (mode-independent)

- `POST /api/study/review`

Payload:

- `cardId`: string
- `difficulty`: `hard | medium | easy`

### Deep Study Session

- `GET /api/study/deep/session?collectionId={id}`

Returns full session cards and basic metrics for focused study.

### Deep Study Answer Check

- `POST /api/study/deep/answer`

Payload:

- `cardId`: string
- `answer`: string

Default grading rule:

- Ignore diacritics
- Case-insensitive
- Normalize whitespace

## Tests

### Multiple Choice

- `POST /api/tests/mcq/start`
- `POST /api/tests/mcq/submit`

`start` payload:

- `collectionId`: string
- `questionCount`: int (optional)
- `optionCount`: int (optional)

`submit` payload:

- `sessionId`: string
- `answers`: array of `{ questionId, selectedOptionIndexes[] }`

### Written

- `POST /api/tests/written/start`
- `POST /api/tests/written/submit`

`start` payload:

- `collectionId`: string
- `questionCount`: int (optional)

`submit` payload:

- `sessionId`: string
- `answers`: array of `{ questionId, answer }`

Written grading uses the same normalization rules as deep-study answer check.

## Compatibility Routes (temporary)

During migration, old route names remain available for compatibility:

- `/api/desk` maps to collections behavior
- `/api/card` maps to cards behavior
- old payload fields (`deskId`, `deckId`) are accepted

These compatibility routes are planned for removal after frontend and extension migration is complete.
