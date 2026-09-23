# WordsNote

> 🌐 **Project Showcase & Portfolio:** [https://thienhn.io.vn/projects/words-note](https://thienhn.io.vn/projects/words-note)

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

### Quiz & Examination Workspace (`/quiz`)

Interactive study & test simulation platform with complete design and interaction parity with `MLN122_FE`:

- **Workspace Routes**: `/quiz`, `/quiz/:subjectId`
- **Multi-Subject Question Banks (4,364 verified questions across 7 subjects)**:
  - **MLN122**: Kinh tế chính trị Mác–Lênin (598 câu) — *Mở tự do*
  - **PRM393**: Flutter / Mobile (2 đề FE + 12 slide - 224 câu) — *Mở tự do*
  - **PRN232**: Lập trình .NET & Web API (FE SP26, B5 FE, FA25 FE & PE - 150 câu) — *Mở tự do*
  - **ITE302c**: Ethics in Information Technology (12 bộ đề FE SP24-SP26 - 1,060 câu) — *Mở tự do*
  - **HCM202**: Tư tưởng Hồ Chí Minh (15 bộ đề FE SU24-SU26 - 900 câu) — *Mở tự do*
  - **JFE301**: IT Fundamentals (FE + textbook + ôn thêm - 726 câu) — *Môn khóa VIP, yêu cầu mã mở khóa 1 lần*
  - **JIT401**: JIT401 (FE + slide + Quiz on Thao + albazzz PT - 706 câu) — *Môn khóa VIP, yêu cầu mã mở khóa 1 lần*
- **Study UX & Interaction Logic**:
  - **Immediate Evaluation**: Instant scoring for single-choice questions; dedicated "Kiểm tra" submission for multi-choice.
  - **Comprehensive Explanations**: Detailed explanation callout banners with specific styling for incorrect answers.
  - **Side Question Map (`SideQuestionMap`)**: Sticky 60fps grid visualizing question status (current, correct, wrong, unanswered) with star markers.
  - **Mode & Source Filters**: Switch between All, Wrong only, Unanswered only, Starred (★), and Shuffle mode; filter by exam source (Đề FE, Ôn thêm, Quiz ôn).
  - **Keyboard Navigation**: Full shortcuts (`A`/`B`/`C`/`D` or `1`/`2`/`3`/`4` to answer, `←`/`A` for Previous, `→`/`D` for Next, `S` to Star).
  - **Search Drawer (`SearchDrawer`)**: Real-time fuzzy question search with instant jump.
  - **SRS Flashcard Capture**: One-click `+ Lưu từ` button to extract vocabulary/concepts into WordsNote study decks.
- **Security & Admin Portal Management**:
  - **Zero-Trust Identity**: Role-based access control strictly enforced via Google OAuth (`ADMIN_EMAIL=hnt.vn.vn@gmail.com`).
  - **Zero Bypass Vulnerabilities**: Static passwords and header bypasses (`x-admin-secret`, `x-user-email`) completely eliminated.
  - **Admin Console (`AdminModal`)**: Protected behind Google Admin authentication with 3 management modules:
    - **Mã mở khóa**: Tạo mã 16 ký tự dùng 1 lần (`Convert.ToHexString(8 bytes)`), theo dõi lượt kích hoạt và người dùng.
    - **Học viên**: Cấp quyền mở khóa trực tiếp theo tài khoản Google email.
    - **Bộ môn (Quiz Sets)**: Thêm mới, chỉnh sửa thông tin, xóa bộ môn, và bật/tắt tức thì trạng thái Khóa VIP / Mở tự do (`isRestricted`).
  - **Atomic Key Redemption**: Concurrency-safe MongoDB atomic `FindOneAndUpdate` eliminates race conditions.
  - **Brute-Force Rate Limiting**: Automatic 5-minute IP/User throttle after 5 consecutive failed unlock attempts (HTTP 429).

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

- Auth (`/api/auth`)
- Download Config (`/api/download-config`)
- Collections (`/api/collections`)
- Cards (`/api/cards`)
- Study (`/api/study`)
- Tests (`/api/tests`)
- Quiz Sets & Examination (`/api/quiz-sets`, `/api/catalog`, `/api/questions/{id}`, `/api/unlock`, `/api/admin/keys`, `/api/admin/users`)

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

- **Current: WordsNote v1.3.0 (20/09/2026)**
  - 🎯 **Full Quiz Workspace Parity with MLN122_FE**: Complete port of responsive 2-column layout, sticky 60fps Question Map (`SideQuestionMap`), top subject navigation, practice controls (`QuizControls`), and real-time fuzzy search (`SearchDrawer`).
  - 📚 **2,254 Questions Across 4 Subject Banks**: MLN122 (598), PRM393 (224), JFE301 (726), and JIT401 (706).
  - 🔒 **Zero-Trust Access Control & Single-Use Unlock Keys**: Open subjects (MLN122, PRM393) vs restricted subjects (JFE301, JIT401). Single-use 16-character unlock code management via Admin Console.
  - 🛡️ **Security Hardening**: Elimination of static password bypasses and header spoofing, atomic redemption via MongoDB `FindOneAndUpdate`, and anti-brute-force rate limiting.
  - 💾 **Flashcard SRS Integration**: One-click `+ Lưu từ` to extract exam terms directly into WordsNote decks.
  - 🌐 **Showcase & Portfolio**: Featured at [https://thienhn.io.vn/projects/words-note](https://thienhn.io.vn/projects/words-note).
- Previous: WordsNote v1.2.0 (20/09/2026)
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
 
- 🌐 **Project Showcase & Portfolio**: [https://thienhn.io.vn/projects/words-note](https://thienhn.io.vn/projects/words-note)
- 🛒 **Microsoft Edge Add-on**: [WordsNote on Edge Add-ons Store](https://microsoftedge.microsoft.com/addons/detail/wordsnote/clocbppplpjhgkhcjhggljebocijhkpk)
- 💻 **Desktop App**: Packaged via MSIX for Microsoft Store

## License

No license file is currently defined in repository root.
Add a LICENSE file if you plan public distribution.
