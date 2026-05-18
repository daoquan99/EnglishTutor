# Exercises Module

## Responsibility
Owns practice exercises and detailed exercise attempts/answers.

## Schema
`exercises`

## Tables
- `exercises.ExerciseSets` — ExerciseType, Level, Topic, Skill, Title
- `exercises.ExerciseQuestions` — Prompt, CorrectAnswer, IsAiGraded, Order, Difficulty
- `exercises.ExerciseOptions` — For multiple choice (OptionText, IsCorrect)
- `exercises.UserExerciseAttempts` — Score, CorrectCount, Status (InProgress/Completed)
- `exercises.UserExerciseAnswers` — UserAnswer, IsCorrect, Score, Feedback
- `exercises.UserExerciseResults` — Final result summary

## Exercise Types
MVP: MultipleChoice, FillInTheBlank, VerbConjugation, SentenceCorrection, SentenceOrdering
Future: Translation, Matching, ListeningChoice, ConversationCompletion, ShortWriting

## Static vs AI-graded
- Static: fixed answer, backend grades directly
- AI-graded: open answer, AI grades using rubric (via AI.Contracts)

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/exercises` | Yes |
| GET | `/api/exercises/{id}` | Yes |
| POST | `/api/exercises/{id}/attempts` | Yes |
| POST | `/api/exercises/attempts/{attemptId}/answers` | Yes |
| POST | `/api/exercises/attempts/{attemptId}/complete` | Yes |
| GET | `/api/exercises/attempts/{attemptId}/result` | Yes |

## Events Produced
- `ExerciseStartedIntegrationEvent`
- `ExerciseQuestionAnsweredIntegrationEvent`
- `ExerciseCompletedIntegrationEvent` (includes WrongAnswers list for Mistakes)
