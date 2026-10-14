using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishLiveTutor.Api.Data;
using EnglishLiveTutor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Api.Services;

public sealed class PronunciationAssessmentService
{
    public const string PromptVersion = "PROMPT_PRONUNCIATION_V1";
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly HttpClient _http;

    public PronunciationAssessmentService(IDbContextFactory<AppDbContext> dbFactory, HttpClient http)
    {
        _dbFactory = dbFactory;
        _http = http;
    }

    public async Task<List<AiModelConfigEntity>> GetAssessmentModelsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.AiModelCatalog
            .AsNoTracking()
            .Where(m => !m.SupportsLiveAudio)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<ScoringRubricEntity> GetActiveRubricAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var rubric = await db.ScoringRubrics
            .AsNoTracking()
            .Include(r => r.Criteria.OrderBy(c => c.DisplayOrder))
            .Where(r => r.IsActive && r.AssessmentType == "Pronunciation")
            .OrderByDescending(r => r.Version)
            .FirstOrDefaultAsync(ct);

        if (rubric is null)
            throw new InvalidOperationException("Chưa có cấu hình Scoring Rubric đang hoạt động.");

        return rubric;
    }

    public async Task<ScoringRubricEntity> UpdateRubricAsync(Guid rubricId, UpdateScoringRubricRequest req, CancellationToken ct = default)
    {
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

        existing.Name = string.IsNullOrWhiteSpace(req.Name) ? existing.Name : req.Name.Trim();
        existing.Description = string.IsNullOrWhiteSpace(req.Description) ? existing.Description : req.Description.Trim();
        existing.Version += 1;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        db.ScoringRubricCriteria.RemoveRange(existing.Criteria);
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
                MinimumScore = 0m,
                MaximumScore = 100m,
                DisplayOrder = order++,
                IsEnabled = true
            });
        }

        await db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<PronunciationAssessmentResultDto> AssessPronunciationAsync(
        string userId,
        SubmitPronunciationAssessmentRequest req,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ApiKey))
            throw new InvalidOperationException("Vui lòng nhập Google AI Studio API Key để chấm điểm phát âm.");

        if (string.IsNullOrWhiteSpace(req.Base64Audio))
            throw new InvalidOperationException("Không tìm thấy dữ liệu ghi âm. Vui lòng thu âm trước khi gửi chấm điểm.");

        byte[] audioBytes;
        try
        {
            audioBytes = Convert.FromBase64String(req.Base64Audio);
        }
        catch
        {
            throw new InvalidOperationException("Định dạng âm thanh Base64 không hợp lệ.");
        }

        if (audioBytes.Length < 256)
            throw new InvalidOperationException("Bản ghi âm quá ngắn hoặc rỗng. Vui lòng thu âm lại rõ ràng hơn.");

        if (audioBytes.Length > 10 * 1024 * 1024)
            throw new InvalidOperationException("Kích thước file ghi âm vượt quá giới hạn 10MB.");

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

        string effectiveModelCode = modelMeta.IsDeprecated && !string.IsNullOrWhiteSpace(modelMeta.RoutedModelCode)
            ? modelMeta.RoutedModelCode!
            : modelMeta.ModelCode;

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
        string ipaHint = vocab.Pronunciations.FirstOrDefault(p => p.Accent == accent)?.Ipa ?? "";

        // 3. Load active Scoring Rubric (Section 9)
        var rubric = await GetActiveRubricAsync(ct);
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

        var criteriaDescLines = string.Join("\n", enabledCriteria.Select(c =>
            $"- Code: \"{c.Code}\" (Weight: {c.Weight}%): {c.Description} Score range: 0 to 100."));

        string targetInstruction = targetType == "Word"
            ? "Target is an individual vocabulary WORD. Evaluate phonetic accuracy of vowels/consonants and syllable stress. For Fluency on a single word, evaluate natural smoothness and absence of awkward stumbling rather than penalizing short duration."
            : "Target is a complete EXAMPLE SENTENCE. Evaluate phonetic accuracy, word stress, intonation, connected speech, and natural sentence rhythm.";

        string systemPrompt = $@"You are an encouraging, precise English Pronunciation Assessor for Vietnamese learners.
