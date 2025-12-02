# EditProfile Page Migration

## Original File
`Tekhnologia/Pages/Auth/EditProfile.razor`

## New File
`Tekhnologia.UI/Pages/Auth/EditProfile.razor`

## Purpose
Allow user to update display name and optionally change password.

## API Endpoints
- `PUT /api/user/{id}` (name update)
- `PUT /api/user/{id}/password` (password update)
- `POST /api/auth/logout` (after password change)

## Data Flow
1. Load current user via `UserClient` for initial name.
2. On submit: update name first (if changed) then optionally password.
3. If password updated: force logout and redirect to SignIn.
4. Else redirect back to Profile.

## Differences & Improvements
- Removed console logging noise present in legacy file.
- Replaced try/catch layers with service-level error returns.
- Centralized success/error messaging in component state.

## Future Enhancements
- Add password strength meter.
- Add unsaved changes prompt.
- Add granular error messages from backend.
