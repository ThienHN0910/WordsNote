# Environment Configuration

This document defines runtime configuration for the WordsNote monorepo.

## Project Layout

- Backend: `src/backend/FeatureFusion`
- Frontend: `src/frontend`
- Desktop: `src/desktop/WordsNote.Desktop`
- Chrome extension: `src/extension`

## Backend (.NET) Configuration

The backend uses `appsettings.json` and `appsettings.Development.json`.

Sensitive values must not be committed in `appsettings*.json`.
Use one of these:

1. `dotnet user-secrets` for local development
2. environment variables for production/deployment

Primary sections:

1. `JwtSettings`
2. `MongoDB`
3. `AllowedOrigins`

### JwtSettings

- `SecretKey`: signing key for JWT
- `Issuer`: token issuer
- `Audience`: token audience
- `ExpiresInMinutes`: token lifetime in minutes

### MongoDB

- `ConnectionString`: MongoDB connection string
- `DatabaseName`: target Mongo database

### AllowedOrigins

Array of frontend origins for CORS. Local dev usually includes:

- `http://localhost:5173`
- backend URL if needed for local integration tests

### Example (development)

```json
{
  "JwtSettings": {
    "SecretKey": "replace_with_strong_secret",
    "Issuer": "http://localhost:3000",
    "Audience": "http://localhost:5173",
    "ExpiresInMinutes": 60
  },
  "MongoDB": {
    "ConnectionString": "mongodb+srv://username:password@cluster.mongodb.net",
    "DatabaseName": "WordsNote"
  },
  "AllowedOrigins": [
    "http://localhost:5173"
  ]
}
```

### Local Secrets (recommended)

From `src/backend/FeatureFusion`:

```powershell
dotnet user-secrets set "MongoDB:ConnectionString" "<your_mongodb_uri>"
dotnet user-secrets set "MongoDB:DatabaseName" "WordsNote"
dotnet user-secrets set "JwtSettings:SecretKey" "<your_jwt_secret>"
dotnet user-secrets set "JwtSettings:Issuer" "http://localhost:3000"
dotnet user-secrets set "JwtSettings:Audience" "http://localhost:5173"
dotnet user-secrets set "AuthProviders:Google:ClientId" "<your_google_client_id>"
dotnet user-secrets set "AuthProviders:Google:ClientSecret" "<your_google_client_secret>"
dotnet user-secrets set "AuthProviders:Google:AdminEmail" "<your_admin_email>"
```

### Supported Flat Environment Variables

The backend maps these flat variables to .NET config keys:

- `MONGODB_URI` -> `MongoDB:ConnectionString`
- `MONGODB_DATABASE_NAME` -> `MongoDB:DatabaseName`
- `JWT_SECRET` -> `JwtSettings:SecretKey`
- `API_BASE_URL` -> `ApiBaseUrl` and `JwtSettings:Issuer`
- `FRONTEND_URL` -> `FrontendUrl` and `JwtSettings:Audience`
- `GOOGLE_CLIENT_ID` -> `AuthProviders:Google:ClientId`
- `GOOGLE_CLIENT_SECRET` -> `AuthProviders:Google:ClientSecret`
- `ADMIN_EMAIL` -> `AuthProviders:Google:AdminEmail`
- `CORS_ORIGIN` -> `AllowedOrigins`

## Frontend (Vue) Environment Variables

Frontend uses `.env` files in `src/frontend`.

- `VITE_APP_API_URL`: backend base URL, for example `http://localhost:3000`
- `VITE_GOOGLE_CLIENT_ID`: Google OAuth client ID for rendering Google Identity button
- `VITE_GOOGLE_ALLOWED_EMAIL`: allowed Google account shown in login UI

Example:

```env
VITE_APP_API_URL=http://localhost:3000
VITE_GOOGLE_CLIENT_ID=<your_google_client_id>
VITE_GOOGLE_ALLOWED_EMAIL=hnt.vn.vn@gmail.com
```

Notes:

- Frontend displays the allowed email as guidance in login page.
- Backend enforces the real allowlist via `AuthProviders:Google:AdminEmail`.
- Download page route `/download` reads shared config from backend endpoint `GET /api/download-config`.
- Download page keeps public download access for anonymous users; only authenticated admin can edit/reset config.
- Frontend auth token is persisted across refresh; store rehydrate reads persisted state and fallback key `wordsnote_auth_token`.

### Download Page Config Persistence

- Download page editor settings are stored in backend Mongo collection `wordsnote_download_page_configs`.
- This shared persistence replaces browser-only local storage for download-page overrides.
- `GET /api/download-config` is public for anonymous download page rendering.
- `PUT /api/download-config` and `DELETE /api/download-config` are admin-only operations.

