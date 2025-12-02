# Journal Page Migration

## Legacy Location
`Tekhnologia/Pages/Journal/Journal.razor`

## Migrated Location
`Tekhnologia.UI/Pages/Journal/Journal.razor`

## Purpose
Marketing + future reflective journaling feature. Shows authenticated placeholder and public marketing/education content for unauthenticated visitors.

## Changes Made
- Namespace updated to `Tekhnologia.UI.Services.Interfaces` for `IAuthStateService`.
- Removed unused legacy service references (only auth state needed currently).
- Markup and CSS class names preserved 1:1.

## Future Implementation Hooks
- Journal entry editor (rich text or markdown) component.
- Sentiment analysis integration through AI service.
- Entry list with filtering (date, sentiment, visibility).
- Privacy flag per entry + sharing workflow.

## Parity Notes
No behavioral changes yet; authenticated block remains placeholder as in legacy.
