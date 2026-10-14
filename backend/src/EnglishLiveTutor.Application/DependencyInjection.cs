using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Application.Features.Conversations;
using EnglishLiveTutor.Application.Features.Learning;
using EnglishLiveTutor.Application.Features.Pronunciation;
using EnglishLiveTutor.Application.Features.Vocabulary;
using EnglishLiveTutor.Domain.Conversations;
using EnglishLiveTutor.Domain.Pronunciation;
using EnglishLiveTutor.Domain.Vocabulary;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishLiveTutor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Conversations
        services.AddTransient<CreateConversationCommandHandler>();
        services.AddTransient<ICommandHandler<CreateConversationCommand, ConversationEntity>>(sp => sp.GetRequiredService<CreateConversationCommandHandler>());

        services.AddTransient<UpdateConversationSettingsCommandHandler>();
        services.AddTransient<ICommandHandler<UpdateConversationSettingsCommand, ConversationEntity>>(sp => sp.GetRequiredService<UpdateConversationSettingsCommandHandler>());

        services.AddTransient<DeleteConversationCommandHandler>();
        services.AddTransient<ICommandHandler<DeleteConversationCommand, bool>>(sp => sp.GetRequiredService<DeleteConversationCommandHandler>());

        services.AddTransient<ListConversationsQueryHandler>();
        services.AddTransient<IQueryHandler<ListConversationsQuery, List<object>>>(sp => sp.GetRequiredService<ListConversationsQueryHandler>());

        services.AddTransient<GetConversationHistoryQueryHandler>();
        services.AddTransient<IQueryHandler<GetConversationHistoryQuery, object?>>(sp => sp.GetRequiredService<GetConversationHistoryQueryHandler>());

        // Vocabulary
        services.AddTransient<GenerateVocabularyCommandHandler>();
        services.AddTransient<ICommandHandler<GenerateVocabularyCommand, List<VocabularyFlashcardDto>>>(sp => sp.GetRequiredService<GenerateVocabularyCommandHandler>());

        services.AddTransient<GetVocabularyFlashcardQueryHandler>();
        services.AddTransient<IQueryHandler<GetVocabularyFlashcardQuery, VocabularyFlashcardDto?>>(sp => sp.GetRequiredService<GetVocabularyFlashcardQueryHandler>());

        services.AddTransient<GetVocabularyGenerationModelsQueryHandler>();
        services.AddTransient<IQueryHandler<GetVocabularyGenerationModelsQuery, List<AiModelConfigEntity>>>(sp => sp.GetRequiredService<GetVocabularyGenerationModelsQueryHandler>());

        // Learning
        services.AddTransient<InitializeDailyVocabularyPlanCommandHandler>();
        services.AddTransient<ICommandHandler<InitializeDailyVocabularyPlanCommand, DailyVocabularyPlanResponseDto>>(sp => sp.GetRequiredService<InitializeDailyVocabularyPlanCommandHandler>());

        services.AddTransient<SubmitVocabularyReviewCommandHandler>();
        services.AddTransient<ICommandHandler<SubmitVocabularyReviewCommand, VocabularyFlashcardDto>>(sp => sp.GetRequiredService<SubmitVocabularyReviewCommandHandler>());

        services.AddTransient<GetDailyVocabularyPlanQueryHandler>();
        services.AddTransient<IQueryHandler<GetDailyVocabularyPlanQuery, DailyVocabularyPlanResponseDto>>(sp => sp.GetRequiredService<GetDailyVocabularyPlanQueryHandler>());

        services.AddTransient<GetLearningProgressQueryHandler>();
        services.AddTransient<IQueryHandler<GetLearningProgressQuery, object>>(sp => sp.GetRequiredService<GetLearningProgressQueryHandler>());

        services.AddTransient<GetLearningHistoryQueryHandler>();
        services.AddTransient<IQueryHandler<GetLearningHistoryQuery, object>>(sp => sp.GetRequiredService<GetLearningHistoryQueryHandler>());

        // Pronunciation
        services.AddTransient<AssessPronunciationCommandHandler>();
        services.AddTransient<ICommandHandler<AssessPronunciationCommand, PronunciationAssessmentResultDto>>(sp => sp.GetRequiredService<AssessPronunciationCommandHandler>());

        services.AddTransient<UpdateScoringRubricCommandHandler>();
        services.AddTransient<ICommandHandler<UpdateScoringRubricCommand, ScoringRubricEntity>>(sp => sp.GetRequiredService<UpdateScoringRubricCommandHandler>());

        services.AddTransient<GetPronunciationModelsQueryHandler>();
        services.AddTransient<IQueryHandler<GetPronunciationModelsQuery, List<AiModelConfigEntity>>>(sp => sp.GetRequiredService<GetPronunciationModelsQueryHandler>());

        services.AddTransient<GetActiveScoringRubricQueryHandler>();
        services.AddTransient<IQueryHandler<GetActiveScoringRubricQuery, ScoringRubricEntity?>>(sp => sp.GetRequiredService<GetActiveScoringRubricQueryHandler>());

        services.AddTransient<GetPronunciationHistoryQueryHandler>();
        services.AddTransient<IQueryHandler<GetPronunciationHistoryQuery, List<PronunciationAssessmentResultDto>>>(sp => sp.GetRequiredService<GetPronunciationHistoryQueryHandler>());

        return services;
    }
}
