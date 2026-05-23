---
name: test-writer
description: Adds domain unit, handler, integration, and architecture tests for new code. Targets the appropriate test project per scope.
---

# Test Writer

## When to invoke

- A new aggregate, command, query, repository, EF configuration, integration event, or Contract Reader has been added.
- A business rule changed.
- A new architecture rule needs enforcing.

## Where tests live

```
tests/
├── EnglishTutor.ArchitectureTests/                  # NetArchTest — boundaries
├── EnglishTutor.IntegrationTests/                   # Postgres-backed cross-module flows
└── EnglishTutor.Modules.{Module}.UnitTests/
    ├── Domain/                                      # aggregate / VO / rule tests
    └── Application/                                 # handler / validator tests
```

## Recipes

### Domain unit test

- Construct the aggregate via its factory / constructor; exercise the behavior; assert state and raised events.
- Test business rule violations: `Assert.Throws<BusinessRuleValidationException>(() => ...)`.
- No mocking — Domain is pure.

```csharp
[Fact]
public void Mark_Mastered_Should_Raise_Event_When_Threshold_Met()
{
    var mastery = UserVocabularyMastery.Start(userId, itemId, languageCode);
    mastery.Review(grade: ReviewGrade.Easy, now: utcNow);
    // ... enough correct reviews
    Assert.Contains(mastery.DomainEvents, e => e is VocabularyMastered);
}
```

### Handler test

- Fake repository implementing the Application interface.
- Fake `IDateTimeProvider`, `ICurrentUser`.
- For producer handlers: assert an `OutboxMessage` was added in the unit of work (if your fake captures it) or assert the integration event mapping in a separate test.

```csharp
[Fact]
public async Task Handle_Should_Return_NotFound_When_Mastery_Missing()
{
    var handler = new MarkMasteredCommandHandler(fakeRepo, fakeUow, fakeClock);
    var result = await handler.Handle(new MarkMasteredCommand(itemId), CancellationToken.None);
    Assert.True(result.IsFailure);
    Assert.Equal(VocabularyErrors.CardNotFound.Code, result.Error.Code);
}
```

### Validator test

```csharp
[Fact]
public void Should_Fail_When_Grade_Is_OutOfRange()
{
    var validator = new ReviewCommandValidator();
    var result = validator.TestValidate(new ReviewCommand(itemId, grade: 99));
    result.ShouldHaveValidationErrorFor(x => x.Grade);
}
```

### Integration test

- Uses the project's `WebApplicationFactory` / Postgres test container.
- Test EF query shapes, migrations, outbox→inbox handoff, projection updates.
- Each test boots a clean schema or uses `scripts/Clear-TestDatabases.ps1` between runs (`Test.ps1` runs this automatically).

```csharp
[Fact]
public async Task Outbox_Dispatcher_Should_Move_Processed_Rows()
{
    // arrange: write business data + outbox row in one tx
    // act: run OutboxProcessingJob once
    // assert: row has ProcessedOnUtc set; inbox row exists in consumer module
}
```

### Architecture test

```csharp
[Fact]
public void Vocabulary_Domain_Should_Not_Reference_Application()
{
    var result = Types.InAssembly(typeof(VocabularyItem).Assembly)
        .ShouldNot()
        .HaveDependencyOn("EnglishTutor.Modules.Vocabulary.Application")
        .GetResult();
    Assert.True(result.IsSuccessful);
}
```

## Conventions

- Naming: `Should_<Outcome>_When_<Condition>`.
- One arrange/act/assert per test.
- No shared mutable state between tests.
- Don't mock the database for tests that exercise EF query shape or migrations.
- Don't skip failing tests — explain why a test cannot run if necessary.

## Commands

```bash
cd EnglishTutor.API
dotnet test EnglishTutor.slnx                                            # all
dotnet test tests/EnglishTutor.Modules.{Module}.UnitTests                # one project
dotnet test --filter "FullyQualifiedName~<ClassOrMethodPattern>"         # filter
dotnet test tests/EnglishTutor.ArchitectureTests                         # architecture only
```

`./scripts/Test.ps1` runs the full suite with DB cleanup.

## Done when

- New code has at least one unit test per behavior branch.
- Architecture test exists for any new structural rule.
- Integration test covers any new DB query / outbox interaction.
- `dotnet test EnglishTutor.slnx` passes.
