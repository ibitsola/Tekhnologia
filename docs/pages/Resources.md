# Resources Page Migration

## Legacy Location
`Tekhnologia/Pages/Resources/Resources.razor`

## Migrated Location
`Tekhnologia.UI/Pages/Resources/Resources.razor`

## Purpose
Marketing + future authenticated resource hub (search, download, tagging, purchase/license tracking).

## Changes Made
- Updated namespace usage for `IAuthStateService`.
- Kept all marketing sections (Why, How It Works, What You Get) intact.
- Authenticated placeholder retained for upcoming file listing/search UI.

## Future Implementation Hooks
- Resource metadata API (title, category, tags, file path).
- Secure download endpoint with Stripe purchase validation (Paid items).
- Keyword + category filtering component.
- Admin upload management panel integration.

## Parity Notes
Exact structural parity; dynamic resource listing deferred.
