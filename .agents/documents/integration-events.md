# Integration Events Catalog

## Users Module (Producer)
| Event | Payload | Consumers |
|-------|---------|-----------|
| `UserRegisteredIntegrationEvent` | UserId, Email, DisplayName, RegisteredAtUtc | Users (create profile), Notifications (default settings) |
| `UserProfileUpdatedIntegrationEvent` | UserId, DisplayName | LearningContent |
| `UserLanguageSettingsUpdatedIntegrationEvent` | UserId, NativeLanguageCode, TargetLanguageCode, UiLanguageCode, ExplanationLanguageCode | — |
| `UserTargetLanguageChangedIntegrationEvent` | UserId, TargetLanguageCode, IsActive | — |
| `UserLevelChangedIntegrationEvent` | UserId, TargetLanguageCode, PreviousLevel, NewLevel, ChangedAtUtc | Progress, LearningContent, Notifications |

## StudyPlans Module (Producer)
| Event | Consumers |
|-------|-----------|
| `StudyPlanCreatedIntegrationEvent` | — |
| `StudyPlanUpdatedIntegrationEvent` | — |
| `PlannedStudySessionMissedIntegrationEvent` | Notifications |
| `DailyStudyTargetCompletedIntegrationEvent` | Notifications, Progress |

## Vocabulary Module (Producer)
| Event | Consumers |
|-------|-----------|
| `VocabularyReviewedIntegrationEvent` | Progress |
| `VocabularyMasteredIntegrationEvent` | Progress |
| `VocabularyPronunciationPracticedIntegrationEvent` | Progress, Mistakes |
| `ExampleSentencePronunciationPracticedIntegrationEvent` | Progress, Mistakes |

## Exercises Module (Producer)
| Event | Consumers |
|-------|-----------|
| `ExerciseCompletedIntegrationEvent` | Mistakes, Progress, AdminReports |

## Speaking Module (Producer)
| Event | Consumers |
|-------|-----------|
| `SpeakingTurnCorrectedIntegrationEvent` | Mistakes, Progress |
| `SpeakingSessionCompletedIntegrationEvent` | Progress, LearningContent, AdminReports |

## Mistakes Module (Producer)
| Event | Consumers |
|-------|-----------|
| `MistakeCreatedIntegrationEvent` | LearningContent, AdminReports |
| `MistakeReviewedIntegrationEvent` | Progress |
| `MistakeMasteredIntegrationEvent` | Progress |

## Assessments Module (Producer)
| Event | Consumers |
|-------|-----------|
| `AssessmentCompletedIntegrationEvent` | Mistakes, Progress, AdminReports |
| `AssessmentPassedIntegrationEvent` | AdminReports |
| `AssessmentFailedIntegrationEvent` | AdminReports |
| `LevelUpApprovedIntegrationEvent` | Users |

## Progress Module (Producer)
| Event | Consumers |
|-------|-----------|
| `ExperienceGrantedIntegrationEvent` | AdminReports |
| `DailyProgressUpdatedIntegrationEvent` | AdminReports |
