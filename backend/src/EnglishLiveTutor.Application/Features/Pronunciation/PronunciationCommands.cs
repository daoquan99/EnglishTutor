using System.Text.Json;
using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Pronunciation;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Pronunciation;

public sealed record AssessPronunciationCommand(
    string UserId,
    SubmitPronunciationAssessmentRequest Request
) : ICommand<PronunciationAssessmentResultDto>;

public sealed class AssessPronunciationCommandHandler : ICommandHandler<AssessPronunciationCommand, PronunciationAssessmentResultDto>
{
    private readonly IAppDbContextFactory _dbFactory;
    private readonly IPronunciationAiAssessor _aiAssessor;

    public AssessPronunciationCommandHandler(
        IAppDbContextFactory dbFactory,
        IPronunciationAiAssessor aiAssessor)
    {
        _dbFactory = dbFactory;
        _aiAssessor = aiAssessor;
    }

    public async Task<PronunciationAssessmentResultDto> HandleAsync(AssessPronunciationCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        var userId = command.UserId;

        if (string.IsNullOrWhiteSpace(req.ApiKey))
            throw new InvalidOperationException("Cần cung cấp API Key của Google Gemini để chấm phát âm Audio.");

        if (string.IsNullOrWhiteSpace(req.Base64Audio))
            throw new InvalidOperationException("Bản ghi âm không được để trống.");

        byte[] audioBytes;
        try
        {
            audioBytes = Convert.FromBase64String(req.Base64Audio);
        }
        catch
        {
            throw new InvalidOperationException("Định dạng âm thanh base64 không hợp lệ.");
        }

        if (req.DurationMilliseconds < 250 || req.DurationMilliseconds > 60000)
            throw new InvalidOperationException("Thời lượng bản ghi âm phải từ 0.25 giây đến 60 giây.");

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // 1. Validate selected model capabilities (Section 5.2)
        var modelMeta = await db.AiModelCatalog
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ModelCode == req.AssessmentModelCode, ct);

        if (modelMeta is null)
            throw new InvalidOperationException($"Model '{req.AssessmentModelCode}' không tồn tại trong hệ thống.");

        if (!modelMeta.SupportsAudioInput || !modelMeta.SupportsPronunciationAssessment)
        {
            throw new InvalidOperationException(
                $"Model '{modelMeta.DisplayName}' ({modelMeta.ModelCode}) KHÔNG hỗ trợ đầu vào âm thanh trực tiếp để chấm phát âm. " +
                "Vui lòng chọn các dòng hỗ trợ Audio như Gemini 3.8 Flash, Gemini 3.6 Flash, hoặc Gemini 3.5 Flash Lite.");
        }

        string effectiveModelCode = modelMeta.GetEffectiveModelCode();

        // 2. Resolve authoritative expected reference text server-side (Section 10.2)
        var vocab = await db.Vocabularies
            .AsNoTracking()
            .Include(v => v.Pronunciations)
            .Include(v => v.Senses).ThenInclude(s => s.Examples)
            .FirstOrDefaultAsync(v => v.Id == req.VocabularyId, ct);

        if (vocab is null)
            throw new InvalidOperationException("Không tìm thấy từ vựng cần chấm phát âm.");

        string targetType = string.Equals(req.TargetType, "Sentence", StringComparison.OrdinalIgnoreCase) ? "Sentence" : "Word";
        string expectedText = vocab.Word;

        if (targetType == "Sentence")
        {
            var allExamples = vocab.Senses.SelectMany(s => s.Examples).ToList();
            var matchedEx = req.TargetId.HasValue
                ? allExamples.FirstOrDefault(e => e.Id == req.TargetId.Value)
                : allExamples.FirstOrDefault();

            if (matchedEx is not null)
            {
                expectedText = matchedEx.SentenceEn;
            }
            else if (!string.IsNullOrWhiteSpace(req.ReferenceText))
            {
                expectedText = req.ReferenceText.Trim();
            }
        }

