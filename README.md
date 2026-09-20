# WordsNote

> 🌐 **Project Showcase & Portfolio:** [https://thienhn0910.vercel.app/projects/words-note](https://thienhn0910.vercel.app/projects/words-note)

WordsNote is a modern, high-performance learning platform for vocabulary, flashcards, and quiz examination, delivered across multiple platforms:

- **ASP.NET Core Backend API**: High-throughput REST API with MongoDB persistence
- **Vue 3 Web App**: Interactive learning desk with quiz bank & test simulator
- **WPF Desktop App**: Native Windows desktop experience (.NET 8) with Windows Store packaging
- **Browser Extension**: Edge & Chrome MV3 popup for quick review & instant vocabulary capture

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
  - Cloud/Sync quick actions in Manage:
    - Sync Cloud -> Local copies cloud collections/cards into browser local storage
    - Sync Local -> Cloud uploads local collections/cards to cloud (requires login)
- Download route: /download
  - Public read uses GET /api/download-config (anonymous access allowed)
  - Edit and reset use PUT/DELETE /api/download-config and require admin JWT
  - Caller must be admin (role Admin or configured admin email)
- Auth-required focused session route: /manage/:deckId/session
- Google sign-in is optional and used for cloud-backed session/deep-study actions

### Quiz Platform (Web & Desktop)

- Public quiz routes: `/quizzes`, `/quizzes/:bankId`
- Curated MLN122 question bank with 2,254 verified questions across philosophical & socio-political topics
- **Practice Mode**: Instant feedback, explanations, and randomized choices
- **Exam Simulation**: Timed quiz sessions with detailed scoring breakdown
- **Vocabulary Capture**: One-click capture of new words from questions directly into the Flashcard Desk
- **Local-First Architecture**: Seamless offline support with automatic caching and zero latency

### Browser Extension

- Local-first popup workspaces:
  - Learn Lab (flashcards, learn, practice)
  - Manage Lab (local collection/card CRUD + text import)
- Cloud token is optional and persisted locally in extension storage
- Manage remains local-first; cloud write is available through explicit sync action
- Learn popup modes:
  - Flashcards
  - Learn
  - Practice
- Optional Cloud mode for public read
- Sync To Local flow to copy cloud cards into local review dataset
- Sync Local -> Cloud flow (requires saved JWT token)
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
- Download Config
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
- docs/RELEASE_NOTES.md

## Latest Release

- **Current: WordsNote v1.2.0 (20/09/2026)**
  - ✨ **Hybrid Quiz Platform**: Integrated 2,254 MLN122 questions with practice, mock exam, and instant word-capture to flashcards.
  - 🚀 **Automated Edge Add-on Publishing**: Direct store deployment via Microsoft Edge Add-ons Publish API v1.1.
  - 📦 **Desktop Store Packaging**: Windows 11 SDK 10.0.22621.0 support and Visual Studio Packaging Wizard alignment.
  - 🌐 **Showcase & Portfolio**: Featured at [https://thienhn0910.vercel.app/projects/words-note](https://thienhn0910.vercel.app/projects/words-note).
- Previous: WordsNote v1.1.3 (21/04/2026)
- See [docs/RELEASE_NOTES.md](file:///E:/workspace/srcPrj/WordsNote/docs/RELEASE_NOTES.md) for full release details.
- Local packaged assets: `release/microsoft-edge-addon/WordsNote-Edge-Addon-v1.2.0.zip` & `src/desktop/WordsNote.Package/AppPackages/`.

## Build Status (Latest)

Recent builds executed successfully for:

- Frontend
- Extension
- Desktop

## Release and Deployment Notes

- Frontend can be deployed with Vercel or equivalent static hosting.
- Backend should be deployed with secure runtime secrets and MongoDB connectivity.
- Extension package can be published to Edge Add-ons and Chrome Web Store.

## Featured Links & Portfolio
 
- 🌐 **Project Showcase & Portfolio**: [https://thienhn0910.vercel.app/projects/words-note](https://thienhn0910.vercel.app/projects/words-note)
- 🛒 **Microsoft Edge Add-on**: [WordsNote on Edge Add-ons Store](https://microsoftedge.microsoft.com/addons/detail/wordsnote/clocbppplpjhgkhcjhggljebocijhkpk)
- 💻 **Desktop App**: Packaged via MSIX for Microsoft Store

## License

No license file is currently defined in repository root.
Add a LICENSE file if you plan public distribution.
