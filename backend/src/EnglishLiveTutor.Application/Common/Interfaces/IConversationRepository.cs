using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Conversations;

namespace EnglishLiveTutor.Application.Common.Interfaces;

public interface IConversationRepository
{
    Task<List<object>> ListConversationsAsync();
    Task<ConversationEntity?> GetConversationWithHistoryAsync(string id);

    Task<ConversationEntity> CreateOrUpdateConversationAsync(
        string? id,
        string? title,
        string cefrLevel,
        bool extendedThinking,
        string voiceName,
        double speechSpeed,
        string conversationMode = "daily_conversation",
        string topic = "",
        string? customSystemPrompt = null);

    Task<bool> DeleteConversationAsync(string id);

    Task<int> GetMaxSequenceOrderAsync(string conversationId);

    Task UpsertMessageTranscriptAsync(
        string conversationId,
        string turnId,
        string speaker,
        int sequenceOrder,
        string fullTranscript,
        bool isFinal);

    Task UpsertMessageTranslationAsync(
        string conversationId,
        string turnId,
        string speaker,
        int sequenceOrder,
        string fullVietnameseText);

    Task SaveCorrectionAsync(LanguageCorrectionDto correction);
}