        string accent = string.Equals(req.Accent, "UK", StringComparison.OrdinalIgnoreCase) ? "UK" : "US";

        // 3. Load active Scoring Rubric (Section 9)
        var rubric = await db.ScoringRubrics
            .AsNoTracking()
            .Include(r => r.Criteria.OrderBy(c => c.DisplayOrder))
            .Where(r => r.IsActive && r.AssessmentType == "Pronunciation")
            .OrderByDescending(r => r.Version)
            .FirstOrDefaultAsync(ct);

        if (rubric is null)
            throw new InvalidOperationException("Không tìm thấy tiêu chí chấm điểm phát âm (Rubric) đang hoạt động.");

        var enabledCriteria = rubric.Criteria.Where(c => c.IsEnabled).OrderBy(c => c.DisplayOrder).ToList();

        // 4. Persist AudioRecording metadata (Section 11.3)
        var recording = new AudioRecordingEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StorageKey = $"mem://recordings/{userId}/{Guid.NewGuid():N}",
            ContentType = string.IsNullOrWhiteSpace(req.ContentType) ? "audio/webm" : req.ContentType,
            FileSizeBytes = audioBytes.Length,
            DurationMilliseconds = req.DurationMilliseconds,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            DeletionStatus = "Retained"
        };
        db.AudioRecordings.Add(recording);

        // 5. Call AI Assessor
        var rawResult = await _aiAssessor.AssessAudioAsync(
            req.ApiKey.Trim(),
            effectiveModelCode,
            req.Base64Audio,
            recording.ContentType,
            expectedText,
            accent,
            rubric,
            ct);

        var assessmentId = Guid.NewGuid();
        var criterionEntities = new List<PronunciationCriterionScoreEntity>();
        var criterionDtos = new List<CriterionScoreResultDto>();

        foreach (var rubricCrit in enabledCriteria)
        {
            decimal rawScore = 0m;
            string fb = rawResult.IsAssessable ? "Phát âm khá rõ ràng." : "Không nhận diện được giọng nói rõ ràng.";

            if (rawResult.CriterionScores.TryGetValue(rubricCrit.Code, out var parsedCrit))
            {
                rawScore = parsedCrit.RawScore;
                if (!string.IsNullOrWhiteSpace(parsedCrit.Feedback)) fb = parsedCrit.Feedback;
            }

            decimal weightedScore = PronunciationAssessmentEntity.CalculateWeightedScore(rawScore, rubricCrit.Weight);

            criterionEntities.Add(new PronunciationCriterionScoreEntity
            {
                Id = Guid.NewGuid(),
                AssessmentId = assessmentId,
                CriterionCode = rubricCrit.Code,
                CriterionName = rubricCrit.Name,
                CriterionDescriptionSnapshot = rubricCrit.Description,
                WeightSnapshot = rubricCrit.Weight,
                RawScore = rawScore,
                WeightedScore = weightedScore,
                Feedback = fb
            });

            criterionDtos.Add(new CriterionScoreResultDto(
                Code: rubricCrit.Code,
                Name: rubricCrit.Name,
                Description: rubricCrit.Description,
                Weight: rubricCrit.Weight,
                Score: rawScore,
                WeightedScore: weightedScore,
                Feedback: fb
            ));
        }

        decimal? overallScore = PronunciationAssessmentEntity.CalculateOverallScore(
            criterionEntities.Select(c => c.WeightedScore),
            rawResult.IsAssessable);

        var assessmentEntity = new PronunciationAssessmentEntity
        {
            Id = assessmentId,
            UserId = userId,
            VocabularyId = req.VocabularyId,
            SenseId = req.SenseId,
            TargetType = targetType,
            TargetId = req.TargetId,
            ReferenceText = expectedText,
            Accent = accent,
            ModelCode = effectiveModelCode,
            RubricId = rubric.Id,
            RubricVersion = rubric.Version,
            AudioRecordingId = recording.Id,
            RecognizedText = rawResult.RecognizedText,
            OverallScore = overallScore,
            Confidence = rawResult.Confidence,
            Status = rawResult.IsAssessable ? "Completed" : "NotAssessable",
            StrengthsJson = JsonSerializer.Serialize(rawResult.Strengths),
            ImprovementsJson = JsonSerializer.Serialize(rawResult.Improvements),
            SuggestedPracticeJson = JsonSerializer.Serialize(rawResult.SuggestedPractice),
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            CriterionScores = criterionEntities
        };

        db.PronunciationAssessments.Add(assessmentEntity);

        // Update User progress with latest pronunciation score
        var progress = await db.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SenseId == req.SenseId, ct);
        if (progress is not null && overallScore.HasValue)
        {
            progress.LatestPronunciationScore = overallScore.Value;
        }

        await db.SaveChangesAsync(ct);

        return new PronunciationAssessmentResultDto(
            AssessmentId: assessmentEntity.Id,
            VocabularyId: assessmentEntity.VocabularyId,
            SenseId: assessmentEntity.SenseId,
            TargetType: assessmentEntity.TargetType,
            TargetId: assessmentEntity.TargetId,
            ExpectedText: assessmentEntity.ReferenceText,
            RecognizedText: assessmentEntity.RecognizedText,
            Accent: assessmentEntity.Accent,
            ModelCode: assessmentEntity.ModelCode,
            RubricCode: rubric.Code,
            RubricVersion: rubric.Version,
            OverallScore: assessmentEntity.OverallScore,
            Confidence: assessmentEntity.Confidence,
            IsAssessable: rawResult.IsAssessable,
            Status: assessmentEntity.Status,
            Criteria: criterionDtos,
            Strengths: rawResult.Strengths,
            Improvements: rawResult.Improvements,
            SuggestedPractice: rawResult.SuggestedPractice,
            CreatedAt: assessmentEntity.CreatedAt
        );
    }
}

