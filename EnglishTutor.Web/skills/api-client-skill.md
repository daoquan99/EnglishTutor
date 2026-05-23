# Skill: Add or Update API Client Usage

Use this skill when adding or updating backend API usage.

## Steps

1. Read related backend API documentation.
2. Define request/response DTOs in feature types.
3. Add API function in `features/{feature}/api`.
4. Add query/mutation hook in `features/{feature}/hooks`.
5. Add error handling.
6. Add loading/error/empty states in UI.
7. Update frontend feature docs.
8. Update workflow docs if user flow changes.

## Rules

- Do not call backend APIs directly in random components.
- Do not hard-code base URLs.
- Use shared HTTP client.
- Use TanStack Query for server state.
