# WordsNote Chrome Extension

Chrome extension for the Learn workflow only.

## Scope

- Learn-only popup experience with 3 modes:
	- Flashcards
	- Learn (typed answer)
	- Practice (multiple choice)
- No authentication required.
- No Manage workspace or Homepage flow inside extension popup.
- Optional Cloud Sync mode reads public collections/cards in read-only mode.

## Data Model

- Cards are stored locally in `chrome.storage.local`.
- Storage key: `wordsnote_local_cards`.
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
- The popup does not navigate to web routes such as `/` or `/manage`.
- In Cloud Sync mode, extension reads from public endpoints only:
	- `GET /api/collections`
	- `GET /api/cards`
- Cloud cards are read-only in popup; spaced repetition review updates remain local-only.
