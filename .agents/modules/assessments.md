# Assessments Module

## Responsibility
Owns placement tests, level-up tests, strict grading, pass/fail decisions.

## Schema
`assessments`

## Tables
- `assessments.AssessmentDefinitions` — AssessmentType, ForLevel, PassingScore, MinSkillScore, TimeLimitMinutes
- `assessments.AssessmentSections` — Skill, Weight, Order
- `assessments.AssessmentQuestions` — Prompt, CorrectAnswer, IsAiGraded, MaxScore
- `assessments.UserAssessmentAttempts` — Status (InProgress/Submitted/Grading/Passed/Failed), TotalScore
- `assessments.UserAssessmentAnswers` — UserAnswer, Score, Feedback
- `assessments.AssessmentGradingResults` — SectionScoresJson, IsPassed
- `assessments.AssessmentRubrics` — Criteria, ScoringGuide (for AI grader)

## Assessment Types
PlacementTest, LevelUpTest, SkillCheck, MonthlyReviewTest

## Pass/Fail Rules (Domain Logic)
- TotalScore >= PassingScore (default 75)
- No section score < MinSkillScore (default 60)
- AI = grader/evaluator, Assessments = decision engine, Users = level owner

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/assessments/available` | Yes |
| POST | `/api/assessments/level-up` | Yes |
| GET | `/api/assessments/attempts/{id}` | Yes |
| POST | `/api/assessments/attempts/{id}/answers` | Yes |
| POST | `/api/assessments/attempts/{id}/submit` | Yes |
| GET | `/api/assessments/attempts/{id}/result` | Yes |

## Events Produced
- `AssessmentStartedIntegrationEvent`
- `AssessmentCompletedIntegrationEvent`
- `AssessmentPassedIntegrationEvent`
- `AssessmentFailedIntegrationEvent`
- `LevelUpApprovedIntegrationEvent` (triggers Users → level update → chain)
