# Speaking Module

## Responsibility
Owns speaking practice runtime, sessions, turns, turn results, session summaries.

## Schema
`speaking`

## Tables
- `speaking.SpeakingSessions` — UserId, SessionType, Topic, language snapshot fields, Status
- `speaking.SpeakingTurns` — SessionId, TurnNumber, UserText, AudioUrl, Status
- `speaking.SpeakingTurnResults` — Scores (Grammar, Vocabulary, Pronunciation, Fluency, Overall), Feedback
- `speaking.SpeakingSessionSummaries` — Average scores, TotalTurns, StrongPoints, WeakPoints
- `speaking.UserSpeakingHistoryCards` — Read model
- `speaking.ConversationPracticeResults` — Results for conversation-based sessions

## Language Snapshot (captured at session start)
NativeLanguageCodeAtStart, TargetLanguageCodeAtStart, UiLanguageCodeAtStart, ExplanationLanguageCodeAtStart, UserLevelAtStart

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| POST | `/api/speaking/sessions` | Yes |
| GET | `/api/speaking/sessions` | Yes |
| GET | `/api/speaking/sessions/{id}` | Yes |
| POST | `/api/speaking/sessions/{id}/turns` | Yes |
| POST | `/api/speaking/sessions/{id}/complete` | Yes |
| GET | `/api/speaking/sessions/{id}/summary` | Yes |

## Uses Contracts
- `Users.Contracts.IUserLanguageSettingsReader`
- `AI.Contracts.IEnglishCorrectionService`
- `LearningContent.Contracts.IConversationScenarioReader` (for conversation practice)

## Events Produced
- `SpeakingSessionStartedIntegrationEvent`
- `SpeakingTurnCorrectedIntegrationEvent`
- `SpeakingSessionCompletedIntegrationEvent`
- `ConversationPracticeCompletedIntegrationEvent`

## Must NOT Do
- Must NOT call Gemini/Gemma clients directly
- Must NOT reference Users.Infrastructure or AI.Infrastructure
