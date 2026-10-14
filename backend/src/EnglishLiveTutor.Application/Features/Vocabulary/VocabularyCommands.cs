using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Learning;
using EnglishLiveTutor.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Vocabulary;

public sealed record GenerateVocabularyCommand(
    string UserId,
    GenerateVocabularyRequest Request
) : ICommand<List<VocabularyFlashcardDto>>;

public sealed class GenerateVocabularyCommandHandler : ICommandHandler<GenerateVocabularyCommand, List<VocabularyFlashcardDto>>
{
    private readonly IAppDbContextFactory _dbFactory;
    private readonly IVocabularyAiGenerator _aiGenerator;

    public const string PromptVersion = "PROMPT_VOCAB_GEN_V1";

    public GenerateVocabularyCommandHandler(
        IAppDbContextFactory dbFactory,
        IVocabularyAiGenerator aiGenerator)
    {
        _dbFactory = dbFactory;
        _aiGenerator = aiGenerator;
    }

    public async Task<List<VocabularyFlashcardDto>> HandleAsync(GenerateVocabularyCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        var userId = command.UserId;

        if (string.IsNullOrWhiteSpace(req.ApiKey))
            throw new InvalidOperationException("Cần cung cấp API Key của Google Gemini để sinh từ vựng AI.");

        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        string requestedModelCode = string.IsNullOrWhiteSpace(req.ModelCode) ? "gemini-3.8-flash" : req.ModelCode.Trim();

        var modelMeta = await db.AiModelCatalog
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ModelCode == requestedModelCode, ct);

        if (modelMeta is null)
            throw new InvalidOperationException($"Model '{requestedModelCode}' không tồn tại trong AI Model Catalog.");

        if (!modelMeta.IsEnabled || !modelMeta.IsAvailable || !modelMeta.SupportsTextGeneration || modelMeta.SupportsLiveAudio)
            throw new InvalidOperationException($"Model '{modelMeta.DisplayName}' không hỗ trợ tạo từ vựng cấu trúc JSON.");

        string effectiveModelCode = modelMeta.GetEffectiveModelCode();

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
            var generatedSenses = await _aiGenerator.GenerateVocabularyAsync(
                req.ApiKey.Trim(),
                effectiveModelCode,
                cefr,
                topic,
                requestedCount,
                excludedList,
                ct);

            int accepted = 0;
            int rejected = 0;
            int senseOrderCounter = 1;

