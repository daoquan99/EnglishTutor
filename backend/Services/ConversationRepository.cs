using EnglishLiveTutor.Api.Data;
using EnglishLiveTutor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Api.Services;

public sealed class ConversationRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public ConversationRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<object>> ListConversationsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var items = await db.Conversations
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.CefrLevel,
                c.ExtendedThinking,
                c.VoiceName,
                c.SpeechSpeed,
                c.CreatedAt,
                c.UpdatedAt,
                MessageCount = c.Messages.Count(m => m.TranscriptLine1 != "" || m.TranslationLine2 != ""),
                LastMessagePreview = c.Messages
                    .Where(m => m.TranscriptLine1 != "")
                    .OrderByDescending(m => m.SequenceOrder)
                    .Select(m => m.TranscriptLine1)
                    .FirstOrDefault() ?? "Chưa có tin nhắn"
            })
            .ToListAsync();

        return items.Cast<object>().ToList();
    }

    public async Task<ConversationEntity?> GetConversationWithHistoryAsync(string conversationId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var conv = await db.Conversations
            .AsNoTracking()
            .Include(c => c.Messages
                .Where(m => m.TranscriptLine1 != "" || m.TranslationLine2 != "")
                .OrderBy(m => m.SequenceOrder))
            .Include(c => c.Corrections.OrderByDescending(x => x.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        return conv;
    }

    public async Task<int> GetMaxSequenceOrderAsync(string conversationId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Messages
            .Where(m => m.ConversationId == conversationId)
            .Select(m => (int?)m.SequenceOrder)
            .MaxAsync() ?? 0;
    }

    public async Task<ConversationEntity> CreateOrUpdateConversationAsync(
        string? id,
        string? title,
        string cefrLevel,
        bool extendedThinking,
        string voiceName,
        double speechSpeed)
    {
        await _writeLock.WaitAsync();
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            ConversationEntity? conv = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == id);
            }

            if (conv is null)
            {
                conv = new ConversationEntity
                {
                    Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N")[..12] : id,
                    Title = string.IsNullOrWhiteSpace(title) ? $"Hội thoại {cefrLevel} ({DateTime.Now:dd/MM HH:mm})" : title,
                    CefrLevel = cefrLevel,
                    ExtendedThinking = extendedThinking,
                    VoiceName = voiceName,
                    SpeechSpeed = speechSpeed,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                db.Conversations.Add(conv);
            }
            else
            {
                conv.CefrLevel = cefrLevel;
                conv.ExtendedThinking = extendedThinking;
                conv.VoiceName = voiceName;
                conv.SpeechSpeed = speechSpeed;
                conv.UpdatedAt = DateTimeOffset.UtcNow;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    conv.Title = title;
                }
            }

            await db.SaveChangesAsync();
            return conv;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task UpsertMessageTranscriptAsync(
        string conversationId,
        string turnId,
        string speaker,
        int sequenceOrder,
        string fullTranscript,
        bool isFinal)
    {
        if (string.IsNullOrWhiteSpace(fullTranscript))
            return;

        await _writeLock.WaitAsync();
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var msg = await db.Messages.FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.TurnId == turnId && m.Speaker == speaker);

            if (msg is null)
            {
                msg = new ChatMessageEntity
                {
                    Id = $"{conversationId}_{turnId}_{speaker}",
                    ConversationId = conversationId,
                    TurnId = turnId,
                    Speaker = speaker,
                    TranscriptLine1 = fullTranscript,
                    IsFinal = isFinal,
                    SequenceOrder = sequenceOrder,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.Messages.Add(msg);
            }
            else
            {
                msg.TranscriptLine1 = fullTranscript;
                msg.IsFinal = isFinal || msg.IsFinal;
                if (sequenceOrder > 0 && (msg.SequenceOrder == 0 || sequenceOrder < msg.SequenceOrder))
                {
                    msg.SequenceOrder = sequenceOrder;
                }
            }

            var conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conv is not null)
            {
                conv.UpdatedAt = DateTimeOffset.UtcNow;
                if (speaker == "user" && conv.Title.StartsWith("Hội thoại ", StringComparison.OrdinalIgnoreCase) && fullTranscript.Length > 6)
                {
                    conv.Title = fullTranscript.Length > 42 ? fullTranscript[..42] + "..." : fullTranscript;
                }
            }

            await db.SaveChangesAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task UpsertMessageTranslationAsync(
        string conversationId,
        string turnId,
        string speaker,
        int sequenceOrder,
        string fullVietnameseText)
    {
        if (string.IsNullOrWhiteSpace(fullVietnameseText))
            return;

        await _writeLock.WaitAsync();
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var msg = await db.Messages.FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.TurnId == turnId && m.Speaker == speaker);

            if (msg is null)
            {
                msg = new ChatMessageEntity
                {
                    Id = $"{conversationId}_{turnId}_{speaker}",
                    ConversationId = conversationId,
                    TurnId = turnId,
                    Speaker = speaker,
                    TranslationLine2 = fullVietnameseText,
                    SequenceOrder = sequenceOrder,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.Messages.Add(msg);
            }
            else
            {
                msg.TranslationLine2 = fullVietnameseText;
                if (sequenceOrder > 0 && msg.SequenceOrder == 0)
                {
                    msg.SequenceOrder = sequenceOrder;
                }
            }

            var conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conv is not null)
            {
                conv.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task SaveCorrectionAsync(LanguageCorrectionDto dto)
    {
        await _writeLock.WaitAsync();
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            db.Corrections.Add(new LanguageCorrectionEntity
            {
                Id = dto.Id,
                ConversationId = dto.ConversationId,
                TurnId = dto.TurnId,
                OriginalText = dto.OriginalText,
                CorrectedText = dto.CorrectedText,
                ErrorCategory = dto.ErrorCategory,
                ExplanationVi = dto.ExplanationVi,
                CefrTip = dto.CefrTip,
                CreatedAt = dto.Timestamp
            });
            await db.SaveChangesAsync();
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<bool> DeleteConversationAsync(string conversationId)
    {
        await _writeLock.WaitAsync();
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var conv = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);
            if (conv is null) return false;
            db.Conversations.Remove(conv);
            await db.SaveChangesAsync();
            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