## Desktop (WPF) Configuration

Desktop app now uses `appsettings.json` + environment-variable override with DI/IConfiguration.

Desktop configuration source priority:

1. `WORDSNOTE_` environment variables
2. `appsettings.{DOTNET_ENVIRONMENT}.json`
3. `appsettings.json`
4. persisted user settings at runtime (`desktop-settings.json`)

Config section:

- `Desktop:ApiBaseUrl`
- `Desktop:GoogleClientId`
- `Desktop:ThemeMode`
- `Desktop:LocalDataFolderName`

Desktop runtime defaults in this repository include a preconfigured `Desktop:GoogleClientId` for optional browser login.
End users are not required to manually input Google Client ID in desktop Settings UI.

Environment variable examples:

- `WORDSNOTE_Desktop__ApiBaseUrl=https://api.example.com`
- `WORDSNOTE_Desktop__GoogleClientId=...`
- `WORDSNOTE_Desktop__ThemeMode=dark`

Desktop authentication behavior:

- Supports Google browser login flow (system browser + local callback to retrieve ID token)
- Supports manual Google ID token submission as fallback (`POST /api/auth/google`)
- Persists JWT token in `%LocalAppData%/WordsNote/desktop/desktop-settings.json` to restore login state across app restarts

Desktop feature/auth scope:

- Manage workspace works in local mode without auth and persists data to local JSON storage:
  - `%LocalAppData%/WordsNote/desktop/manage-local-data.json`
- Desktop local/cloud ID mapping state is persisted under:
  - `%LocalAppData%/WordsNote/desktop/manage-local-sync-state.json`
- Desktop settings are persisted under:
  - `%LocalAppData%/WordsNote/desktop/desktop-settings.json`
- After Google login, desktop enters cloud mode for manage operations and enables both sync directions:
  - `Sync Local -> Cloud`
  - `Sync Cloud -> Local`
- Session and Tests APIs remain available on backend, but current desktop UI scope focuses on Landing/Learn/Manage/Privacy.

Run desktop locally:

```powershell
dotnet build src/desktop/WordsNote.Desktop.sln
dotnet run --project src/desktop/WordsNote.Desktop/WordsNote.Desktop.csproj
```

MSIX packaging project path:

- `src/desktop/WordsNote.Package/WordsNote.Package.wapproj`

## Chrome Extension Configuration

Extension runs local-first and does not require mandatory login.

Recommended:

- Extension popup contains both Learn Lab and Manage Lab workspaces.
- Do not add web route navigation (no homepage routing from popup).
- Default behavior must work with local storage only (`wordsnote_local_cards`, `wordsnote_local_collections`).
- Optional cloud sync uses API endpoint base URL, defaulting to `http://words-note.runasp.net`.
- Cloud endpoint is persisted in extension storage key `wordsnote_cloud_api_base_url`.
- Optional cloud JWT token is persisted in extension storage key `wordsnote_cloud_auth_token`.
- Extension supports collection-level filtering in Local and Cloud modes.
- `Sync To Local` copies cloud cards into local storage for local review flow.
- `Sync Local -> Cloud` uploads local collections/cards with the saved JWT token.

## Security Notes

- Never commit real credentials to source control.
- Rotate JWT secret if leaked.
- Keep production origins strict in `AllowedOrigins`.
- Use HTTPS for production frontend and backend endpoints.

## Deployment Notes (Vercel + External Backend)

If frontend is deployed on Vercel and backend is hosted separately:

- Set frontend API base URL to Vercel proxy path:
  - `VITE_APP_API_URL=/api-backend`
- Configure Vercel rewrite from `/api-backend/:path*` to backend HTTPS origin.
- Prefer HTTPS backend destination (not HTTP) to avoid upstream instability.

Recommended Vercel header for Google popup auth:

- `Cross-Origin-Opener-Policy: same-origin-allow-popups`

This reduces browser warnings like:

- `Cross-Origin-Opener-Policy policy would block the window.postMessage call.`

### 503 on deployed API (`/api-backend/api/*`)

This usually means backend dependency/configuration failure, not frontend routing.

Common checks:

1. Backend secrets are real values (not placeholders):
   - `MONGODB_URI`
   - `JWT_SECRET`
   - `GOOGLE_CLIENT_SECRET`
2. Backend can connect to MongoDB from hosting environment.
3. Backend `CORS_ORIGIN` includes deployed frontend origin.
4. Backend `API_BASE_URL` and `FRONTEND_URL` are valid production URLs.

Important:

- Values like `__SET_LOCALLY__` or `__SET_IN_USER_SECRETS_OR_ENV__` are placeholders and will break runtime if used in production.
