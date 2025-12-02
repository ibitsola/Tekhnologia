# AI Coach Page Migration

## Original File
`Tekhnologia/Pages/AICoach/AICoach.razor`

## New File
`Tekhnologia.UI/Pages/AICoach.razor`

## Purpose
Provide authenticated users with an interactive AI coaching chat interface for career guidance.

## API Endpoint
- `POST /api/chatbot/business-coach` (requires auth cookie) returns `{ response: string }`.

## Data Flow
1. User enters message; form submission triggers `Send()`.
2. Component appends user line to log immediately (optimistic UI).
3. `IAIApiService.GetBusinessCoachingResponseAsync` sends payload `{ message }`.
4. Successful response appended as `AI` line; errors captured in `error`.

## Differences & Improvements
- Removed dependency on backend `IAIService` directly in UI; replaced with HTTP client wrapper.
- Preserved simple log structure; still a candidate for richer message model later.
- Added null-response fallback `(no response)` to handle unexpected empty results.

## Future Enhancements
- Streaming responses (SignalR or SSE) for incremental AI tokens.
- Conversation context persistence (store last N messages server-side).
- Add typing indicator and message timestamps.
- Add error retry button and resilience for transient failures.
