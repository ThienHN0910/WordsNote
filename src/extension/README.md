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
- No mandatory login flow.
- Optional Cloud Sync mode reads public collections/cards and supports token-based `Sync Local -> Cloud`.
- Cards can be filtered by collection in both Local and Cloud modes.

## Data Model

- Cards are stored locally in `chrome.storage.local`.
- Storage key: `wordsnote_local_cards`.
- Collections are stored locally in `chrome.storage.local`.
- Storage key: `wordsnote_local_collections`.
- Highlighted text from webpages is saved as local cards by content script + background worker.
- Cloud Sync endpoint key in storage: `wordsnote_cloud_api_base_url`.
- Cloud auth token key in storage: `wordsnote_cloud_auth_token`.

## Local Development

```bash
npm install
npm run build
```

Load unpacked extension from `src/extension/dist` in Chrome.

## Notes

- The extension is intentionally independent from web app auth state.
- The popup does not depend on web routes or web login state.

- In Cloud Sync mode, extension reads from public endpoints:
	- `GET /api/collections`
	- `GET /api/cards`
- Cloud mode provides `Sync To Local` to fetch cards and store them into local storage for Local Due learning.
- Cloud mode provides `Sync Local -> Cloud` using saved JWT token (`Authorization: Bearer ...`).
