# SignUp Page Migration

## Original File
`Tekhnologia/Pages/Auth/SignUp.razor`

## New File
`Tekhnologia.UI/Pages/Auth/SignUp.razor`

## Purpose
Register a new user and perform automatic login on success.

## API Endpoints
- `POST /api/auth/register`
- `POST /api/auth/login` (auto-login step)

## Data Flow
1. Form validation via data annotations.
2. `AuthApiService.RegisterAsync` sends registration payload.
3. On success, calls `AuthStateService.LoginAsync`.
4. Navigate to `/profile`.

## Differences & Improvements
- Removed navigation to `/api/auth/autologin` endpoint.
- Standardized flow using the same login endpoint as SignIn.
- Clear success/error messaging.

## Future Enhancements
- Password strength indicator.
- Inline async validation for existing email.
