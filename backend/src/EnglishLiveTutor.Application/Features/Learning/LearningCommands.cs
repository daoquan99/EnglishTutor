using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Application.Features.Vocabulary;
using EnglishLiveTutor.Domain.Learning;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Learning;

public sealed record InitializeDailyVocabularyPlanCommand(
    string UserId,
    InitializeDailyPlanRequest Request
) : ICommand<DailyVocabularyPlanResponseDto>;

public sealed class InitializeDailyVocabularyPlanCommandHandler : ICommandHandler<InitializeDailyVocabularyPlanCommand, DailyVocabularyPlanResponseDto>
{
    private readonly IAppDbContextFactory _dbFactory;
    private readonly ICommandHandler<GenerateVocabularyCommand, List<VocabularyFlashcardDto>> _generateHandler;

    public InitializeDailyVocabularyPlanCommandHandler(
        IAppDbContextFactory dbFactory,
        ICommandHandler<GenerateVocabularyCommand, List<VocabularyFlashcardDto>> generateHandler)
    {
        _dbFactory = dbFactory;
        _generateHandler = generateHandler;
    }

    public async Task<DailyVocabularyPlanResponseDto> HandleAsync(InitializeDailyVocabularyPlanCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        var userId = command.UserId;

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        string learningDate = DailyVocabularyPlanEntity.ResolveLocalLearningDate(req.TimeZoneId);
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
            await _generateHandler.HandleAsync(
                new GenerateVocabularyCommand(
                    userId,
                    new GenerateVocabularyRequest(
                        ApiKey: req.ApiKey!,
                        ModelCode: req.GenerationModelCode ?? "gemini-3.8-flash",
                        RequestedCount: batchToGenerate,
                        CefrLevel: req.CefrLevel,
                        Topic: req.Topic,
                        ExcludedWords: null,
                        AssignToTodayPlan: true
                    )
                ),
                ct);
        }

        return await VocabularyProjectionHelper.BuildDailyPlanResponseAsync(db, plan.Id, userId, ct);
    }
}

public sealed record SubmitVocabularyReviewCommand(
    string UserId,
    Guid DailyItemId,
    SubmitVocabularyReviewRequest Request
) : ICommand<VocabularyFlashcardDto>;

public sealed class SubmitVocabularyReviewCommandHandler : ICommandHandler<SubmitVocabularyReviewCommand, VocabularyFlashcardDto>
{
    private readonly IAppDbContextFactory _dbFactory;

    public SubmitVocabularyReviewCommandHandler(IAppDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<VocabularyFlashcardDto> HandleAsync(SubmitVocabularyReviewCommand command, CancellationToken ct = default)
    {
        var userId = command.UserId;
        var dailyItemId = command.DailyItemId;
        var req = command.Request;

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

        string rating = progress.ApplyReview(req.Rating, now);
        item.Complete(now);

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
                plan.Complete(now);
                await db.SaveChangesAsync(ct);
            }
        }

        var updatedCard = await VocabularyProjectionHelper.GetFlashcardByVocabularyIdAsync(db, item.VocabularyId, item.SenseId, userId, ct);
        return updatedCard! with
        {
            DailyItemId = item.Id,
            ItemType = item.ItemType,
            ItemStatus = item.Status
        };
    }
}