public sealed record UpdateScoringRubricCommand(
    Guid RubricId,
    UpdateScoringRubricRequest Request
) : ICommand<ScoringRubricEntity>;

public sealed class UpdateScoringRubricCommandHandler : ICommandHandler<UpdateScoringRubricCommand, ScoringRubricEntity>
{
    private readonly IAppDbContextFactory _dbFactory;

    public UpdateScoringRubricCommandHandler(IAppDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<ScoringRubricEntity> HandleAsync(UpdateScoringRubricCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        var rubricId = command.RubricId;

        if (req.Criteria is null || req.Criteria.Count == 0)
            throw new InvalidOperationException("Rubric phải có ít nhất 1 tiêu chí chấm điểm.");

        decimal totalWeight = req.Criteria.Sum(c => c.Weight);
        if (Math.Abs(totalWeight - 100.00m) > 0.01m)
            throw new InvalidOperationException($"Tổng trọng số các tiêu chí phải bằng 100% (hiện tại: {totalWeight}%).");

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var existing = await db.ScoringRubrics
            .Include(r => r.Criteria)
            .FirstOrDefaultAsync(r => r.Id == rubricId, ct);

        if (existing is null)
            throw new InvalidOperationException("Không tìm thấy cấu hình Rubric.");

        existing.Name = req.Name?.Trim() ?? existing.Name;
        existing.Description = req.Description?.Trim() ?? existing.Description;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        existing.Criteria.Clear();
        int order = 1;
        foreach (var c in req.Criteria)
        {
            existing.Criteria.Add(new ScoringRubricCriterionEntity
            {
                Id = Guid.NewGuid(),
                RubricId = existing.Id,
                Code = c.Code.Trim(),
                Name = c.Name.Trim(),
                Description = c.Description.Trim(),
                Weight = c.Weight,
                DisplayOrder = order++
            });
        }

        existing.ValidateWeights();
        await db.SaveChangesAsync(ct);
        return existing;
    }
}
