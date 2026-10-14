using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using EnglishLiveTutor.Api.Data;
using EnglishLiveTutor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Api.Services;

/// <summary>
/// Implements AI Vocabulary Generation (Section 6), Daily Vocabulary Plan & Backlog Carry-Over (Section 7),
/// Spaced Repetition SM-2 Scheduling (Section 7.4), and Flashcard DTO Projections (Section 8).
/// </summary>
public sealed class VocabularyLearningService
{
    public const string PromptVersion = "PROMPT_VOCAB_GEN_V1";
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly HttpClient _http;

    public VocabularyLearningService(IDbContextFactory<AppDbContext> dbFactory, HttpClient http)
    {
        _dbFactory = dbFactory;
        _http = http;
    }

    public async Task<List<AiModelConfigEntity>> GetGenerationModelsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.AiModelCatalog
            .AsNoTracking()
            .Where(m => m.SupportsTextGeneration && !m.SupportsLiveAudio)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(ct);
    }

    public static string NormalizeWord(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        return string.Join(' ', raw.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string ResolveLocalLearningDate(string? timeZoneId)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
                return localNow.ToString("yyyy-MM-dd");
            }
        }
        catch
        {
            // Fallback to UTC+7 (Asia/Ho_Chi_Minh)
        }
        return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("yyyy-MM-dd");
    }

    public async Task<VocabularyFlashcardDto?> GetFlashcardByVocabularyIdAsync(
        Guid vocabularyId,
        Guid? senseId,
        string userId = "default-learner",
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var vocab = await db.Vocabularies
            .AsNoTracking()
            .Include(v => v.Pronunciations)
            .Include(v => v.WordFamilies)
            .Include(v => v.Senses).ThenInclude(s => s.Relations)
            .Include(v => v.Senses).ThenInclude(s => s.Collocations)
            .Include(v => v.Senses).ThenInclude(s => s.Examples)
            .Include(v => v.Senses).ThenInclude(s => s.GrammarPatterns)
            .FirstOrDefaultAsync(v => v.Id == vocabularyId, ct);

        if (vocab is null) return null;
        var sense = senseId.HasValue
            ? vocab.Senses.FirstOrDefault(s => s.Id == senseId.Value) ?? vocab.Senses.FirstOrDefault()
            : vocab.Senses.OrderByDescending(s => s.IsPrimary).ThenBy(s => s.DisplayOrder).FirstOrDefault();

        if (sense is null) return null;

        var progress = await db.UserVocabularyProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SenseId == sense.Id, ct);

        var latestScore = await db.PronunciationAssessments
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.SenseId == sense.Id && a.OverallScore != null)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.OverallScore)
            .FirstOrDefaultAsync(ct);

        return MapToFlashcardDto(Guid.Empty, vocab, sense, "New", "Pending", progress, latestScore);
    }

    public async Task<DailyVocabularyPlanResponseDto> GetOrCreateDailyPlanAsync(
        string userId,
        InitializeDailyPlanRequest req,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        string learningDate = ResolveLocalLearningDate(req.TimeZoneId);
        int targetNew = Math.Clamp(req.NewVocabularyTarget <= 0 ? 20 : req.NewVocabularyTarget, 1, 100);
        var now = DateTimeOffset.UtcNow;

        var plan = await db.DailyVocabularyPlans
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LearningDate == learningDate, ct);

        if (plan is null)
        {
            plan = new DailyVocabularyPlanEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LearningDate = learningDate,
                TimeZoneId = string.IsNullOrWhiteSpace(req.TimeZoneId) ? "Asia/Ho_Chi_Minh" : req.TimeZoneId,
                NewVocabularyTarget = targetNew,
                Status = "InProgress",
                CreatedAt = now
            };
            db.DailyVocabularyPlans.Add(plan);
            await db.SaveChangesAsync(ct);
        }
        else if (plan.NewVocabularyTarget != targetNew && req.NewVocabularyTarget > 0)
        {
            plan.NewVocabularyTarget = targetNew;
            await db.SaveChangesAsync(ct);
        }

        var existingSenseIdsInPlan = plan.Items.Select(i => i.SenseId).ToHashSet();
        int nextOrder = plan.Items.Count == 0 ? 1 : plan.Items.Max(i => i.DisplayOrder) + 1;

        // 1. Carry-over unfinished items from earlier dates (Section 7.1)
        var unfinishedPastItems = await (
            from i in db.DailyVocabularyItems
            join p in db.DailyVocabularyPlans on i.PlanId equals p.Id
            where p.UserId == userId &&
                  p.LearningDate != learningDate &&
                  i.Status != "Completed"
            orderby i.AssignedAt
            select i
        ).ToListAsync(ct);

        foreach (var pastItem in unfinishedPastItems)
        {
            if (existingSenseIdsInPlan.Add(pastItem.SenseId))
            {
                db.DailyVocabularyItems.Add(new DailyVocabularyItemEntity
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    VocabularyId = pastItem.VocabularyId,
                    SenseId = pastItem.SenseId,
                    ItemType = "CarryOver",
                    Status = "Pending",
                    AssignedAt = now,
                    DisplayOrder = nextOrder++
                });
            }
        }

        // 2. Pending due reviews (NextReviewAt <= now)
        var dueProgresses = await db.UserVocabularyProgresses
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.NextReviewAt != null && p.NextReviewAt <= now)
            .OrderBy(p => p.NextReviewAt)
            .Take(30)
            .ToListAsync(ct);

        foreach (var due in dueProgresses)
        {
            if (existingSenseIdsInPlan.Add(due.SenseId))
            {
                db.DailyVocabularyItems.Add(new DailyVocabularyItemEntity
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    VocabularyId = due.VocabularyId,
                    SenseId = due.SenseId,
                    ItemType = "Review",
                    Status = "Pending",
                    AssignedAt = now,
                    DisplayOrder = nextOrder++
                });
            }
        }

        await db.SaveChangesAsync(ct);

        // Reload plan items to check how many New items are currently assigned
        var currentPlanItems = await db.DailyVocabularyItems
            .Where(i => i.PlanId == plan.Id)
            .ToListAsync(ct);
        existingSenseIdsInPlan = currentPlanItems.Select(i => i.SenseId).ToHashSet();

        int currentNewCount = currentPlanItems.Count(i => i.ItemType == "New");
        int neededNewCount = Math.Max(0, plan.NewVocabularyTarget - currentNewCount);

        if (neededNewCount > 0)
        {
            var learnedSenseIds = await db.UserVocabularyProgresses
                .Where(p => p.UserId == userId)
                .Select(p => p.SenseId)
                .ToListAsync(ct);
            var excludedSenses = existingSenseIdsInPlan.Union(learnedSenseIds).ToHashSet();

            var candidateSenses = await (
                from s in db.VocabularySenses.AsNoTracking()
                join v in db.Vocabularies.AsNoTracking() on s.VocabularyId equals v.Id
                where v.IsActive && !excludedSenses.Contains(s.Id)
                orderby v.FrequencyRank ?? 99999, s.DisplayOrder
                select s
            )
            .Take(neededNewCount)
            .ToListAsync(ct);

            foreach (var sense in candidateSenses)
            {
                if (existingSenseIdsInPlan.Add(sense.Id))
                {
                    db.DailyVocabularyItems.Add(new DailyVocabularyItemEntity
                    {
                        Id = Guid.NewGuid(),
                        PlanId = plan.Id,
                        VocabularyId = sense.VocabularyId,
                        SenseId = sense.Id,
                        ItemType = "New",
                        Status = "Pending",
                        AssignedAt = now,
                        DisplayOrder = nextOrder++
                    });
                    neededNewCount--;
                }
            }

            await db.SaveChangesAsync(ct);
        }

        // 3. Generate additional vocabulary using AI when requested and API key is supplied
        if (req.GenerateWithAiIfNeeded && neededNewCount > 0 && !string.IsNullOrWhiteSpace(req.ApiKey))
        {
            int batchToGenerate = Math.Min(neededNewCount, 5);
            await GenerateVocabularyWithAiAsync(
                userId,
                new GenerateVocabularyRequest(
                    ApiKey: req.ApiKey!,
                    ModelCode: req.GenerationModelCode ?? "gemini-3.8-flash",
                    RequestedCount: batchToGenerate,
                    CefrLevel: req.CefrLevel,
                    Topic: req.Topic,
                    ExcludedWords: null,
                    AssignToTodayPlan: true
                ),
                ct);
        }

        return await BuildDailyPlanResponseAsync(db, plan.Id, userId, ct);
    }

    public async Task<List<VocabularyFlashcardDto>> GenerateVocabularyWithAiAsync(
        string userId,
        GenerateVocabularyRequest req,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.ApiKey))
            throw new InvalidOperationException("Vui lòng nhập Google AI Studio API Key để tạo từ vựng bằng AI.");

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        string requestedModelCode = string.IsNullOrWhiteSpace(req.ModelCode) ? "gemini-3.8-flash" : req.ModelCode.Trim();

        var modelMeta = await db.AiModelCatalog
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ModelCode == requestedModelCode, ct);

        if (modelMeta is null)
            throw new InvalidOperationException($"Model '{requestedModelCode}' không tồn tại trong AI Model Catalog.");

        if (!modelMeta.IsEnabled || !modelMeta.IsAvailable || !modelMeta.SupportsTextGeneration || modelMeta.SupportsLiveAudio)
            throw new InvalidOperationException($"Model '{modelMeta.DisplayName}' không hỗ trợ tạo từ vựng cấu trúc JSON.");

        // Section 5.2: Gemini 3.7 Flash is deprecated and routes to Gemini 3.8 Flash
        string effectiveModelCode = modelMeta.IsDeprecated && !string.IsNullOrWhiteSpace(modelMeta.RoutedModelCode)
            ? modelMeta.RoutedModelCode!
            : modelMeta.ModelCode;

        int requestedCount = Math.Clamp(req.RequestedCount, 1, 10);
        string cefr = string.IsNullOrWhiteSpace(req.CefrLevel) ? "B1" : req.CefrLevel.Trim().ToUpperInvariant();
        string topic = string.IsNullOrWhiteSpace(req.Topic) ? "General English & Career Communication" : req.Topic.Trim();

        var existingWords = await db.Vocabularies
            .AsNoTracking()
            .Select(v => v.NormalizedWord)
            .ToListAsync(ct);

        var excludedList = existingWords
            .Concat(req.ExcludedWords ?? new List<string>())
            .Distinct()
            .Take(80)
            .ToList();

        string prompt = $@"You are an expert English lexicographer and curriculum designer for Vietnamese learners.
