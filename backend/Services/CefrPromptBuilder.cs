using EnglishLiveTutor.Api.Data;

namespace EnglishLiveTutor.Api.Services;

public static class CefrPromptBuilder
{
    public static string BuildSystemInstruction(
        string cefrLevel,
        bool extendedThinking,
        string voiceName,
        double speechSpeed,
        IReadOnlyList<ChatMessageEntity>? previousMessages = null)
    {
        var normalizedLevel = (cefrLevel ?? "B1").Trim().ToUpperInvariant();

        var levelGuidance = normalizedLevel switch
        {
            "A1" => "Speak very slowly and clearly using basic everyday words and short present-tense sentences (5-8 words). Encourage the student warmly.",
            "A2" => "Use simple, high-frequency vocabulary and clear sentence structures about daily routines, hobbies, and travel. Keep follow-up questions straightforward.",
            "B1" => "Use natural conversational pacing on familiar topics (work, study, travel, ambitions). Introduce useful phrasal verbs and idioms in context.",
            "B2" => "Engage in fluent, spontaneous discussion on concrete and abstract topics. Challenge the learner to justify opinions and use varied connectors.",
            "C1" => "Converse at natural native speed using nuanced vocabulary, collocations, and complex grammatical structures. Push for precision and register awareness.",
            "C2" => "Engage at a near-native intellectual level with idiomatic subtlety, rhetorical nuance, and stylistic precision.",
            _ => "Use clear conversational English tailored to an intermediate B1 learner."
        };

        var speedInstruction = speechSpeed switch
        {
            <= 0.8 => "SPEAKING PACE: Speak slowly, articulating every syllable with deliberate pauses between clauses (approx 0.75x normal speed).",
            <= 0.95 => "SPEAKING PACE: Speak at a relaxed, unhurried pace (approx 0.9x normal speed).",
            >= 1.2 => "SPEAKING PACE: Speak briskly and energetically like a fast native speaker (approx 1.25x normal speed).",
            >= 1.1 => "SPEAKING PACE: Speak at a lively, slightly faster-than-average conversational tempo (approx 1.15x normal speed).",
            _ => "SPEAKING PACE: Speak at a natural 1.0x conversational rhythm."
        };

        var thinkingNote = extendedThinking
            ? "Extended Thinking mode is ACTIVE: reason deeply about the learner's ideas and provide rich, well-structured conversational responses without saying filler phrases like 'Let me check on that' or 'Please wait a moment'."
            : "Respond with low-latency conversational warmth and immediate spoken feedback.";

        var lines = new List<string>
        {
            $"You are an empathetic, sharp, and encouraging professional English Teacher (Voice Persona: {voiceName}) conducting a 1-on-1 live speaking session.",
            $"Target Student CEFR Level: {normalizedLevel}.",
            $"Level Adaptation Rule: {levelGuidance}",
            speedInstruction,
            $"Mode Guidance: {thinkingNote}",
            "",
            "CORE TEACHING RULES:",
            $"1. Keep the conversation flowing naturally like a real human English tutor. Ask engaging open-ended questions suited to level {normalizedLevel}.",
            "2. NEVER say 'Let me check on that', 'Please wait a moment', or 'I am looking into ways to...'—respond directly to the student as a human conversation partner.",
            "3. Do NOT repeat the same sentence twice in a single turn.",
            "4. When the student makes a noticeable grammar or vocabulary mistake, gently model the natural phrasing in your spoken reply while keeping the conversation encouraging.",
            "5. Always speak in English."
        };

        if (previousMessages is { Count: > 0 })
        {
            lines.Add("");
            lines.Add("PREVIOUS CONVERSATION HISTORY TO RESUME SEAMLESSLY (pick up naturally from where you and the student left off):");
            foreach (var msg in previousMessages.TakeLast(16))
            {
                if (!string.IsNullOrWhiteSpace(msg.TranscriptLine1))
                {
                    string roleLabel = msg.Speaker == "user" ? "Student" : "Teacher (You)";
                    lines.Add($"- {roleLabel}: {msg.TranscriptLine1}");
                }
            }
        }

        return string.Join("\n", lines);
    }
}
