# Environment Configuration

This document defines runtime configuration for the WordsNote monorepo.

## Project Layout

- Backend: `src/backend/FeatureFusion`
- Frontend: `src/frontend`
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

## Chrome Extension Configuration

Extension currently runs in local mode and does not require authentication.

Recommended:

- Keep API base URL configurable in extension service config if server sync is enabled later.
- Default behavior should work with local storage only.

## Security Notes

- Never commit real credentials to source control.
- Rotate JWT secret if leaked.
- Keep production origins strict in `AllowedOrigins`.
- Use HTTPS for production frontend and backend endpoints.
