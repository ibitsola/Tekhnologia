# Index (Home) Page Migration

## Original File
`Tekhnologia/Pages/Home/Home.razor`

## New File
`Tekhnologia.UI/Pages/Index.razor`

## Purpose
Displays personalized dashboard when authenticated; marketing/landing content when unauthenticated.

## Data & Services
- Uses `IAuthStateService` to determine auth state.
- Calls `IUserClient.GetCurrentUserAsync()` to fetch current user for first-name greeting.

## API Endpoints Utilized
- `GET /api/user/current`

## Legacy Differences
- Removed direct dependency on backend `IUserService`.
- JS utilities not required; greeting resolved server-side.
- Layout provided by new `MainLayout` rather than legacy `Views/Shared/MainLayout.razor`.

## Improvements
- Strong typing through `UserInfo` model.
- Clear separation of concerns (no mixed marketing/auth code outside component scope).

## Future Tasks
- Add loading skeleton.
- Parameterize marketing sections into reusable `MarketingHeader` component.
