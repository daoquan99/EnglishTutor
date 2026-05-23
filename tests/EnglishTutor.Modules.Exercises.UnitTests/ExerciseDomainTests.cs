using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Events;
using Xunit;

namespace EnglishTutor.Modules.Exercises.UnitTests;

public sealed class ExerciseDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 5, 19, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Start_CreatesInProgressAttempt()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 3, UtcNow);

        Assert.Equal(ExerciseAttemptStatus.InProgress, attempt.Status);
        Assert.Equal(3, attempt.TotalQuestions);
        Assert.Equal(UtcNow, attempt.StartedAtUtc);
        Assert.Single(attempt.DomainEvents);
    }

    [Fact]
    public void RecordAnswer_WhenCorrect_IncrementsCorrectCount()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 2, UtcNow);

        attempt.RecordAnswer(Guid.NewGuid(), "answer", true, 100, "ok", UtcNow);

        Assert.Equal(1, attempt.CorrectCount);
        Assert.Single(attempt.Answers);
    }

    [Fact]
    public void RecordAnswer_WhenIncorrect_DoesNotIncrementCorrectCount()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 2, UtcNow);

        attempt.RecordAnswer(Guid.NewGuid(), "answer", false, 0, "no", UtcNow);

        Assert.Equal(0, attempt.CorrectCount);
    }

    [Fact]
    public void RecordAnswer_WhenDuplicateQuestion_Throws()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 2, UtcNow);
        var questionId = Guid.NewGuid();
        attempt.RecordAnswer(questionId, "answer", true, 100, "ok", UtcNow);

        Assert.Throws<DomainException>(() => attempt.RecordAnswer(questionId, "again", true, 100, "ok", UtcNow));
    }

    [Fact]
    public void Complete_WhenMissingAnswers_Throws()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 2, UtcNow);
        attempt.RecordAnswer(Guid.NewGuid(), "answer", true, 100, "ok", UtcNow);

        Assert.Throws<DomainException>(() => attempt.Complete(UtcNow.AddMinutes(2), "MultipleChoice"));
    }

    [Fact]
    public void Complete_CalculatesPercentageScore()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 4, UtcNow);
        attempt.RecordAnswer(Guid.NewGuid(), "a", true, 100, null, UtcNow);
        attempt.RecordAnswer(Guid.NewGuid(), "b", true, 100, null, UtcNow);
        attempt.RecordAnswer(Guid.NewGuid(), "c", false, 0, null, UtcNow);
        attempt.RecordAnswer(Guid.NewGuid(), "d", false, 0, null, UtcNow);

        attempt.Complete(UtcNow.AddMinutes(5), "FillInTheBlank");

        Assert.Equal(ExerciseAttemptStatus.Completed, attempt.Status);
        Assert.Equal(50, attempt.Score);
        Assert.Equal(2, attempt.CorrectCount);
    }

    [Fact]
    public void Complete_AddsCompletedDomainEvent()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 1, UtcNow);
        var questionId = Guid.NewGuid();
        attempt.RecordAnswer(questionId, "wrong", false, 0, null, UtcNow);

        attempt.Complete(UtcNow.AddMinutes(1), "SentenceCorrection", [
            new ExerciseCompletedWrongAnswer(questionId, "prompt", "wrong", "right", "explanation", "SentenceCorrection")
        ]);

        var completedEvent = attempt.DomainEvents.OfType<ExerciseCompletedDomainEvent>().Single();
        Assert.Single(completedEvent.WrongAnswers);
        Assert.Equal("SentenceCorrection", completedEvent.ExerciseType);
    }

    [Fact]
    public void RecordAnswer_AfterCompleted_Throws()
    {
        var attempt = UserExerciseAttempt.Start(Guid.NewGuid(), Guid.NewGuid(), "en", 1, UtcNow);
        attempt.RecordAnswer(Guid.NewGuid(), "answer", true, 100, null, UtcNow);
        attempt.Complete(UtcNow.AddMinutes(1), "MultipleChoice");

        Assert.Throws<DomainException>(() => attempt.RecordAnswer(Guid.NewGuid(), "new", true, 100, null, UtcNow));
    }

    [Fact]
    public void ExerciseSet_PublishWithoutQuestions_Throws()
    {
        var set = ExerciseSet.Create("en", LanguageLevel.A1, "topic", LearningSkill.Grammar, ExerciseType.FillInTheBlank, "title", null, UtcNow);

        Assert.Throws<DomainException>(() => set.Publish(UtcNow));
    }

    [Fact]
    public void ExerciseSet_AddQuestion_IncrementsTotalQuestions()
    {
        var set = ExerciseSet.Create("en", LanguageLevel.A1, "topic", LearningSkill.Grammar, ExerciseType.FillInTheBlank, "title", null, UtcNow);

        set.AddQuestion(ExerciseType.FillInTheBlank, "I ___ a dev.", "am", "Use be.", 1, QuestionDifficulty.Easy, false, UtcNow);

        Assert.Equal(1, set.TotalQuestions);
    }

    [Fact]
    public void ExerciseSet_AiGradedQuestion_AllowsMissingCorrectAnswer()
    {
        var set = ExerciseSet.Create("en", LanguageLevel.A1, "topic", LearningSkill.Writing, ExerciseType.ShortWriting, "title", null, UtcNow);

        var question = set.AddQuestion(ExerciseType.ShortWriting, "Write two sentences.", null, null, 1, QuestionDifficulty.Easy, true, UtcNow);

        Assert.True(question.IsAiGraded);
        Assert.Null(question.CorrectAnswer);
    }

    [Fact]
    public void Grading_MultipleChoice_MatchesOptionId()
    {
        var question = CreateMultipleChoiceQuestion();
        var correctOption = question.Options.Single(option => option.IsCorrect);

        var result = new ExerciseGradingService().GradeStaticAnswer(question, correctOption.Id.ToString());

        Assert.True(result.IsCorrect);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Grading_MultipleChoice_MatchesOptionText()
    {
        var question = CreateMultipleChoiceQuestion();

        var result = new ExerciseGradingService().GradeStaticAnswer(question, "HELLO");

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grading_FillInTheBlank_IsCaseInsensitive()
    {
        var result = new ExerciseGradingService().GradeStaticAnswer(ExerciseType.FillInTheBlank, "  AM ", "am");

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grading_SentenceOrdering_NormalizesPunctuationAndSpaces()
    {
        var result = new ExerciseGradingService().GradeStaticAnswer(ExerciseType.SentenceOrdering, "My   name is Linh!", "My name is Linh.");

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grading_VerbConjugation_WhenWrong_ReturnsZero()
    {
        var result = new ExerciseGradingService().GradeStaticAnswer(ExerciseType.VerbConjugation, "go", "goes");

        Assert.False(result.IsCorrect);
        Assert.Equal(0, result.Score);
    }

    private static EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Entities.ExerciseQuestion CreateMultipleChoiceQuestion()
    {
        var set = ExerciseSet.Create("en", LanguageLevel.A1, "topic", LearningSkill.Vocabulary, ExerciseType.MultipleChoice, "title", null, UtcNow);
        var question = set.AddQuestion(ExerciseType.MultipleChoice, "Choose greeting", "Hello", null, 1, QuestionDifficulty.Easy, false, UtcNow);
        question.AddOption("Hello", true, 1, UtcNow);
        question.AddOption("Book", false, 2, UtcNow);
        return question;
    }
}
