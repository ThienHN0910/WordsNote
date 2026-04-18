# WPF Desktop Execution Plan

Date: 2026-04-18

## 1. Objective

Build a WPF desktop application with feature parity to active frontend web flows:

- Landing
- Learn Lab (Flashcards, Learn, Practice)
- Privacy Policy (VI/EN)
- Manage (local-first CRUD + cloud sync + browser Google login)

## 2. Scope Mapping (Frontend -> WPF)

1. LandingPage.vue -> Landing tab
2. LearnLabPage.vue -> Learn tab
3. PrivacyPolicyPage.vue -> Privacy tab
4. StudyHubPage.vue -> Manage tab
5. StudyStore + StudyAPI/AuthAPI -> MainViewModel + WordsNoteApiClient

## 3. Implementation Plan

1. Scaffold desktop solution and project
2. Implement API client and models
3. Implement state/viewmodel for learn/manage/privacy/auth
4. Build WPF UI tabs and bind actions
5. Compile and fix errors
6. Update documentation and execution plan

## 4. Execution Checklist

- [x] Created solution `src/desktop/WordsNote.Desktop.sln`
- [x] Created project `src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj`
- [x] Added API client for auth/collections/cards/study endpoints
- [x] Added desktop models for study data contracts
- [x] Added main viewmodel for full app state and flows
- [x] Implemented multi-tab UI for Landing/Learn/Privacy/Manage
- [x] Implemented Google login/logout flow for protected manage cloud mode
- [x] Implemented collection/card CRUD + import + filters/sorts
- [x] Implemented local-mode manage without login (local JSON persistence)
- [x] Implemented sync controls: Local -> Cloud and Cloud -> Local
- [x] Implemented browser-based Google login flow (Postman/UnityHub style)
- [x] Added placeholders for key desktop input fields
- [x] Build passed with `dotnet build src/desktop/WordsNote.Desktop.sln`
- [x] Updated docs (`README.md`, `docs/ENVIRONMENT.md`, `docs/API_REFERENCE.md`, `docs/WORDSNOTE_EXECUTION_PLAN.md`)

## 5. Risks and Next Steps

1. Suggested next increment:
   - Add persistent desktop settings for API base URL and Google client ID
   - Add integration tests for desktop API client and sync paths
   - Optionally reintroduce Session/Tests as on-demand modules if needed later
