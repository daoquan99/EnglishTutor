using System.Text.Json;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Feedback.Application.Abstractions.Persistence;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Repositories;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Feedback.Application.SessionFeedback.Commands.GenerateSessionFeedback;

public sealed class GenerateSessionFeedbackCommandHandler : ICommandHandler<GenerateSessionFeedbackCommand, Guid>
{
    private const string PromptVersion = "v1.0.0";
    
    private readonly ISessionFeedbackRepository _feedbackRepository;
    private readonly IFeedbackUnitOfWork _unitOfWork;
    private readonly IPracticeModule _practice;
    private readonly IAiGatewayModule _aiGateway;
    private readonly ILogger<GenerateSessionFeedbackCommandHandler> _logger;

    public GenerateSessionFeedbackCommandHandler(
        ISessionFeedbackRepository feedbackRepository,
        IFeedbackUnitOfWork unitOfWork,
        IPracticeModule practice,
        IAiGatewayModule aiGateway,
        ILogger<GenerateSessionFeedbackCommandHandler> logger)
    {
        _feedbackRepository = feedbackRepository;
        _unitOfWork = unitOfWork;
        _practice = practice;
        _aiGateway = aiGateway;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(GenerateSessionFeedbackCommand request, CancellationToken ct)
    {
        // 1. Check idempotency: if feedback already exists for this session, return it
        var existingFeedback = await _feedbackRepository.GetByPracticeSessionIdAsync(request.SessionId, ct);
        if (existingFeedback is not null)
        {
            if (existingFeedback.Status is FeedbackStatus.Success or FeedbackStatus.Pending)
            {
                return Result.Success(existingFeedback.Id);
            }
        }

        // 2. Fetch session details from Practice module
        var sessionResult = await _practice.GetSessionAsync(request.UserId, request.SessionId, ct);
        if (sessionResult.Status != PracticeQueryStatus.Success || sessionResult.Session is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Feedback.SessionNotFound", "Practice session not found."));
        }

        var sessionSummary = sessionResult.Session;

        // 3. Create a pending SessionFeedback aggregate
        var feedback = Domain.Aggregates.SessionFeedback.SessionFeedback.CreatePending(
            id: Guid.NewGuid(),
            practiceSessionId: request.SessionId,
            userId: request.UserId,
            languagePairId: sessionSummary.LanguagePairId,
            nativeLanguageCode: sessionSummary.NativeLanguageCode,
            targetLanguageCode: sessionSummary.TargetLanguageCode,
            explanationLanguageCode: sessionSummary.ExplanationLanguageCode);

        await _feedbackRepository.AddAsync(feedback, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Load transcript messages
        var transcriptResult = await _practice.GetTranscriptAsync(request.UserId, request.SessionId, ct);
        if (transcriptResult.Status != PracticeQueryStatus.Success || transcriptResult.Messages == null || transcriptResult.Messages.Count == 0)
        {
            feedback.CompleteFailed("empty_transcript");
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success(feedback.Id);
        }

        // Format transcript for AI model
        var transcriptText = string.Join("\n", transcriptResult.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => $"{m.Role}: {m.Content}"));

        // 5. Try to acquire an AI route lease from AiGateway
        var lease = await _aiGateway.CreateRouteLeaseAsync(new CreateRouteLeaseRequest
        {
            UserId = request.UserId,
            ActivityType = "feedback",
            TopicCode = sessionSummary.TopicCode,
            ScenarioCode = sessionSummary.ScenarioId.ToString("N"),
            IdempotencyKey = Guid.NewGuid().ToString(),
        }, ct);

        // Fallback to "practice" mode rule if "feedback" specific rule is not configured
        if (lease.Status == CreateRouteLeaseStatus.RouteNoMatch)
        {
            lease = await _aiGateway.CreateRouteLeaseAsync(new CreateRouteLeaseRequest
            {
                UserId = request.UserId,
                ActivityType = sessionSummary.ModeCode,
                TopicCode = sessionSummary.TopicCode,
                ScenarioCode = sessionSummary.ScenarioId.ToString("N"),
                IdempotencyKey = Guid.NewGuid().ToString(),
            }, ct);
        }

        if (lease.Status != CreateRouteLeaseStatus.Success || lease.LeaseId is not Guid leaseId)
        {
            _logger.LogWarning("Failed to acquire AI route lease for session {SessionId}. Status: {Status}", request.SessionId, lease.Status);
            feedback.CompleteFailed(lease.Status == CreateRouteLeaseStatus.RouteNoMatch ? "not_configured" : lease.Status.ToString());
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success(feedback.Id);
        }

        // 6. Build the prompts (no secrets included)
        var systemPrompt = $"You are an expert language tutor. The learner is practicing {sessionSummary.TargetLanguageCode}.\n" +
            $"Write corrected and natural examples in {sessionSummary.TargetLanguageCode}.\n" +
            $"Write summaries, meanings, and explanations in {sessionSummary.ExplanationLanguageCode}.\n" +
            @"Analyze the conversation transcript and return a detailed feedback JSON object.
You MUST output ONLY valid raw JSON matching this schema:
{
  ""summary"": ""overall session summary"",
  ""strengths"": ""what they did well"",
  ""improvement_areas"": ""what they can improve"",
  ""score"": 85,
  ""cefr_level"": ""B2"",
  ""corrections"": [
    {
      ""original_text"": ""original user sentence"",
      ""corrected_text"": ""corrected user sentence"",
      ""explanation"": ""explanation of the correction"",
      ""category"": ""grammar | vocabulary | pronunciation | syntax"",
      ""severity"": ""low | medium | high""
    }
  ],
  ""vocabulary"": [
    {
      ""term"": ""extracted term or phrase"",
      ""meaning"": ""meaning of the term"",
      ""example_sentence"": ""an example sentence using the term"",
      ""difficulty"": ""beginner | intermediate | advanced"",
      ""confidence"": 0.95
    }
  ],
  ""mistake_patterns"": [
    {
      ""pattern"": ""repeated mistake pattern"",
      ""description"": ""description of the pattern"",
      ""frequency"": 3
    }
  ]
}";

        var userPrompt = $"Analyze the following conversation transcript:\n\n{transcriptText}";

        // 7. Execute Chat Completion via AI Gateway
        var completionResult = await _aiGateway.ExecuteChatCompletionAsync(new ExecuteChatCompletionRequest
        {
            LeaseId = leaseId,
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            CorrelationId = request.SessionId
        }, ct);

        if (completionResult.Status != ExecuteChatCompletionStatus.Success || string.IsNullOrWhiteSpace(completionResult.ResponseText))
        {
            _logger.LogWarning("AI Chat completion failed for session {SessionId}. Status: {Status}", request.SessionId, completionResult.Status);
            feedback.CompleteFailed(completionResult.Status.ToString());
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success(feedback.Id);
        }

        // 8. Validate and parse the JSON response
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
            var responseJson = completionResult.ResponseText;
            
            // Clean markdown formatting if returned (e.g. ```json ... ```)
            if (responseJson.StartsWith("```"))
            {
                var lines = responseJson.Split('\n').ToList();
                if (lines.Count > 2)
                {
                    lines.RemoveAt(0);
                    if (lines[^1].StartsWith("```"))
                    {
                        lines.RemoveAt(lines.Count - 1);
                    }
                    responseJson = string.Join("\n", lines);
                }
            }

            var aiDto = JsonSerializer.Deserialize<FeedbackAiResponse>(responseJson, options);
            if (aiDto is null || string.IsNullOrWhiteSpace(aiDto.Summary))
            {
                throw new JsonException("Deserialized output is empty or lacks summary.");
            }

            // Map AI DTOs to child entities
            var corrections = (aiDto.Corrections ?? new())
                .Select(c => new Correction(
                    id: Guid.NewGuid(),
                    sessionFeedbackId: feedback.Id,
                    originalText: c.OriginalText,
                    correctedText: c.CorrectedText,
                    explanation: c.Explanation,
                    category: c.Category,
                    severity: c.Severity))
                .ToList();

            var vocabulary = (aiDto.Vocabulary ?? new())
                .Select(v => new ExtractedVocabulary(
                    id: Guid.NewGuid(),
                    sessionFeedbackId: feedback.Id,
                    term: v.Term,
                    meaning: v.Meaning,
                    exampleSentence: v.ExampleSentence,
                    difficulty: v.Difficulty,
                    confidence: v.Confidence))
                .ToList();

            var mistakePatterns = (aiDto.MistakePatterns ?? new())
                .Select(m => new MistakePattern(
                    id: Guid.NewGuid(),
                    sessionFeedbackId: feedback.Id,
                    pattern: m.Pattern,
                    description: m.Description,
                    frequency: m.Frequency))
                .ToList();

            // Transition aggregate status to Success
            feedback.CompleteSuccess(
                summary: aiDto.Summary,
                strengths: aiDto.Strengths,
                improvementAreas: aiDto.ImprovementAreas,
                score: aiDto.Score,
                cefrLevel: aiDto.CefrLevel,
                corrections: corrections,
                vocabulary: vocabulary,
                mistakePatterns: mistakePatterns);

            _feedbackRepository.Update(feedback);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success(feedback.Id);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI feedback JSON for session {SessionId}", request.SessionId);
            if (feedback.Status == FeedbackStatus.Pending)
            {
                feedback.CompleteFailed("invalid_ai_response_json");
                await _unitOfWork.SaveChangesAsync(ct);
            }
            return Result.Success(feedback.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI feedback for session {SessionId}", request.SessionId);
            if (feedback.Status == FeedbackStatus.Pending)
            {
                feedback.CompleteFailed("generation_failed");
                await _unitOfWork.SaveChangesAsync(ct);
            }
            throw;
        }
    }

    private sealed class FeedbackAiResponse
    {
        public string Summary { get; set; } = string.Empty;
        public string Strengths { get; set; } = string.Empty;
        public string ImprovementAreas { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string CefrLevel { get; set; } = string.Empty;
        public List<CorrectionItemAiDto>? Corrections { get; set; }
        public List<VocabularyItemAiDto>? Vocabulary { get; set; }
        public List<MistakePatternAiDto>? MistakePatterns { get; set; }
    }

    private sealed class CorrectionItemAiDto
    {
        public string OriginalText { get; set; } = string.Empty;
        public string CorrectedText { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    private sealed class VocabularyItemAiDto
    {
        public string Term { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string ExampleSentence { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    private sealed class MistakePatternAiDto
    {
        public string Pattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Frequency { get; set; }
    }
}
