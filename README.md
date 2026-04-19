# WordsNote

WordsNote is a learning platform for vocabulary and flashcards, delivered as:

- ASP.NET Core backend API
- Vue 3 web app
- WPF desktop app
- Browser extension popup for quick review

## Project Layout

- docs
  - Product and technical documentation
- src/backend
  - .NET backend, API, domain, infrastructure
- src/frontend
  - Vue 3 + Vite web application
- src/desktop
  - WPF desktop application (.NET 8)
- src/extension
  - Extension popup and browser scripts

## Core Product Flow

### Web App

- Public learning route: /learn
- Public privacy route: /privacy-policy with language query mode
  - /privacy-policy?lang=vi
  - /privacy-policy?lang=en
- Local-first management route: /manage (no login required, browser local storage)
  - Card UX: create form is create-only; editing runs in popup modal from card actions
- Auth-required focused session route: /manage/:deckId/session
- Google sign-in is optional and used for cloud-backed session/deep-study actions

### Browser Extension

- Local-first popup workspaces:
  - Learn Lab (flashcards, learn, practice)
  - Manage Lab (local collection/card CRUD + text import)
- No authentication required
- Manage is local-only (no cloud write)
- Learn popup modes:
  - Flashcards
  - Learn
  - Practice
- Optional Cloud mode (read-only fetch from public API)
- Sync To Local flow to copy cloud cards into local review dataset
- Collection-level filtering in popup

### Desktop App (WPF)

- Home/Landing workspace
- Dedicated Login workspace (separate from Manage)
- Learn workspace:
  - Flashcards
  - Learn (typed answer with normalized compare)
  - Practice (multiple choice)
- Privacy policy workspace (VI/EN) hidden from main tabs and opened from landing link/button
- Manage workspace:
  - Local mode without login: CRUD collections/cards, import, filter/sort (no backend calls)
  - Card UX: create form is create-only; edit opens popup dialog via Edit Selected
  - Google login for cloud mode (browser flow with optional ID token fallback)
  - Sync Local -> Cloud and Sync Cloud -> Local
  - Input placeholders for faster desktop data entry

## Technology Stack

- Backend: .NET 8, ASP.NET Core, MongoDB
- Frontend: Vue 3, Pinia, Vue Router, Vite
- Extension: Vue 3, Vite, Chrome Extension Manifest V3

## Prerequisites

- Node.js 20+
- npm
- .NET SDK 8+
- MongoDB connection for backend runtime

## Local Development

### 1. Backend

From repository root:

- dotnet build src/backend/FeatureFusion.sln
- dotnet run --project src/backend/FeatureFusion/FeatureFusion.csproj

Required config can be provided through appsettings, user-secrets, or environment variables.
See docs/ENVIRONMENT.md for details.

### 2. Frontend

- cd src/frontend
- npm install
- npm run dev

Production build:

- npm run build

### 3. Extension

- cd src/extension
- npm install
- npm run build

Load unpacked extension from src/extension/dist.

### 4. Desktop (WPF)

- dotnet build src/desktop/WordsNote.Desktop.sln
- dotnet run --project src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj

Desktop defaults are loaded from `src/desktop/WordsNote.Desktop/appsettings.json`.
Override values by environment variables with prefix `WORDSNOTE_` (for example `WORDSNOTE_Desktop__ApiBaseUrl`).
For MSIX packaging (`src/desktop/WordsNote.Package`), build from Visual Studio with Windows packaging tooling installed.

Note:
Current extension host permissions are configured for words-note.runasp.net.
If you need a different API host for Cloud mode, update manifest host permissions accordingly.

## API Overview

Base path:

- /api

Main resource groups:

- Auth
- Collections
- Cards
- Study
- Tests

Detailed contracts are documented in docs/API_REFERENCE.md.

## Documentation Index

- docs/ENVIRONMENT.md
- docs/API_REFERENCE.md
- docs/ADMIN_CONTENT_GUIDE.md
- docs/AGENT_REPLICATION_PLAYBOOK.md
- docs/WORDSNOTE_EXECUTION_PLAN.md
- docs/WPF_DESKTOP_EXECUTION_PLAN.md
- docs/MSIX_STORE_PREP.md

## Build Status (Latest)

Recent builds executed successfully for:

- Frontend
- Extension
- Desktop

## Release and Deployment Notes

- Frontend can be deployed with Vercel or equivalent static hosting.
- Backend should be deployed with secure runtime secrets and MongoDB connectivity.
- Extension package can be published to Edge Add-ons and Chrome Web Store.

## License

No license file is currently defined in repository root.
Add a LICENSE file if you plan public distribution.
