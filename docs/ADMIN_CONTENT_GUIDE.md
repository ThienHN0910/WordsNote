# WordsNote Workflow Guide

This guide explains the functional workflow for the WordsNote product:

- Web app with split access:
  - Public learning space (no sign-in)
  - Authenticated management workspace
- Chrome extension (no authentication)

## 1. Main User Flow (Web)

1. Open `/learn` and use learning modes immediately (no auth).
2. For personal collection management, sign in with Google at `/login`.
3. In `/manage`, create collections and cards.
4. Start focused session from manage workspace.
5. Review progress and continue spaced repetition.

## 2. Authentication Policy

- Register route is disabled.
- Login uses Google ID token verification.
- Only one configured email (`AuthProviders:Google:AdminEmail`) is allowed.
- Management APIs require JWT returned by Google login endpoint.

## 3. Public Learning Modes (No Auth)

The public page `/learn` provides:

- Flashcards mode
- Learn mode (typed answer checking)
- Practice mode (quick multiple-choice)

These modes read dynamic collections/cards through public read APIs and do not require sign-in.

## 4. Collection Management (Auth Required)

Main actions:

- Create collection
- Edit collection title/description
- Delete collection

Expected behavior:

- Deleting a collection also removes its cards.
- Collection list is sorted by most recently updated.

## 5. Card Management (Auth Required)

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
- Review due local cards in popup with Learn-only modes:
  - Flashcards
  - Learn (typed answer)
  - Practice (multiple choice)
- Optional cloud sync: read public collections/cards for Learn-only practice (read-only)

Important:

- Extension should work without login.
- Extension should not depend on web JWT token.
- Extension popup must not include homepage or manage navigation.
- Scope rule: web app owns `/` and `/manage`; extension owns local Learn-only flow.
- Cloud sync mode only calls public read endpoints (`GET /api/collections`, `GET /api/cards`) and must not call management/study write APIs.

## 9. Operational Notes

- Keep API naming consistent: `collections` and `cards`.
- Maintain temporary compatibility with legacy `desk/card` routes during migration.
- Remove compatibility routes only after web + extension clients are migrated.

## 10. Troubleshooting

### 401 responses on management routes

- Verify Google login completed and JWT is stored.
- Verify request includes `Authorization: Bearer <token>`.

### Import result shows many skipped lines

- Confirm input follows `front:back` format.

### Written answers marked wrong unexpectedly

- Confirm expected answer exists in card back content.
- Check punctuation and extra text handling in current normalization rules.
