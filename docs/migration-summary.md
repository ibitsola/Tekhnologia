# Tekhnologia Blazor UI Migration Summary

## Overview
The legacy `Tekhnologia` project retains all backend API logic, authentication, and data access. The new `Tekhnologia.UI` Blazor Web App (Server) provides the modern frontend, consuming backend endpoints via `HttpClient`.

## Architecture Changes
- Separation of concerns: Backend (`Tekhnologia`) vs Frontend (`Tekhnologia.UI`).
- Frontend state: `AuthStateService` performs lightweight cookie-based auth checks via `/api/user/current`.
- JS auth helpers replaced by native Blazor service calls.
- Shared UI components: `NavMenu`, `Footer`, `RedirectToSignIn`, `MainLayout`.

## Routing
- Root page moved to `Pages/Index.razor`.
- Existing functional routes preserved (`/goaltracker`, `/journal`, `/visionboard`, `/aicoach`, `/resources`, `/profile`, `/admin/users`).
- Future pages will adopt PascalCase file names but maintain existing route casing for continuity.

## Component Structure (Current Seed)
```
Tekhnologia.UI/
  Layouts/MainLayout.razor
  Components/NavMenu.razor
  Components/Footer.razor
  Components/RedirectToSignIn.razor
  Pages/Index.razor
  Services/
    Interfaces/IAuthStateService.cs
    Interfaces/IUserClient.cs
    AuthStateService.cs
    UserClient.cs
```

## Data Flow
- UI pages request user/auth info through `AuthStateService` and `UserClient` (HTTP calls to backend).
- No direct EF or business logic exists in UI; all persistence remains in backend.

## Improvements vs Legacy
- Eliminated JS `auth.js` for login/logout logic (native Blazor services planned).
- Strongly typed user info (`UserInfo`) rather than dynamic JSON parsing in page components.
- Centralized layout under `Layouts/MainLayout.razor` instead of `Views/Shared` folder.

## Pending Migration Tasks
- Migrate Auth pages (SignIn, SignUp, Profile, EditProfile).
- Migrate Goal Tracker with modal component extraction.
- Migrate AI Coach chat and replace inline list rendering with message component.
- Port marketing/placeholder pages (Journal, VisionBoard, Resources) unchanged; later implement full feature UIs.
- Admin Users page: replace reflection with typed DTO.
- Introduce shared utility helpers (date/progress) replacing `utils.js`.
- Documentation per page under `docs/pages/`.

## Future Enhancements
- Consider separate shared contracts library for DTOs.
- Add error boundary components.
- Add caching layer for frequently accessed user/profile data.
- Replace button anchor tags with `NavLink` components for active styling.

---
Initial scaffold committed on branch `feature/blazor-ui-migration`.
