# Level-Up Assessment Workflow

## Overview
User takes level-up test. AI grades open answers. Backend applies strict pass/fail rules. Level promotion flows across modules.

## Main Flow
```
1. User → POST /api/assessments/level-up (start assessment)
2. Assessments → checks eligibility (no recent failure within 7 days)
3. Assessments → selects assessment for current level
4. User → POST /api/assessments/attempts/{id}/answers (submit answers)
5. User → POST /api/assessments/attempts/{id}/submit (submit for grading)
6. Assessments → grades static answers directly
7. Assessments → grades open answers via AI.Contracts.IAssessmentGradingService
8. Assessments → calculates section scores (weighted) + total score
9. Assessments → applies pass/fail rules (domain logic):
   - Total >= 75 AND all sections >= 60 → PASSED
   - Otherwise → FAILED
10. If PASSED → saves LevelUpApprovedIntegrationEvent to Outbox
11. If FAILED → saves AssessmentFailedIntegrationEvent to Outbox
```

## Level-Up Event Chain (Cross-Module)
```
Assessments → LevelUpApprovedIntegrationEvent
  → Users.LevelUpApprovedEventHandler → updates UserTargetLanguage.CurrentLevel
  → Users saves UserLevelChangedIntegrationEvent to Outbox
    → Progress → grants 500 bonus EXP, logs activity
    → LearningContent → refreshes learning path cards for new level
    → Notifications → sends congratulations notification
```

## Key Design Rule
AI = grader/evaluator. Assessments = decision engine. Users = level owner.

## Modules Involved
Assessments, AI (contract), Users (consumer), Progress (consumer), LearningContent (consumer), Notifications (consumer)
