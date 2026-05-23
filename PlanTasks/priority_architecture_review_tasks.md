# Priority Architecture Review Tasks

Status checked against source on 2026-05-20.

## Priority 1

- [x] Auth domain namespace consistency: aggregate folders now expose aggregate namespaces such as `EnglishTutor.Modules.Auth.Domain.AuthUser`, `AuthSession`, `AuthPermission`, and `AuthSecurityEvent`; no remaining `EnglishTutor.Modules.Auth.Domain.Entities` namespace in Auth source.
- [x] Auth manual audit field writes removed from domain methods: `AuthUser.Register`, `AuthRole.Create/Update`, `AuthPermission.Create/Update`, `AuthSession.Create`, and `AuthSecurityEvent.Create` no longer set `CreatedAtUtc` or `UpdatedAtUtc` manually. Business timestamps such as `LastUsedAtUtc`, `OccurredAtUtc`, `ReviewedAtUtc`, and revoke/suspicious timestamps remain domain state.

## Priority 2

- [x] Auth outbox mapping extracted to `IAuthDomainEventToOutboxMapper` / `AuthDomainEventToOutboxMapper`.
- [x] Shared outbox creation duplicate reduced with `OutboxMessageFactory`; producer DbContexts now call the factory instead of inline `new OutboxMessage`.
- [x] Worker outbox processor uses `IEnumerable<IModuleOutboxStore>` and `EfCoreModuleOutboxStore`; it no longer injects every module DbContext directly.
- [x] Auth aggregate boundaries clarified in code: `AuthSession`, `AuthPermission`, and `AuthSecurityEvent` are explicit `AggregateRoot<Guid>` types with aggregate-folder namespaces.
- [x] Permission codes exist for `StudyPlans`, `LearningContent`, `Exercises`, `Assessments`, and `Notifications` in `Auth.Contracts`.
- [x] Permission seed descriptions include `StudyPlans`, `LearningContent`, `Exercises`, `Assessments`, and `Notifications`, so `AuthDataSeeder` can seed all `PermissionCodes.All` entries.

## Priority 3

- [x] Unit test projects are not empty: Auth, Users, StudyPlans, LearningContent, Vocabulary, Exercises, Speaking, AI, Mistakes, Progress, Notifications, AdminReports, Worker, Integration, and Architecture tests all contain `[Fact]` tests.
- [x] `Microsoft.AspNetCore.Http.Abstractions` 2.3.0 is not present in `Directory.Packages.props` or project references; ASP.NET Core types are provided by the shared framework.
- [x] Add real database integration tests for module DbContexts, migrations, and persistence behavior.
- [x] Add API endpoint integration tests beyond smoke checks for auth/users/language settings and key learning flows.
- [x] Add messaging integration tests covering Outbox -> Worker -> Inbox idempotency and dead-letter/reprocess behavior against PostgreSQL.

## Priority 4

- [x] `scripts/Clear-TestDatabases.ps1` drops old `english_tutor_test_*` databases before a test run.
- [x] `scripts/Test.ps1` runs database cleanup before `dotnet test` by default.
- [x] Database integration tests use a unique `english_tutor_test_*` database name per run and clean stale test databases before creating a new one.
