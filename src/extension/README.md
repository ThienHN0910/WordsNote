# WordsNote Chrome Extension

Chrome extension for local-first Learn and Manage workflows.

## Scope

Popup workspaces:

- Learn Lab with 3 modes:
	- Flashcards
	- Learn (typed answer)
	- Practice (multiple choice)
- Manage Lab:
	- Local collection CRUD
	- Local card CRUD
	- Quick text import (`front|back|hint` per line)
- No authentication required.
- Optional Cloud Sync mode reads public collections/cards in read-only mode.
- Cards can be filtered by collection in both Local and Cloud modes.

## Data Model

- Cards are stored locally in `chrome.storage.local`.
- Storage key: `wordsnote_local_cards`.
- Collections are stored locally in `chrome.storage.local`.
- Storage key: `wordsnote_local_collections`.
- Highlighted text from webpages is saved as local cards by content script + background worker.
- Cloud Sync endpoint key in storage: `wordsnote_cloud_api_base_url`.

## Local Development

```bash
npm install
npm run build
```

Load unpacked extension from `src/extension/dist` in Chrome.

## Notes

- The extension is intentionally independent from web app auth state.
- The popup does not depend on web routes or web login state.
- In Cloud Sync mode, extension reads from public endpoints only:
	- `GET /api/collections`
	- `GET /api/cards`
- Cloud cards are read-only in popup; spaced repetition review updates remain local-only.
- Cloud mode provides `Sync To Local` to fetch cards and store them into local storage for Local Due learning.