            foreach (var rawSense in generatedSenses)
            {
                string wordRaw = rawSense.Word;
                string normWord = VocabularyEntity.NormalizeWord(wordRaw);
                if (string.IsNullOrWhiteSpace(normWord) || normWord.Length < 2)
                {
                    rejected++;
                    continue;
                }

                var existingVocab = await db.Vocabularies
                    .Include(v => v.Senses)
                    .FirstOrDefaultAsync(v => v.NormalizedWord == normWord, ct);

                VocabularyEntity vocab;
                if (existingVocab is null)
                {
                    vocab = new VocabularyEntity
                    {
                        Id = Guid.NewGuid(),
                        Word = wordRaw.Trim(),
                        NormalizedWord = normWord,
                        LanguageCode = "en",
                        SourceType = "AI",
                        SourceModelId = requestedModelCode,
                        TopicCategory = topic,
                        CreatedAt = DateTimeOffset.UtcNow,
                        IsActive = true
                    };

                    if (!string.IsNullOrWhiteSpace(rawSense.IpaUs))
                    {
                        vocab.Pronunciations.Add(new VocabularyPronunciationEntity
                        {
                            Id = Guid.NewGuid(),
                            VocabularyId = vocab.Id,
                            Accent = "US",
                            Ipa = rawSense.IpaUs.Trim(),
                            AudioSource = "BrowserTTS"
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(rawSense.IpaUk))
                    {
                        vocab.Pronunciations.Add(new VocabularyPronunciationEntity
                        {
                            Id = Guid.NewGuid(),
                            VocabularyId = vocab.Id,
                            Accent = "UK",
                            Ipa = rawSense.IpaUk.Trim(),
                            AudioSource = "BrowserTTS"
                        });
                    }

                    foreach (var wf in rawSense.WordFamilies)
                    {
                        if (!string.IsNullOrWhiteSpace(wf.RelatedWord))
                        {
                            vocab.WordFamilies.Add(new VocabularyWordFamilyEntity
                            {
                                Id = Guid.NewGuid(),
                                VocabularyId = vocab.Id,
                                RelatedWord = wf.RelatedWord.Trim(),
                                PartOfSpeech = string.IsNullOrWhiteSpace(wf.PartOfSpeech) ? "other" : wf.PartOfSpeech.Trim()
                            });
                        }
                    }

                    db.Vocabularies.Add(vocab);
                }
                else
                {
                    vocab = existingVocab;
                }

                var senseId = Guid.NewGuid();
                var sense = new VocabularySenseEntity
                {
                    Id = senseId,
                    VocabularyId = vocab.Id,
                    PartOfSpeech = string.IsNullOrWhiteSpace(rawSense.PartOfSpeech) ? "noun" : rawSense.PartOfSpeech.Trim().ToLowerInvariant(),
                    CefrLevel = string.IsNullOrWhiteSpace(rawSense.CefrLevel) ? cefr : rawSense.CefrLevel.Trim(),
                    DefinitionEn = rawSense.DefinitionEn,
                    MeaningVi = rawSense.MeaningVi,
                    DescriptionEn = rawSense.DescriptionEn,
                    IsPrimary = vocab.Senses.Count == 0,
                    DisplayOrder = senseOrderCounter++
                };

                foreach (var syn in rawSense.Synonyms)
                {
                    sense.Relations.Add(new VocabularyRelationEntity
                    {
                        Id = Guid.NewGuid(),
                        SenseId = senseId,
                        RelationType = "Synonym",
                        RelatedWord = syn
                    });
                }
                foreach (var ant in rawSense.Antonyms)
                {
                    sense.Relations.Add(new VocabularyRelationEntity
                    {
                        Id = Guid.NewGuid(),
                        SenseId = senseId,
                        RelationType = "Antonym",
                        RelatedWord = ant
                    });
                }

                int colOrder = 1;
                foreach (var col in rawSense.Collocations)
                {
                    sense.Collocations.Add(new VocabularyCollocationEntity
                    {
                        Id = Guid.NewGuid(),
                        SenseId = senseId,
                        PhraseEn = col.PhraseEn,
                        MeaningVi = col.MeaningVi,
                        DisplayOrder = colOrder++
                    });
                }

                int exOrder = 1;
                foreach (var ex in rawSense.Examples)
                {
                    sense.Examples.Add(new VocabularyExampleEntity
                    {
                        Id = Guid.NewGuid(),
                        SenseId = senseId,
                        SentenceEn = ex.SentenceEn,
                        SentenceVi = ex.SentenceVi,
                        CefrLevel = cefr,
                        DisplayOrder = exOrder++
                    });
                }

                foreach (var gp in rawSense.GrammarPatterns)
                {
                    sense.GrammarPatterns.Add(new VocabularyGrammarPatternEntity
                    {
                        Id = Guid.NewGuid(),
                        SenseId = senseId,
                        Pattern = gp.Pattern,
                        Explanation = gp.Explanation,
                        Example = gp.Example
                    });
                }

                db.VocabularySenses.Add(sense);
                createdSenseIds.Add((vocab.Id, sense.Id));
                accepted++;
            }

            audit.AcceptedCount = accepted;
            audit.RejectedCount = rejected;
            audit.Status = "Completed";
            db.VocabularyGenerationAudits.Add(audit);

            await db.SaveChangesAsync(ct);

            // Optional: assign to today's plan immediately
            if (req.AssignToTodayPlan && createdSenseIds.Count > 0)
            {
                string today = DailyVocabularyPlanEntity.ResolveLocalLearningDate("Asia/Ho_Chi_Minh");
                var todayPlan = await db.DailyVocabularyPlans
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.LearningDate == today, ct);

                if (todayPlan is not null)
                {
                    int nextOrder = todayPlan.Items.Count == 0 ? 1 : todayPlan.Items.Max(i => i.DisplayOrder) + 1;
                    foreach (var (vId, sId) in createdSenseIds)
                    {
                        if (!todayPlan.Items.Any(i => i.SenseId == sId))
                        {
                            db.DailyVocabularyItems.Add(new DailyVocabularyItemEntity
                            {
                                Id = Guid.NewGuid(),
                                PlanId = todayPlan.Id,
                                VocabularyId = vId,
                                SenseId = sId,
                                ItemType = "New",
                                Status = "Pending",
                                AssignedAt = DateTimeOffset.UtcNow,
                                DisplayOrder = nextOrder++
                            });
                        }
                    }
                    await db.SaveChangesAsync(ct);
                }
            }

            var resultDtos = new List<VocabularyFlashcardDto>();
            foreach (var (vId, sId) in createdSenseIds)
            {
                var card = await VocabularyProjectionHelper.GetFlashcardByVocabularyIdAsync(db, vId, sId, userId, ct);
                if (card is not null) resultDtos.Add(card);
            }

            return resultDtos;
        }
        catch (Exception ex)
        {
            audit.Status = "Failed";
            audit.ErrorMessage = ex.Message;
            db.VocabularyGenerationAudits.Add(audit);
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }
}
