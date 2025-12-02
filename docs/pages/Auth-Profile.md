# Profile Page Migration

## Original File
`Tekhnologia/Pages/Auth/Profile.razor`

## New File
`Tekhnologia.UI/Pages/Auth/Profile.razor`

## Purpose
Display authenticated user profile details and link to edit page.

## API Endpoints
- `GET /api/user/current` (get user ID)
- `GET /api/user/{id}` (profile details via `UserClient` in legacy; simplified now)

## Data Flow
1. Ensure auth state initialized.
2. If authenticated, fetch current user with `UserClient.GetCurrentUserAsync`.
3. Render fields; provide navigation to edit.

## Differences & Improvements
- Removed legacy layout directive; now uses global `MainLayout`.
- Eliminated multiple conditional blocks for error cases; simplified states.
- Strongly typed `UserInfo` model.

## Future Enhancements
- Add created timestamp.
- Add avatar / personalization.
- Add server error detail parsing.
