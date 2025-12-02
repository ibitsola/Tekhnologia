# Tekhnologia Architecture Guide

## Project Structure

```
Tekhnologia.sln (Solution)
├── Tekhnologia (Backend API + Services)
│   ├── Controllers/          # Web API endpoints
│   ├── Services/             # Business logic (shared with UI via DI)
│   ├── Models/               # Domain entities
│   ├── Data/                 # EF Core DbContext
│   └── Database/Migrations/  # EF migrations
│
└── Tekhnologia.UI (Blazor Server Frontend)
    ├── Pages/                # Razor components
    ├── Shared/               # Layout components
    └── wwwroot/              # Static assets (CSS, JS, images)
```

## Architecture Pattern: Monolithic Solution with Separate Concerns

### ✅ What You Have (Best Practice)

**Single Solution, Multi-Project:**
- One `.sln` file containing both projects
- UI project references backend via `<ProjectReference>`
- Services, models, and interfaces are shared through DI
- No NuGet packaging needed (that's for separate repos)

**Shared Services via Dependency Injection:**
```csharp
// Backend defines services
public interface IUserService { ... }
public class UserService : IUserService { ... }

// UI registers AND uses them
builder.Services.AddScoped<IUserService, UserService>();

// Blazor components inject them
@inject IUserService UserService
```

### 🔐 Authentication Architecture

**Backend Responsibilities:**
- User registration/login via API endpoints (`/api/auth/login`)
- Password hashing and validation
- Cookie issuance (`.Tekhnologia.Identity`)
- Database operations (EF Core + Identity)

**UI Responsibilities:**
- Cookie reading (via shared DataProtection keys)
- Rendering based on `ClaimsPrincipal`
- No password handling or direct DB access
- Uses YARP reverse proxy to forward `/api/*` to backend

**Cookie Flow:**
1. User submits login form → UI calls `/api/auth/login` (proxied to backend)
2. Backend validates credentials → Issues cookie with `Set-Cookie` header
3. Browser stores cookie for UI domain (`localhost:5145`)
4. UI reads cookie on subsequent requests → Populates `ClaimsPrincipal`
5. Blazor `<AuthorizeView>` renders based on authentication state

### 📦 Static Assets (CSS/JS)

**Current Setup (Recommended for Your Scale):**
- Pre-built CSS files in `wwwroot/css/` (bulma.min.css, site.css)
- No build pipeline (no Webpack/Vite/NPM)
- Browser loads directly from `wwwroot`

**When to Use NPM + Build Tools:**
- Large projects with TypeScript, SCSS, bundling
- Need tree-shaking or dead code elimination
- Want hot module replacement (HMR)

**You don't need it because:**
- Bulma CSS is already minified
- Custom CSS is simple enough to write directly
- Blazor Server handles component hot reload

### 🔄 Service Sharing Best Practices

**What's Shared (via ProjectReference):**
- ✅ Business logic services (`IUserService`, `IAIService`, etc.)
- ✅ Domain models (`User`, `Goal`, `JournalEntry`)
- ✅ DTOs for data transfer
- ✅ Interfaces for testability

**What's NOT Shared:**
- ❌ Controllers (backend only)
- ❌ Blazor components (UI only)
- ❌ `Program.cs` logic (each host configures independently)

### 📊 When to Use NuGet Packages Instead

**Use NuGet packages if:**
1. Projects are in separate repositories
2. Multiple solutions need the same code
3. You want versioned releases of shared libraries
4. External teams consume your APIs

**Your scenario doesn't need it because:**
- Single repo with single solution
- One team maintaining both projects
- Project reference provides compile-time safety
- Simpler debugging and refactoring

### 🎯 Current Pain Point: Authentication State Refresh

**The Issue:**
- Login happens via fetch (JavaScript XHR call)
- Blazor circuit was created before cookie existed
- Default `AuthenticationStateProvider` doesn't auto-refresh

**Solutions (in order of complexity):**

1. **Force page reload after login** ✅ (Current implementation)
   ```javascript
   await fetch('/api/auth/login', { credentials: 'include' });
   location.reload(); // Rebuild circuit with new cookie
   ```

2. **Custom AuthenticationStateProvider** (Better UX)
   - Implement `RevalidatingServerAuthenticationStateProvider`
   - Add `ForceRefresh()` method to trigger state change
   - Call after login without full reload

3. **Server-side form POST** (Most reliable)
   - Replace fetch with `<form method="post" action="/login">`
   - UI endpoint calls backend service internally
   - Circuit built in same request that sets cookie

### 📝 Recommended Next Steps

**Architecture Improvements:**
1. Move `AuthStateService` to `Tekhnologia.UI/Services/` (it's UI-specific)
2. Remove duplicate Identity registration from UI (keep only in backend)
3. Implement custom `AuthenticationStateProvider` for seamless refresh
4. Optional: Add a shared `Tekhnologia.Shared` project for true shared code

**Code Quality:**
1. ✅ Removed duplicate `package.json` from backend
2. Add XML documentation comments to public services
3. Consider using MediatR for command/query separation (future)
4. Add integration tests for auth flow

### 🚀 Deployment Considerations

**Development (Current):**
- Two processes: Backend (`localhost:7137`) + UI (`localhost:5145`)
- YARP proxy in UI forwards API calls to backend
- Shared DataProtection keys folder

**Production Options:**

**Option A: Keep Separate (Microservices-lite)**
- Deploy backend as API service
- Deploy UI as web app
- Configure YARP with production backend URL
- Use Redis for distributed DataProtection keys

**Option B: Consolidate (Simplest)**
- Merge into single host (remove YARP)
- Serve Blazor + API from same process
- Eliminates cross-origin cookie issues
- Easier to deploy as single container/app service

**Recommendation:** Start with Option B for first production release, evaluate Option A only if you need independent scaling.

---

## Quick Reference

### Run Both Projects
```powershell
# Terminal 1 - Backend
cd C:\Dev\Tekhnologia\Tekhnologia
dotnet run

# Terminal 2 - UI
cd C:\Dev\Tekhnologia\Tekhnologia.UI
dotnet run
```

### Add EF Migration
```powershell
cd C:\Dev\Tekhnologia\Tekhnologia
dotnet ef migrations add YourMigrationName --context ApplicationDbContext --output-dir Database/Migrations
```

### Debug Authentication
- Visit `http://localhost:5145/debug-auth` after login
- Check browser DevTools → Application → Cookies for `.Tekhnologia.Identity`
- Check server console for `[DEBUG] CheckAuthStatus` logs

### Clean Rebuild
```powershell
Remove-Item -Recurse -Force ".\Tekhnologia\bin", ".\Tekhnologia\obj", ".\Tekhnologia.UI\bin", ".\Tekhnologia.UI\obj"
dotnet restore
dotnet build
```
