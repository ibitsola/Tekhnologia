# GoalTracker Page Migration

## Original File
`Tekhnologia/Pages/GoalTracker/GoalTracker.razor`

## New File
`Tekhnologia.UI/Pages/GoalTracker.razor`

## Purpose
Authenticated interface for listing, selecting, creating, editing, completing, and deleting user goals. Public marketing view when unauthenticated.

## API Endpoints
- `GET /api/goal/user/{userId}` – fetch user goals
- `POST /api/goal` – create goal
- `PUT /api/goal/{goalId}` – update goal
- `PUT /api/goal/{goalId}/complete` – mark goal complete
- `DELETE /api/goal/{goalId}?userId={userId}` – delete goal

## Data Flow
1. Auth state verified via `IAuthStateService` / user fetched with `IUserClient`.
2. Goals loaded through `IGoalApiService.GetUserGoalsAsync`.
3. UI state holds currently selected goal + modal visibility flags.
4. Create/Edit operations reuse same inline form (`CreateGoalDTO` for editing as a temporary container; converted to `UpdateGoalDTO`).
5. On mutation success, list refreshed and selection updated.

## Differences & Improvements
- Removed JS utility functions (date math & progress) and replaced with `GoalHelpers` C# static methods.
- No reflection or dynamic property access—strongly typed DTO classes.
- Inline modals retained for parity; easily componentizable later.
- Lean HttpClient wrappers keep frontend simple; no business logic moved.

## Helper Methods (GoalHelpers)
- `DaysIntoGoal` / `DaysRemaining` / `ProgressPercentage` / `PriorityLabel` for timeline & stats.

## Future Enhancements
- Add client-side filtering & sorting.
- Add sub-goals / tasks view.
- Extract modal & notifications into reusable components.
- Add optimistic UI updates & skeleton loaders.