Generate {requestedCount} distinct, high-utility English vocabulary items tailored to CEFR level {cefr} and topic '{topic}'.
Do NOT include any of these already-existing words: {string.Join(", ", excludedList)}.

Strict Rules:
1. Return ONLY valid JSON matching the requested schema (a JSON array of objects).
2. Provide accurate UK and US IPA transcriptions, syllable counts, and stress patterns only when confident.
3. Provide clear English definitions (`definitionEn`), accurate Vietnamese meanings (`meaningVi`), synonyms, antonyms, word family members, natural collocations with Vietnamese meanings, 2 practical example sentences (`sentenceEn` + `sentenceVi`), and grammar patterns.

JSON Schema per item in array:
[
  {{
    ""word"": ""string"",
    ""partOfSpeech"": ""noun | verb | adjective | adverb | phrase"",
    ""cefrLevel"": ""{cefr}"",
    ""definitionEn"": ""string"",
    ""meaningVi"": ""string"",
    ""descriptionEn"": ""string"",
    ""ipaUk"": ""/string/"",
    ""ipaUs"": ""/string/"",
    ""syllableCount"": 3,
    ""stressPattern"": ""string"",
    ""synonyms"": [""string""],
    ""antonyms"": [""string""],
    ""wordFamilies"": [{{ ""relatedWord"": ""string"", ""partOfSpeech"": ""string"" }}],
    ""collocations"": [{{ ""phraseEn"": ""string"", ""meaningVi"": ""string"" }}],
    ""examples"": [{{ ""sentenceEn"": ""string"", ""sentenceVi"": ""string"" }}],
    ""grammarPatterns"": [{{ ""pattern"": ""string"", ""explanation"": ""string"", ""example"": ""string"" }}]
  }}
]";

        var audit = new VocabularyGenerationAuditEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ModelCode = requestedModelCode,
            PromptVersion = PromptVersion,
            RequestedCount = requestedCount,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createdSenseIds = new List<(Guid VocabId, Guid SenseId)>();

        try
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{effectiveModelCode}:generateContent?key={Uri.EscapeDataString(req.ApiKey.Trim())}";

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new
                {
                    temperature = 0.4,
                    responseMimeType = "application/json"
                }
            };

            using var resp = await _http.PostAsJsonAsync(url, payload, ct);
            string rawBody = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Google AI Studio trả về lỗi {(int)resp.StatusCode}: {rawBody}");
            }

            var root = JsonNode.Parse(rawBody);
            string? jsonText = root?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(jsonText))
                throw new InvalidOperationException("AI không trả về dữ liệu JSON hợp lệ.");

            jsonText = CleanJsonFence(jsonText);
            var itemsArray = JsonNode.Parse(jsonText)?.AsArray();
            if (itemsArray is null)
                throw new InvalidOperationException("Cấu trúc JSON từ AI không phải là mảng từ vựng.");

            int accepted = 0;
            int rejected = 0;

            foreach (var itemNode in itemsArray)
            {
                string rawWord = itemNode?["word"]?.GetValue<string>()?.Trim() ?? "";
                string normalized = NormalizeWord(rawWord);
                string pos = itemNode?["partOfSpeech"]?.GetValue<string>()?.Trim() ?? "noun";
                string defEn = itemNode?["definitionEn"]?.GetValue<string>()?.Trim() ?? "";
                string meanVi = itemNode?["meaningVi"]?.GetValue<string>()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(defEn) || string.IsNullOrWhiteSpace(meanVi))
                {
                    rejected++;
                    continue;
                }

                // Section 6.3 Duplicate Prevention: Check existing catalog by NormalizedWord
                var existingVocab = await db.Vocabularies
                    .Include(v => v.Senses)
                    .FirstOrDefaultAsync(v => v.NormalizedWord == normalized && v.LanguageCode == "en", ct);

                if (existingVocab is not null)
                {
                    bool sameSenseExists = existingVocab.Senses.Any(s =>
                        string.Equals(s.PartOfSpeech, pos, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(s.MeaningVi.Trim(), meanVi, StringComparison.OrdinalIgnoreCase));

                    if (sameSenseExists)
                    {
                        var matchedSense = existingVocab.Senses.First();
                        createdSenseIds.Add((existingVocab.Id, matchedSense.Id));
                        rejected++;
                        continue;
                    }

                    // Add new sense to existing vocabulary (e.g. polysemous word)
                    var newSense = BuildSenseFromJson(existingVocab.Id, itemNode!, pos, cefr, defEn, meanVi, existingVocab.Senses.Count + 1);
                    db.VocabularySenses.Add(newSense);
                    await db.SaveChangesAsync(ct);
                    createdSenseIds.Add((existingVocab.Id, newSense.Id));
                    accepted++;
                }
                else
                {
                    var vocabId = Guid.NewGuid();
                    var newVocab = new VocabularyEntity
                    {
                        Id = vocabId,
                        Word = rawWord,
                        NormalizedWord = normalized,
                        LanguageCode = "en",
                        SourceType = "AI",
                        SourceModelId = effectiveModelCode,
                        TopicCategory = topic,
                        CreatedAt = DateTimeOffset.UtcNow,
                        IsActive = true
                    };

                    string ipaUk = itemNode?["ipaUk"]?.GetValue<string>()?.Trim() ?? "";
                    string ipaUs = itemNode?["ipaUs"]?.GetValue<string>()?.Trim() ?? "";
                    int? syllables = itemNode?["syllableCount"]?.GetValue<int?>();
                    string? stress = itemNode?["stressPattern"]?.GetValue<string>()?.Trim();

                    if (!string.IsNullOrWhiteSpace(ipaUk))
                    {
                        newVocab.Pronunciations.Add(new VocabularyPronunciationEntity
                        {
                            VocabularyId = vocabId,
                            Accent = "UK",
                            Ipa = ipaUk,
                            SyllableCount = syllables,
                            StressPattern = stress
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(ipaUs))
                    {
                        newVocab.Pronunciations.Add(new VocabularyPronunciationEntity
                        {
                            VocabularyId = vocabId,
                            Accent = "US",
                            Ipa = ipaUs,
                            SyllableCount = syllables,
                            StressPattern = stress
                        });
                    }

                    var wfArr = itemNode?["wordFamilies"]?.AsArray();
                    if (wfArr is not null)
                    {
                        foreach (var wf in wfArr)
                        {
                            string rw = wf?["relatedWord"]?.GetValue<string>()?.Trim() ?? "";
                            string rpos = wf?["partOfSpeech"]?.GetValue<string>()?.Trim() ?? "noun";
                            if (!string.IsNullOrWhiteSpace(rw))
                            {
                                newVocab.WordFamilies.Add(new VocabularyWordFamilyEntity
                                {
                                    VocabularyId = vocabId,
                                    RelatedWord = rw,
                                    PartOfSpeech = rpos
                                });
                            }
                        }
                    }

                    var primarySense = BuildSenseFromJson(vocabId, itemNode!, pos, cefr, defEn, meanVi, 1);
                    newVocab.Senses.Add(primarySense);

                    db.Vocabularies.Add(newVocab);
                    await db.SaveChangesAsync(ct);

                    createdSenseIds.Add((vocabId, primarySense.Id));
                    accepted++;
                }
            }

            audit.AcceptedCount = accepted;
            audit.RejectedCount = rejected;
            audit.Status = "Completed";
            db.VocabularyGenerationAudits.Add(audit);
            await db.SaveChangesAsync(ct);

            if (req.AssignToTodayPlan && createdSenseIds.Count > 0)
            {
                string today = ResolveLocalLearningDate("Asia/Ho_Chi_Minh");
                var plan = await db.DailyVocabularyPlans
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.LearningDate == today, ct);

                if (plan is null)
                {
                    plan = new DailyVocabularyPlanEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        LearningDate = today,
                        TimeZoneId = "Asia/Ho_Chi_Minh",
                        NewVocabularyTarget = 20,
                        Status = "InProgress"
                    };
                    db.DailyVocabularyPlans.Add(plan);
                    await db.SaveChangesAsync(ct);
                }

                var existingInPlan = plan.Items.Select(i => i.SenseId).ToHashSet();
                int nextOrder = plan.Items.Count == 0 ? 1 : plan.Items.Max(i => i.DisplayOrder) + 1;

                foreach (var (vocabId, senseId) in createdSenseIds)
                {
                    if (existingInPlan.Add(senseId))
                    {
                        db.DailyVocabularyItems.Add(new DailyVocabularyItemEntity
                        {
                            Id = Guid.NewGuid(),
                            PlanId = plan.Id,
                            VocabularyId = vocabId,
                            SenseId = senseId,
                            ItemType = "New",
                            Status = "Pending",
                            AssignedAt = DateTimeOffset.UtcNow,
                            DisplayOrder = nextOrder++
                        });
                    }
                }
                await db.SaveChangesAsync(ct);
            }

            var resultCards = new List<VocabularyFlashcardDto>();
            foreach (var (vId, sId) in createdSenseIds)
            {
                var card = await GetFlashcardByVocabularyIdAsync(vId, sId, userId, ct);
                if (card is not null) resultCards.Add(card);
            }
            return resultCards;
        }
        catch (Exception ex)
        {
            audit.Status = "Failed";
            audit.ErrorMessage = ex.Message;
            db.VocabularyGenerationAudits.Add(audit);
            await db.SaveChangesAsync(ct);
            throw;
        }
    }

    public async Task<VocabularyFlashcardDto> SubmitVocabularyReviewAsync(
        string userId,
        Guid dailyItemId,
        SubmitVocabularyReviewRequest req,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var now = DateTimeOffset.UtcNow;

        var item = await db.DailyVocabularyItems
            .FirstOrDefaultAsync(i => i.Id == dailyItemId, ct);

        if (item is null)
            throw new InvalidOperationException("Không tìm thấy mục từ vựng trong kế hoạch học tập hôm nay.");

        var progress = await db.UserVocabularyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SenseId == item.SenseId, ct);

        if (progress is null)
        {
            progress = new UserVocabularyProgressEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VocabularyId = item.VocabularyId,
                SenseId = item.SenseId,
                MasteryLevel = 0,
                IntervalDays = 0,
                EaseFactor = 2.50m,
                LearningStatus = "New"
            };
            db.UserVocabularyProgresses.Add(progress);
        }

        string rating = (req.Rating ?? "Good").Trim();
        progress.ReviewCount += 1;
        progress.LastReviewedAt = now;

        // Deterministic SM-2 Spaced Repetition Algorithm (Section 7.4)
        switch (rating.ToLowerInvariant())
        {
            case "again":
                progress.IncorrectCount += 1;
                progress.IntervalDays = 1;
                progress.EaseFactor = Math.Max(1.30m, progress.EaseFactor - 0.20m);
                progress.MasteryLevel = Math.Max(0, progress.MasteryLevel - 15);
                progress.LearningStatus = "Learning";
                progress.NextReviewAt = now.AddHours(4);
                break;

            case "hard":
                progress.CorrectCount += 1;
                progress.IntervalDays = Math.Max(1, (int)Math.Round(Math.Max(1, progress.IntervalDays) * 1.2m));
                progress.EaseFactor = Math.Max(1.30m, progress.EaseFactor - 0.15m);
                progress.MasteryLevel = Math.Min(100, progress.MasteryLevel + 12);
                progress.LearningStatus = "Review";
                progress.NextReviewAt = now.AddDays(progress.IntervalDays);
                break;

            case "easy":
                progress.CorrectCount += 1;
                progress.IntervalDays = progress.IntervalDays <= 1
                    ? 5
                    : (int)Math.Round(progress.IntervalDays * progress.EaseFactor * 1.3m);
                progress.EaseFactor = Math.Min(3.00m, progress.EaseFactor + 0.15m);
                progress.MasteryLevel = Math.Min(100, progress.MasteryLevel + 30);
                progress.LearningStatus = progress.MasteryLevel >= 80 ? "Mastered" : "Review";
                progress.NextReviewAt = now.AddDays(progress.IntervalDays);
                break;

            default: // "Good"
                rating = "Good";
                progress.CorrectCount += 1;
                progress.IntervalDays = progress.IntervalDays <= 1
                    ? 3
                    : (int)Math.Round(progress.IntervalDays * progress.EaseFactor);
                progress.MasteryLevel = Math.Min(100, progress.MasteryLevel + 22);
                progress.LearningStatus = progress.MasteryLevel >= 85 ? "Mastered" : "Review";
                progress.NextReviewAt = now.AddDays(progress.IntervalDays);
                break;
        }

        item.Status = "Completed";
        item.CompletedAt = now;

        db.VocabularyReviewAttempts.Add(new VocabularyReviewAttemptEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VocabularyId = item.VocabularyId,
            SenseId = item.SenseId,
            ReviewType = "FlashcardRecall",
            Rating = rating,
            ReviewedAt = now,
            NextReviewAtSnapshot = progress.NextReviewAt
        });

        await db.SaveChangesAsync(ct);

        // Check if all items in today's plan are completed
        bool anyRemaining = await db.DailyVocabularyItems
            .AnyAsync(i => i.PlanId == item.PlanId && i.Status != "Completed", ct);
        if (!anyRemaining)
        {
            var plan = await db.DailyVocabularyPlans.FirstOrDefaultAsync(p => p.Id == item.PlanId, ct);
            if (plan is not null)
            {
                plan.Status = "Completed";
                plan.CompletedAt = now;
                await db.SaveChangesAsync(ct);
            }
        }

        var updatedCard = await GetFlashcardByVocabularyIdAsync(item.VocabularyId, item.SenseId, userId, ct);
        return updatedCard! with
        {
            DailyItemId = item.Id,
            ItemType = item.ItemType,
            ItemStatus = item.Status
        };
    }

    public async Task<object> GetLearningProgressSummaryAsync(string userId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var now = DateTimeOffset.UtcNow;

        var progresses = await db.UserVocabularyProgresses
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        int totalCatalogSenses = await db.VocabularySenses.CountAsync(ct);
        int totalAssessments = await db.PronunciationAssessments.CountAsync(a => a.UserId == userId, ct);
        decimal? avgPronunciation = await db.PronunciationAssessments
            .Where(a => a.UserId == userId && a.OverallScore != null)
            .AverageAsync(a => a.OverallScore, ct);

        return new
        {
            userId,
            totalCatalogSenses,
            totalStudied = progresses.Count,
            masteredCount = progresses.Count(p => p.LearningStatus == "Mastered"),
            reviewCount = progresses.Count(p => p.LearningStatus == "Review"),
            learningCount = progresses.Count(p => p.LearningStatus == "Learning"),
            dueForReviewNow = progresses.Count(p => p.NextReviewAt != null && p.NextReviewAt <= now),
            averageMastery = progresses.Count == 0 ? 0 : Math.Round(progresses.Average(p => p.MasteryLevel), 1),
            totalPronunciationAssessments = totalAssessments,
            averagePronunciationScore = avgPronunciation.HasValue ? Math.Round(avgPronunciation.Value, 1) : (decimal?)null
        };
    }

    public async Task<object> GetLearningHistoryAsync(string userId, int limit = 30, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var recentReviews = await db.VocabularyReviewAttempts
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReviewedAt)
            .Take(limit)
            .ToListAsync(ct);

        var vocabIds = recentReviews.Select(r => r.VocabularyId).Distinct().ToList();
        var vocabDict = await db.Vocabularies
            .AsNoTracking()
            .Where(v => vocabIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.Word, ct);

        return recentReviews.Select(r => new
        {
            r.Id,
            r.VocabularyId,
            r.SenseId,
            Word = vocabDict.TryGetValue(r.VocabularyId, out var w) ? w : "Vocabulary",
            r.ReviewType,
            r.Rating,
            r.ReviewedAt,
            r.NextReviewAtSnapshot
        });
    }

    private async Task<DailyVocabularyPlanResponseDto> BuildDailyPlanResponseAsync(
        AppDbContext db,
        Guid planId,
        string userId,
        CancellationToken ct)
    {
        var plan = await db.DailyVocabularyPlans
            .AsNoTracking()
            .Include(p => p.Items)
            .FirstAsync(p => p.Id == planId, ct);

        var orderedItems = plan.Items.OrderBy(i => i.DisplayOrder).ToList();
        var vocabIds = orderedItems.Select(i => i.VocabularyId).Distinct().ToList();
        var senseIds = orderedItems.Select(i => i.SenseId).Distinct().ToList();

        var vocabs = await db.Vocabularies
            .AsNoTracking()
            .Include(v => v.Pronunciations)
            .Include(v => v.WordFamilies)
            .Include(v => v.Senses).ThenInclude(s => s.Relations)
            .Include(v => v.Senses).ThenInclude(s => s.Collocations)
            .Include(v => v.Senses).ThenInclude(s => s.Examples)
            .Include(v => v.Senses).ThenInclude(s => s.GrammarPatterns)
            .Where(v => vocabIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, ct);

        var progresses = await db.UserVocabularyProgresses
            .AsNoTracking()
            .Where(p => p.UserId == userId && senseIds.Contains(p.SenseId))
            .ToDictionaryAsync(p => p.SenseId, ct);

        var latestScores = await db.PronunciationAssessments
            .AsNoTracking()
            .Where(a => a.UserId == userId && senseIds.Contains(a.SenseId) && a.OverallScore != null)
            .GroupBy(a => a.SenseId)
            .Select(g => new
            {
                SenseId = g.Key,
                Score = g.OrderByDescending(x => x.CreatedAt).Select(x => x.OverallScore).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.SenseId, x => x.Score, ct);

        var flashcards = new List<VocabularyFlashcardDto>();
        foreach (var item in orderedItems)
        {
            if (!vocabs.TryGetValue(item.VocabularyId, out var vocab)) continue;
            var sense = vocab.Senses.FirstOrDefault(s => s.Id == item.SenseId) ?? vocab.Senses.FirstOrDefault();
            if (sense is null) continue;

            progresses.TryGetValue(sense.Id, out var prog);
            latestScores.TryGetValue(sense.Id, out var latestScore);

            flashcards.Add(MapToFlashcardDto(item.Id, vocab, sense, item.ItemType, item.Status, prog, latestScore));
        }

        return new DailyVocabularyPlanResponseDto(
            PlanId: plan.Id,
            UserId: plan.UserId,
            LearningDate: plan.LearningDate,
            TimeZoneId: plan.TimeZoneId,
            NewVocabularyTarget: plan.NewVocabularyTarget,
            TotalAssignedCount: orderedItems.Count,
            NewCount: orderedItems.Count(i => i.ItemType == "New"),
            PendingReviewCount: orderedItems.Count(i => i.ItemType == "Review"),
            CarryOverCount: orderedItems.Count(i => i.ItemType == "CarryOver"),
            CompletedCount: orderedItems.Count(i => i.Status == "Completed"),
            Status: plan.Status,
            Flashcards: flashcards
        );
    }

    private static VocabularyFlashcardDto MapToFlashcardDto(
        Guid dailyItemId,
        VocabularyEntity vocab,
        VocabularySenseEntity sense,
        string itemType,
        string itemStatus,
        UserVocabularyProgressEntity? prog,
        decimal? latestScore)
    {
        var now = DateTimeOffset.UtcNow;
        bool isDue = prog?.NextReviewAt != null && prog.NextReviewAt <= now;

        return new VocabularyFlashcardDto(
            DailyItemId: dailyItemId,
            VocabularyId: vocab.Id,
            SenseId: sense.Id,
            Word: vocab.Word,
            PartOfSpeech: sense.PartOfSpeech,
            CefrLevel: sense.CefrLevel,
            DefinitionEn: sense.DefinitionEn,
            MeaningVi: sense.MeaningVi,
            DescriptionEn: sense.DescriptionEn,
            Pronunciations: vocab.Pronunciations
                .Select(p => new PronunciationVariantDto(p.Id, p.Accent, p.Ipa, p.AudioUrl, p.AudioSource, p.SyllableCount, p.StressPattern))
                .ToList(),
            Synonyms: sense.Relations
                .Where(r => r.RelationType == "Synonym")
                .OrderBy(r => r.DisplayOrder)
                .Select(r => r.RelatedWord)
                .ToList(),
            Antonyms: sense.Relations
                .Where(r => r.RelationType == "Antonym")
                .OrderBy(r => r.DisplayOrder)
                .Select(r => r.RelatedWord)
                .ToList(),
            WordFamilies: vocab.WordFamilies
                .Select(wf => new WordFamilyItemDto(wf.Id, wf.RelatedWord, wf.PartOfSpeech, wf.RelatedVocabularyId))
                .ToList(),
            Collocations: sense.Collocations
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CollocationItemDto(c.Id, c.PhraseEn, c.MeaningVi, c.DisplayOrder))
                .ToList(),
            Examples: sense.Examples
                .OrderBy(e => e.DisplayOrder)
                .Select(e => new ExampleSentenceDto(e.Id, e.SentenceEn, e.SentenceVi, e.AudioUrl, e.CefrLevel, e.DisplayOrder))
                .ToList(),
            GrammarPatterns: sense.GrammarPatterns
                .Select(g => new GrammarPatternDto(g.Id, g.Pattern, g.Explanation, g.Example))
                .ToList(),
            ItemType: itemType,
            ItemStatus: itemStatus,
            LearningStatus: prog?.LearningStatus ?? "New",
            MasteryLevel: prog?.MasteryLevel ?? 0,
            IsNew: prog is null || prog.ReviewCount == 0,
            IsDueForReview: isDue,
            LatestPronunciationScore: latestScore,
            NextReviewAt: prog?.NextReviewAt,
            IntervalDays: prog?.IntervalDays ?? 0,
            EaseFactor: prog?.EaseFactor ?? 2.50m
        );
    }

    private static VocabularySenseEntity BuildSenseFromJson(
        Guid vocabId,
        JsonNode itemNode,
        string pos,
        string cefr,
        string defEn,
        string meanVi,
        int displayOrder)
    {
        var senseId = Guid.NewGuid();
        var sense = new VocabularySenseEntity
        {
            Id = senseId,
            VocabularyId = vocabId,
            PartOfSpeech = pos,
            CefrLevel = itemNode["cefrLevel"]?.GetValue<string>()?.Trim() ?? cefr,
            DefinitionEn = defEn,
            MeaningVi = meanVi,
            DescriptionEn = itemNode["descriptionEn"]?.GetValue<string>()?.Trim(),
            IsPrimary = displayOrder == 1,
            DisplayOrder = displayOrder
        };

        var syns = itemNode["synonyms"]?.AsArray();
        if (syns is not null)
        {
            int ord = 1;
            foreach (var s in syns)
            {
                string val = s?.GetValue<string>()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(val))
                    sense.Relations.Add(new VocabularyRelationEntity { SenseId = senseId, RelationType = "Synonym", RelatedWord = val, DisplayOrder = ord++ });
            }
        }

        var ants = itemNode["antonyms"]?.AsArray();
        if (ants is not null)
        {
            int ord = 1;
            foreach (var a in ants)
            {
                string val = a?.GetValue<string>()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(val))
                    sense.Relations.Add(new VocabularyRelationEntity { SenseId = senseId, RelationType = "Antonym", RelatedWord = val, DisplayOrder = ord++ });
            }
        }

        var cols = itemNode["collocations"]?.AsArray();
        if (cols is not null)
        {
            int ord = 1;
            foreach (var c in cols)
            {
                string pEn = c?["phraseEn"]?.GetValue<string>()?.Trim() ?? "";
                string? mVi = c?["meaningVi"]?.GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(pEn))
                    sense.Collocations.Add(new VocabularyCollocationEntity { SenseId = senseId, PhraseEn = pEn, MeaningVi = mVi, DisplayOrder = ord++ });
            }
        }

        var exs = itemNode["examples"]?.AsArray();
        if (exs is not null)
        {
            int ord = 1;
            foreach (var ex in exs)
            {
                string sEn = ex?["sentenceEn"]?.GetValue<string>()?.Trim() ?? "";
                string? sVi = ex?["sentenceVi"]?.GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(sEn))
                    sense.Examples.Add(new VocabularyExampleEntity { SenseId = senseId, SentenceEn = sEn, SentenceVi = sVi, CefrLevel = sense.CefrLevel, DisplayOrder = ord++ });
            }
        }

        var gps = itemNode["grammarPatterns"]?.AsArray();
        if (gps is not null)
        {
            foreach (var gp in gps)
            {
                string pat = gp?["pattern"]?.GetValue<string>()?.Trim() ?? "";
                string? exp = gp?["explanation"]?.GetValue<string>()?.Trim();
                string? ex = gp?["example"]?.GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(pat))
                    sense.GrammarPatterns.Add(new VocabularyGrammarPatternEntity { SenseId = senseId, Pattern = pat, Explanation = exp, Example = ex });
            }
        }

        return sense;
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
