# SignIn Page Migration

## Original File
`Tekhnologia/Pages/Auth/SignIn.razor`

## New File
`Tekhnologia.UI/Pages/Auth/SignIn.razor`

## Purpose
Authenticate user via email/password and establish cookie-based session.

## API Endpoints
- `POST /api/auth/login`
- `GET /api/user/current` (indirect via `AuthStateService.RefreshAsync`)

## Data Flow
1. User submits credentials.
2. `AuthStateService.LoginAsync` posts to backend.
3. On success, state refreshed and navigation to `/profile`.

## Differences & Improvements
- Removed JS interop call (`auth.signin`).
- Pure HttpClient pattern via service; no page reload hack.
- Simplified error handling (will extend with structured errors later).

## Future Enhancements
- Add detailed backend error parsing.
- Add password visibility toggle.
- Add loading skeleton for initial auth check.
