using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Domain.Conversations;
using EnglishLiveTutor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Infrastructure.Ai;

/// <summary>Composes the Gemini system instruction exclusively from editable database templates.</summary>
public sealed class CefrPromptBuilder(IDbContextFactory<AppDbContext> factory) : ICefrPromptBuilder
{
    public async Task<string> BuildSystemInstructionAsync(string cefrLevel, bool extendedThinking,
        string voiceName, double speechSpeed, string conversationMode = "daily_conversation",
        string topic = "", string? customSystemPrompt = null,
        IReadOnlyList<ChatMessageEntity>? previousMessages = null, CancellationToken ct = default)
    {
        var level = (cefrLevel ?? "B1").Trim().ToUpperInvariant();
        var mode = (conversationMode ?? "daily_conversation").Trim().ToLowerInvariant();
        var speedKey = speechSpeed <= 0.8 ? "slow" : speechSpeed <= 0.95 ? "relaxed"
            : speechSpeed >= 1.2 ? "fast" : speechSpeed >= 1.1 ? "lively" : "normal";
        await using var db = await factory.CreateDbContextAsync(ct);
        var templates = await db.AiPromptTemplates.AsNoTracking().Where(x => x.IsEnabled)
            .ToDictionaryAsync(x => x.Code, x => x.Content, ct);
        string Required(string key) => templates.TryGetValue(key, out var text) ? text
            : throw new InvalidOperationException($"Missing enabled prompt template: {key}. Run database seeder.");
        string Mode(string key) => templates.TryGetValue(key, out var text) ? text : Required("mode.default");
        if (!templates.ContainsKey("cefr." + level)) level = "B1";
        var lines = new List<string>
        {
            Required("common.role").Replace("{{VOICE_NAME}}", voiceName),
            $"Target Learner CEFR Level: {level}.",
            "Level Adaptation Rule: " + Required("cefr." + level),
            Required("speed." + speedKey),
            "Mode Guidance: " + Required(extendedThinking ? "thinking.extended" : "thinking.standard"),
            "", "=== SELECTED SESSION MODE ===",
            Mode("mode." + mode).Replace("{{CEFR_LEVEL}}", level)
        };
        if (!string.IsNullOrWhiteSpace(topic))
        {
            lines.Add(""); lines.Add("=== CONVERSATION TOPIC / SCENARIO ===");
            lines.Add($"Active Topic: \"{topic.Trim()}\"");
            lines.Add("Anchor your questions, examples, vocabulary, and follow-ups directly around this topic.");
        }
        if (!string.IsNullOrWhiteSpace(customSystemPrompt))
        {
            lines.Add(""); lines.Add("=== USER-DEFINED CONVERSATION INSTRUCTIONS ===");
            lines.Add(customSystemPrompt.Trim());
        }
        lines.Add(""); lines.Add("=== CORE CONVERSATION RULES ===");
        lines.Add(Required("common.rules"));
        if (previousMessages is { Count: > 0 })
        {
            lines.Add(""); lines.Add("=== PREVIOUS CONVERSATION HISTORY TO RESUME SEAMLESSLY ===");
            foreach (var msg in previousMessages.TakeLast(16))
            {
                if (!string.IsNullOrWhiteSpace(msg.TranscriptLine1))
                    lines.Add($"- {(msg.Speaker == "user" ? "User" : "AI (You)")}: {msg.TranscriptLine1}");
            }
        }
        return string.Join("\n", lines);
    }
}
