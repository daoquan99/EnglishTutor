# LearningContent Module

## Responsibility
Owns structured learning content except vocabulary flashcard content.

## Schema
`learningcontent`

## Tables
- `learningcontent.Lessons` — TargetLanguageCode, Level, Topic, Skill, Title, IsPublished
- `learningcontent.LessonTranslations` — Title, Description per language
- `learningcontent.LessonSections` — Content (markdown), SectionType (Theory/Example/Practice)
- `learningcontent.LessonSectionTranslations`
- `learningcontent.ConversationScenarios` — Setting, Difficulty, EstimatedMinutes
- `learningcontent.ConversationScenarioTranslations`
- `learningcontent.ConversationLines` — Speaker (User/AI), Order, Text, ExpectedResponseHint
- `learningcontent.ConversationLineTranslations`
- `learningcontent.SentencePatterns`
- `learningcontent.Quizzes`
- `learningcontent.UserLearningPathCards` — Read model (ContentType, Status, Order)

## Conversation Ownership
LearningContent = conversation script. Speaking = user practice session and results.

## Endpoints
| Method | Route | Auth |
|--------|-------|------|
| GET | `/api/lessons` | Yes |
| GET | `/api/lessons/{id}` | Yes |
| POST | `/api/lessons/{id}/complete` | Yes |
| GET | `/api/learning-path` | Yes |
| GET | `/api/conversations` | Yes |
| GET | `/api/conversations/{id}` | Yes |

## Contract Readers
- `IConversationScenarioReader` → Speaking module

## Events Produced
- `LessonPublishedIntegrationEvent`
- `LessonCompletedIntegrationEvent`
- `ConversationScenarioCompletedIntegrationEvent`

## Events Consumed
- `UserProfileUpdatedIntegrationEvent` → refresh learning path
- `UserLevelChangedIntegrationEvent` → refresh learning path cards
- `SpeakingSessionCompletedIntegrationEvent` → mark conversation completed
