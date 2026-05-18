# Solution Structure

```
src/
├── Bootstrapper/
│   ├── EnglishTutor.Api/          (HTTP host, stateless, scalable)
│   └── EnglishTutor.Worker/       (Background jobs, outbox processing)
│
├── BuildingBlocks/
│   ├── EnglishTutor.BuildingBlocks.Domain/          (Entity, AggregateRoot, ValueObject, DomainEvent)
│   ├── EnglishTutor.BuildingBlocks.Application/     (Result<T>, CQRS, Pipeline, ICurrentUser)
│   ├── EnglishTutor.BuildingBlocks.Infrastructure/  (DateTimeProvider, Storage, Persistence)
│   ├── EnglishTutor.BuildingBlocks.EventBus/        (IEventBus, IIntegrationEvent, InProcessEventBus)
│   ├── EnglishTutor.BuildingBlocks.Outbox/          (OutboxMessage, InboxMessage, DeadLetter)
│   └── EnglishTutor.BuildingBlocks.SharedKernel/    (LanguageCode, LanguageLevel, LearningSkill)
│
├── Modules/
│   ├── Auth/           (5 projects: Domain, Application, Infrastructure, Presentation, Contracts)
│   ├── Users/
│   ├── StudyPlans/
│   ├── LearningContent/
│   ├── Vocabulary/
│   ├── Exercises/
│   ├── Speaking/
│   ├── AI/
│   ├── Mistakes/
│   ├── Assessments/
│   ├── Progress/
│   ├── Notifications/
│   └── AdminReports/
│
└── Tests/
    ├── EnglishTutor.ArchitectureTests/
    ├── EnglishTutor.IntegrationTests/
    └── EnglishTutor.Modules.{Module}.UnitTests/

docs/
├── api/           (API documentation per module)
├── workflows/     (Business workflow documentation)
├── events/        (Event catalog + outbox/inbox docs)
└── adr/           (Architecture Decision Records)
```

## Database Schemas
```
auth, users, studyplans, learningcontent, vocabulary,
exercises, speaking, ai, mistakes, assessments,
progress, notifications, adminreports, messaging
```

## Centralized Build
- `Directory.Build.props` — shared build settings (TargetFramework, Nullable, etc.)
- `Directory.Packages.props` — all NuGet package versions (centralized)
