# WordsNote Workflow Guide

This guide explains the functional workflow for the WordsNote product:

- Web app with split access:
  - Public learning space (no sign-in)
  - Local-first management workspace (no sign-in required)
  - Authenticated focused session workflow for cloud-backed study actions
- Chrome extension (no authentication)

## 1. Main User Flow (Web)

1. Open `/learn` and use learning modes immediately (no auth).
2. Open `/privacy-policy` to review legal/privacy content.
3. Switch policy language by query parameter (`?lang=vi` or `?lang=en`) for shareable links.
4. Open `/manage` to create collections/cards in local-first mode.
5. Optional: sign in with Google at `/login` when you need cloud sync or focused session APIs.
6. Start focused session from `/manage/:deckId/session` after login.
7. Review progress and continue spaced repetition.

## 2. Authentication Policy

- Register route is disabled.
- Login uses Google ID token verification.
- Only one configured email (`AuthProviders:Google:AdminEmail`) is allowed.
- Cloud-backed management APIs require JWT returned by Google login endpoint.
- Local-first manage workflows can run without login on web and desktop clients.

## 3. Public Learning Modes (No Auth)

The public page `/learn` provides:

- Flashcards mode
- Learn mode (typed answer checking)
- Practice mode (quick multiple-choice)

These modes read dynamic collections/cards through public read APIs and do not require sign-in.

## 4. Collection Management (Local-First + Optional Cloud Sync)

Main actions:

- Create collection
- Edit collection title/description
- Delete collection

Expected behavior:

- Deleting a collection also removes its cards.
- Collection list is sorted by most recently updated.
- `Sync Cloud -> Local` copies cloud snapshot into local storage.
- `Sync Local -> Cloud` uploads local snapshot and requires login.

## 5. Card Management (Local-First + Optional Cloud Sync)

Main actions:

- Create card
- Edit card
- Delete card
- Import cards from text

Card fields:

- front (required)
- back (required)
- hint (optional)
- tags (optional)

Card UX policy:

- Create form is create-only.
- Existing cards are edited from the card action popup/dialog.

Import rules:

- Format: `front:back` per line
- Empty or invalid lines are skipped
- API returns `imported` and `skipped` counts

## 6. Manage Workspace Study Modes

### Flashcard

- Show front first, reveal back on demand
- Display card order indicator (`Card X / Y`)
- Allow previous/next card navigation in learn lab flashcards
- User grades difficulty: hard/medium/easy
- Due date and streak are updated

### Quick Study

- Prioritizes due cards
- Optimized for short review rounds

### Deep Study

- Full focused session over collection cards
- Includes answer check endpoint for typed responses
- Uses normalized answer comparison

## 7. Test Modes

### Multiple-Choice

- Start a session with configurable question count
- Submit selected options for each question
- Receive score and per-question results

### Written

- Start a session and answer free-text prompts
- Submit all answers
- Grading defaults:
  - diacritics-insensitive
  - case-insensitive
  - whitespace-normalized

## 8. Chrome Extension Workflow (No Auth)

Extension goals:

- Save highlighted words/phrases quickly
- Keep local inbox cards in `chrome.storage.local`
- Review due local cards in popup Learn Lab modes:
  - Flashcards
  - Learn (typed answer)
  - Practice (multiple choice)
- Manage collections/cards in popup Manage Lab (local CRUD + local text import)
- Optional cloud sync: read public collections/cards for Learn mode and `Sync To Local`
- Optional cloud write: `Sync Local -> Cloud` using saved cloud JWT token
- Collection-level filtering is available in both Local and Cloud modes.
- `Sync To Local` in Cloud mode fetches cards and stores them into local extension storage for Local Due study.

Important:

- Extension should work without login.
- Extension cloud token is optional and independent from web login state.
- Extension popup must not include homepage route navigation.
- Scope rule: web app owns route pages (`/`, `/learn`, `/manage`); extension owns popup Learn/Manage labs.
- Without cloud token, extension Cloud mode is read-only (`GET /api/collections`, `GET /api/cards`).
- With cloud token, extension can run `Sync Local -> Cloud` via collections/cards write endpoints.

## 9. Privacy Policy Page

Public route:

- `/privacy-policy`

Language behavior:

- `?lang=vi` renders Vietnamese policy copy.
- `?lang=en` renders English policy copy.
- Query parameter is kept in URL so policy links can be shared with a fixed language.

## 10. Operational Notes

- Keep API naming consistent: `collections` and `cards`.
- Maintain temporary compatibility with legacy `desk/card` routes during migration.
- Remove compatibility routes only after web + extension clients are migrated.

## 11. Troubleshooting

### 401 responses on management routes

- Verify Google login completed and JWT is stored.
- Verify request includes `Authorization: Bearer <token>`.

### Import result shows many skipped lines

- Confirm input follows `front:back` format.

### Written answers marked wrong unexpectedly

- Confirm expected answer exists in card back content.
- Check punctuation and extra text handling in current normalization rules.

### Cloud sync in extension does not return cards

- Verify extension cloud endpoint is reachable and returns `GET /api/collections` and `GET /api/cards`.
- Verify selected collection has cards when using collection filter.
- Retry `Refresh` in Cloud mode, then `Sync To Local`.