Evaluate the attached user audio recording against the expected reference text.

Target Type: {targetType}
Expected Text: ""{expectedText}""
Target Accent: {accent} {(string.IsNullOrEmpty(ipaHint) ? "" : $"(Reference IPA: {ipaHint})")}
{targetInstruction}

Active Scoring Rubric ({rubric.Code} v{rubric.Version}):
{criteriaDescLines}

Rules:
1. Listen carefully to the user's audio. If the audio is completely silent, pure noise, or unintelligible, set ""isAssessable"": false and provide helpful Vietnamese guidance in ""improvements"".
2. Otherwise set ""isAssessable"": true, transcribe what the user actually said in ""recognizedText"", and score each criterion from 0 to 100.
3. Provide concise, actionable feedback in Vietnamese (`feedback`, `strengths`, `improvements`, `suggestedPractice`).
4. Return ONLY valid JSON matching this exact structure:
{{
  ""targetType"": ""{targetType}"",
  ""expectedText"": ""{expectedText}"",
  ""recognizedText"": ""string"",
  ""accent"": ""{accent}"",
  ""isAssessable"": true,
  ""confidence"": 0.92,
  ""criteria"": [
    {{ ""code"": ""Accuracy"", ""score"": 85, ""feedback"": ""Nhận xét tiếng Việt..."" }},
    {{ ""code"": ""Clarity"", ""score"": 90, ""feedback"": ""Nhận xét tiếng Việt..."" }},
    {{ ""code"": ""Fluency"", ""score"": 82, ""feedback"": ""Nhận xét tiếng Việt..."" }}
  ],
  ""strengths"": [""Điểm phát âm tốt bằng tiếng Việt""],
  ""improvements"": [""Điểm cần cải thiện bằng tiếng Việt""],
  ""suggestedPractice"": [""Bài tập luyện phát âm cụ thể bằng tiếng Việt""]
}}";

        string cleanMime = (req.ContentType ?? "audio/webm").Split(';')[0].Trim();
        if (string.IsNullOrWhiteSpace(cleanMime)) cleanMime = "audio/webm";

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{effectiveModelCode}:generateContent?key={Uri.EscapeDataString(req.ApiKey.Trim())}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = systemPrompt },
                        new
                        {
                            inlineData = new
                            {
                                mimeType = cleanMime,
                                data = req.Base64Audio
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        using var resp = await _http.PostAsJsonAsync(url, payload, ct);
        string rawBody = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Google AI Studio trả về lỗi {(int)resp.StatusCode} khi chấm phát âm: {rawBody}");
        }

        var root = JsonNode.Parse(rawBody);
        string? jsonText = root?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(jsonText))
            throw new InvalidOperationException("AI không trả về kết quả chấm phát âm hợp lệ.");

        jsonText = CleanJsonFence(jsonText);
        var parsed = JsonNode.Parse(jsonText);
        if (parsed is null)
            throw new InvalidOperationException("Không thể phân tích JSON chấm điểm phát âm.");

        bool isAssessable = parsed["isAssessable"]?.GetValue<bool>() ?? true;
        string? recognizedText = parsed["recognizedText"]?.GetValue<string>()?.Trim();
        decimal confidence = Math.Clamp(parsed["confidence"]?.GetValue<decimal>() ?? 0.85m, 0m, 1m);

        var strengths = ReadStringArray(parsed["strengths"]);
        var improvements = ReadStringArray(parsed["improvements"]);
        var suggestedPractice = ReadStringArray(parsed["suggestedPractice"]);

        var assessmentId = Guid.NewGuid();
        var criterionEntities = new List<PronunciationCriterionScoreEntity>();
        var criterionDtos = new List<CriterionScoreResultDto>();

        var aiCriteriaArray = parsed["criteria"]?.AsArray();

        foreach (var rubricCrit in enabledCriteria)
        {
            JsonNode? matchedNode = aiCriteriaArray?.FirstOrDefault(x =>
                string.Equals(x?["code"]?.GetValue<string>(), rubricCrit.Code, StringComparison.OrdinalIgnoreCase));

            decimal rawScore = isAssessable
                ? Math.Clamp(matchedNode?["score"]?.GetValue<decimal>() ?? 75m, rubricCrit.MinimumScore, rubricCrit.MaximumScore)
                : 0m;

            string fb = matchedNode?["feedback"]?.GetValue<string>()?.Trim()
                ?? (isAssessable ? "Phát âm khá rõ ràng." : "Không nhận diện được giọng nói rõ ràng.");

            // Section 9.4: Deterministic Backend Weighted Score Formula: OverallScore = SUM(CriterionScore * CriterionWeight / 100)
            decimal weightedScore = Math.Round(rawScore * rubricCrit.Weight / 100.00m, 2, MidpointRounding.AwayFromZero);

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

        decimal? overallScore = isAssessable
            ? Math.Round(criterionEntities.Sum(c => c.WeightedScore), 2, MidpointRounding.AwayFromZero)
            : null;

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
            ModelCode = req.AssessmentModelCode,
            RubricId = rubric.Id,
            RubricVersion = rubric.Version,
            AudioRecordingId = recording.Id,
            RecognizedText = recognizedText,
            OverallScore = overallScore,
            Confidence = confidence,
            Status = isAssessable ? "Completed" : "NotAssessable",
            StrengthsJson = JsonSerializer.Serialize(strengths),
            ImprovementsJson = JsonSerializer.Serialize(improvements),
            SuggestedPracticeJson = JsonSerializer.Serialize(suggestedPractice),
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            CriterionScores = criterionEntities
        };

        db.PronunciationAssessments.Add(assessmentEntity);
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
            IsAssessable: isAssessable,
            Status: assessmentEntity.Status,
            Criteria: criterionDtos,
            Strengths: strengths,
            Improvements: improvements,
            SuggestedPractice: suggestedPractice,
            CreatedAt: assessmentEntity.CreatedAt
        );
    }

    public async Task<List<PronunciationAssessmentResultDto>> GetAssessmentHistoryAsync(
        string userId,
        Guid? vocabularyId = null,
        int limit = 20,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var query = db.PronunciationAssessments
            .AsNoTracking()
            .Include(a => a.CriterionScores)
            .Where(a => a.UserId == userId);

        if (vocabularyId.HasValue)
        {
            query = query.Where(a => a.VocabularyId == vocabularyId.Value);
        }

        var list = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(Math.Clamp(limit, 1, 100))
            .ToListAsync(ct);

        return list.Select(a => new PronunciationAssessmentResultDto(
            AssessmentId: a.Id,
            VocabularyId: a.VocabularyId,
            SenseId: a.SenseId,
            TargetType: a.TargetType,
            TargetId: a.TargetId,
            ExpectedText: a.ReferenceText,
            RecognizedText: a.RecognizedText,
            Accent: a.Accent,
            ModelCode: a.ModelCode,
            RubricCode: "ENGLISH_PRONUNCIATION_V1",
            RubricVersion: a.RubricVersion,
            OverallScore: a.OverallScore,
            Confidence: a.Confidence,
            IsAssessable: a.Status == "Completed",
            Status: a.Status,
            Criteria: a.CriterionScores.Select(c => new CriterionScoreResultDto(
                c.CriterionCode,
                c.CriterionName,
                c.CriterionDescriptionSnapshot,
                c.WeightSnapshot,
                c.RawScore,
                c.WeightedScore,
                c.Feedback
            )).ToList(),
            Strengths: DeserializeList(a.StrengthsJson),
            Improvements: DeserializeList(a.ImprovementsJson),
            SuggestedPractice: DeserializeList(a.SuggestedPracticeJson),
            CreatedAt: a.CreatedAt
        )).ToList();
    }

    private static List<string> ReadStringArray(JsonNode? node)
    {
        var result = new List<string>();
        if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                string? val = item?.GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(val)) result.Add(val);
            }
        }
        return result;
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static string CleanJsonFence(string raw)
    {
        string t = raw.Trim();
        if (t.StartsWith("```"))
        {
            int firstNewline = t.IndexOf('\n');
            if (firstNewline >= 0) t = t[(firstNewline + 1)..];
            if (t.EndsWith("```")) t = t[..^3];
        }
        return t.Trim();
    }
}
