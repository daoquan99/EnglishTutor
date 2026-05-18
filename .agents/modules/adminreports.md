# AdminReports Module

## Responsibility
Owns admin dashboard, reports, analytics, audit logs, operational views.

## Schema
`adminreports`

## Tables/Views
- `adminreports.UserOverviewCards` — Projection: user + level + EXP + streak
- `adminreports.DailyAiUsageReports` — AI usage by day/model
- `adminreports.LearningActivityReports` — Activity stats by period
- `adminreports.CommonMistakeStats` — Top mistake categories
- `adminreports.AssessmentPassRateReports` — Pass/fail rates by level
- `adminreports.RetentionReports` — User retention
- `adminreports.AuditLogs` — Admin action tracking (old/new values)

## Database Views (allowed for this module only)
- `vw_UserOverview` — cross-schema join for admin
- `vw_DailyAiUsage` — AI request log aggregation
- `vw_CommonMistakes` — mistake statistics
- `vw_AssessmentPassRates` — pass/fail aggregation

## Endpoints (Admin auth required)
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/admin/reports/users` | Admin |
| GET | `/api/admin/reports/ai-usage` | Admin |
| GET | `/api/admin/reports/learning-activity` | Admin |
| GET | `/api/admin/reports/mistakes` | Admin |
| GET | `/api/admin/reports/assessments` | Admin |
| GET | `/api/admin/audit-logs` | Admin |

## Events Consumed
All learning module events → update report projections incrementally
