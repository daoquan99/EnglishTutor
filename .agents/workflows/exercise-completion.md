# Exercise Completion Workflow

## Overview
User completes exercise sets with static or AI grading. Wrong answers create mistakes.

## Main Flow
```
1. User → GET /api/exercises (browse exercise sets)
2. User → POST /api/exercises/{id}/attempts (start attempt)
3. User → POST /api/exercises/attempts/{attemptId}/answers (submit answer per question)
4. For static questions: backend compares with CorrectAnswer
5. For AI-graded questions: backend calls AI.Contracts for grading
6. User → POST /api/exercises/attempts/{attemptId}/complete
7. Exercises → calculates total score
8. Exercises → saves ExerciseCompletedIntegrationEvent to Outbox (includes WrongAnswers list)
9. Worker → dispatches to:
   - Mistakes → creates mistakes for each wrong answer
   - Progress → grants EXP, updates skill progress, dashboard
```

## Modules Involved
Exercises, AI (contract for grading), Mistakes (consumer), Progress (consumer)

## Events Published
- `ExerciseCompletedIntegrationEvent` → Mistakes, Progress, AdminReports
