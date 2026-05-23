# Frontend Architecture

## Stack

EnglishTutor frontend uses:

```text
Next.js App Router
React
TypeScript
Tailwind CSS
shadcn/ui
TanStack Query
React Hook Form
Zod
Recharts
lucide-react
```

## Architecture Style

The frontend follows feature-based architecture:

```text
src/app      = routing/page composition
src/features = business feature modules
src/shared   = cross-feature reusable code
```

## Main Principle

App routes should compose feature components. Feature modules own business UI and API usage. Shared code must stay generic.

## Runtime Areas

```text
(public) = marketing/public pages
(auth)   = login/register/password recovery
(app)    = authenticated learner app
(admin)  = admin/reporting area
```
