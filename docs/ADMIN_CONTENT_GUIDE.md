# WordsNote Workflow Guide

This guide explains the functional workflow for the WordsNote product:

- Web app (authenticated)
- Chrome extension (no authentication)

## 1. Main User Flow (Web)

1. Register or login.
2. Create a collection.
3. Add cards manually or import in bulk.
4. Start learning:
   - Flashcard mode
   - Quick study mode
   - Deep study mode
5. Run a test session:
   - Multiple-choice
   - Written
6. Review results and continue spaced repetition.

## 2. Collection Management

Main actions:

- Create collection
- Edit collection title/description
- Delete collection

Expected behavior:

- Deleting a collection also removes its cards.
- Collection list is sorted by most recently updated.

## 3. Card Management

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

## 4. Learning Modes

### Flashcard

- Show front first, reveal back on demand
- User grades difficulty: hard/medium/easy
- Due date and streak are updated

### Quick Study

- Prioritizes due cards
- Optimized for short review rounds

### Deep Study

- Full focused session over collection cards
- Includes answer check endpoint for typed responses
- Uses normalized answer comparison

## 5. Test Modes

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

## 6. Chrome Extension Workflow (No Auth)

Extension goals:

- Save highlighted words/phrases quickly
- Keep local inbox cards in `chrome.storage.local`
- Review due local cards in popup

Important:

- Extension should work without login.
- Extension should not depend on web JWT token.

## 7. Operational Notes

- Keep API naming consistent: `collections` and `cards`.
- Maintain temporary compatibility with legacy `desk/card` routes during migration.
- Remove compatibility routes only after web + extension clients are migrated.

## 8. Troubleshooting

### 401 responses on web app

- Verify login token is present.
- Verify backend `JwtSettings` is valid.

### Import result shows many skipped lines

- Confirm input follows `front:back` format.

### Written answers marked wrong unexpectedly

- Confirm expected answer exists in card back content.
- Check punctuation and extra text handling in current normalization rules.
