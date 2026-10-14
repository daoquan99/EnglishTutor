using EnglishLiveTutor.Domain.Conversations;

namespace EnglishLiveTutor.Infrastructure.Ai;

public static class CefrPromptBuilder
{
    public static string BuildSystemInstruction(
        string cefrLevel,
        bool extendedThinking,
        string voiceName,
        double speechSpeed,
        string conversationMode = "daily_conversation",
        string topic = "",
        string? customSystemPrompt = null,
        IReadOnlyList<ChatMessageEntity>? previousMessages = null)
    {
        var normalizedLevel = (cefrLevel ?? "B1").Trim().ToUpperInvariant();
        var normalizedMode = (conversationMode ?? "daily_conversation").Trim().ToLowerInvariant();

        var levelGuidance = normalizedLevel switch
        {
            "A1" => "Speak very slowly and clearly using basic everyday words and short present-tense sentences (5-8 words). Encourage the student warmly.",
            "A2" => "Use simple, high-frequency vocabulary and clear sentence structures about daily routines, hobbies, and work basics. Keep follow-up questions straightforward.",
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

        var modeDirective = GetModeDirective(normalizedMode, normalizedLevel);

        var thinkingNote = extendedThinking
            ? "Extended Thinking mode is ACTIVE: reason deeply about the learner's ideas and provide rich, well-structured conversational responses without saying filler phrases like 'Let me check on that' or 'Please wait a moment'."
            : "Respond with low-latency conversational warmth and immediate spoken feedback.";

        var lines = new List<string>
        {
            $"You are an expert English-speaking AI Partner & Coach (Voice Persona: {voiceName}) conducting a 1-on-1 live voice session.",
            $"Target Learner CEFR Level: {normalizedLevel}.",
            $"Level Adaptation Rule: {levelGuidance}",
            speedInstruction,
            $"Mode Guidance: {thinkingNote}",
            "",
            "=== SELECTED SESSION MODE ===",
            modeDirective
        };

        if (!string.IsNullOrWhiteSpace(topic))
        {
            lines.Add("");
            lines.Add("=== CONVERSATION TOPIC / SCENARIO ===");
            lines.Add($"Active Topic: \"{topic.Trim()}\"");
            lines.Add("Anchor your questions, examples, vocabulary, and follow-ups directly around this topic.");
        }

        if (!string.IsNullOrWhiteSpace(customSystemPrompt))
        {
            lines.Add("");
            lines.Add("=== USER-DEFINED CUSTOM SYSTEM PROMPT (HIGHEST PRIORITY FOR THIS CONVERSATION) ===");
            lines.Add(customSystemPrompt.Trim());
        }

        lines.Add("");
        lines.Add("=== CORE CONVERSATION RULES ===");
        lines.Add("1. Keep the spoken interaction natural, engaging, and human-like.");
        lines.Add("2. NEVER say robotic filler phrases like 'Let me check on that', 'Please wait a moment', or 'I am looking into ways to...'.");
        lines.Add("3. Do NOT repeat the same sentence twice in a single turn.");
        lines.Add("4. Always speak in English.");

        if (previousMessages is { Count: > 0 })
        {
            lines.Add("");
            lines.Add("=== PREVIOUS CONVERSATION HISTORY TO RESUME SEAMLESSLY ===");
            foreach (var msg in previousMessages.TakeLast(16))
            {
                if (!string.IsNullOrWhiteSpace(msg.TranscriptLine1))
                {
                    string roleLabel = msg.Speaker == "user" ? "User" : "AI (You)";
                    lines.Add($"- {roleLabel}: {msg.TranscriptLine1}");
                }
            }
        }

        return string.Join("\n", lines);
    }

    private static string GetModeDirective(string mode, string cefrLevel) => mode switch
    {
        // --- 12 INTERVIEW & COACHING MODES ---
        "practice_interview" =>
            "MODE: Practice Interview Mode (Phỏng vấn & Sửa lỗi sau mỗi câu trả lời).\n" +
            "- Act as a supportive interviewer and English coach.\n" +
            "- After each answer from the user, briefly point out 1 key grammar/vocabulary upgrade to make their interview answer sound more professional, then ask the next interview question.",

        "real_interview" =>
            "MODE: Real Interview Mode (Phỏng vấn thực tế, không ngắt lời, đánh giá cuối buổi).\n" +
            "- Act strictly as a real-world hiring interviewer. Do NOT correct grammar mid-conversation.\n" +
            "- Maintain professional interview flow with realistic follow-up questions. Only provide a comprehensive evaluation of their English and interview performance when the candidate asks to conclude or review the interview.",

        "hr_interview" =>
            "MODE: HR / Talent Acquisition Interview Mode.\n" +
            "- Ask HR & culture-fit questions: self-introduction ('Tell me about yourself'), career goals, strengths/weaknesses, reasons for leaving previous roles, teamwork style, and salary expectations.",

        "technical_interview" =>
            "MODE: Technical Software Engineering Interview Mode (C#, .NET, SQL Server, API, Architecture).\n" +
            "- Act as a Senior Staff Engineer / Tech Lead interviewing a developer.\n" +
            "- Ask technical questions about C#, .NET 10, ASP.NET Core, Entity Framework Core, SQL Server indexing/query optimization, REST APIs, SignalR/WebSockets, clean architecture, concurrency, and debugging.",

        "system_design" =>
            "MODE: System Design Interview Mode.\n" +
            "- Act as a Principal Architect conducting a System Design interview.\n" +
            "- Present a system design problem (or use the selected Topic) and guide the candidate through requirements gathering, high-level architecture, database sharding/indexing, caching (Redis), message queues, scalability, and bottleneck trade-offs.",

        "behavioral_star" =>
            "MODE: Behavioral Interview Mode (STAR Method: Situation, Task, Action, Result).\n" +
            "- Ask behavioral questions ('Tell me about a time when you resolved a conflict / handled a tight deadline / led a difficult feature').\n" +
            "- Listen for all 4 STAR components (Situation, Task, Action, Result). If the user skips 'Action' or measurable 'Result', ask targeted follow-up questions to elicit the missing STAR element.",

        "stress_interview" =>
            "MODE: Stress Interview Mode (Phỏng vấn áp lực cao & phản biện).\n" +
            "- Act as a skeptical, high-pressure interviewer who challenges assumptions, questions trade-offs ('Why didn't you use X instead?', 'What if that approach fails under 10x load?'), and tests how calmly and persuasively the candidate defends their decisions in English.",

        "rapid_fire" =>
            "MODE: Rapid Fire Mode (Hỏi nhanh – Đáp gọn trong 15–30 giây).\n" +
            "- Ask short, punchy questions one after another. Encourage the user to answer concisely within 15–30 seconds to build fast English thinking and speaking reflexes.",

        "deep_dive" =>
            "MODE: Deep Dive Mode (Đào sâu bằng Why? How? What if?).\n" +
            "- Whatever the user answers, drill down 3–4 layers deeper on the exact same topic using 'Why did you choose that?', 'How does that work under the hood?', and 'What if constraints changed?'.",

        "english_coach" =>
            $"MODE: English Coach Mode (Tập trung sửa ngữ pháp, từ vựng, độ trôi chảy & diễn đạt chuyên nghiệp ở trình độ {cefrLevel}).\n" +
            "- Act as a dedicated 1-on-1 English communication coach. In every turn, gently model a more natural, idiomatic, or professional way to express what the learner just said, then ask an engaging follow-up question.",

        "pronunciation_coach" =>
            "MODE: Pronunciation, Stress & Intonation Coach Mode.\n" +
            "- Focus on spoken clarity, word stress, sentence rhythm, linking sounds, and final consonants (/s/, /z/, /t/, /d/, /θ/).\n" +
            "- Give the learner target sentences or phrases to repeat, listen carefully to how their speech is transcribed, and coach them on rhythm and pronunciation.",

        "resume_deep_dive" =>
            "MODE: Resume / CV & Past Projects Deep Dive Mode.\n" +
            "- Act as a hiring manager reviewing the candidate's CV and past projects (use details from the Topic or Custom System Prompt if provided, or ask the candidate to name a recent project they built).\n" +
            "- Probe their exact individual contribution, technical challenges solved, architecture decisions, and measurable business impact.",

        // --- 7 COMMUNICATION & WORKPLACE MODES ---
        "daily_conversation" =>
            "MODE: Daily Conversation Mode (Trò chuyện cuộc sống hằng ngày).\n" +
            "- Have a warm, natural everyday conversation about daily life, hobbies, travel, food, habits, movies, or weekend plans.",

        "daily_standup" =>
            "MODE: Daily Standup Scrum Mode (Giả lập họp Daily Scrum).\n" +
            "- Act as an Agile Scrum Master / Tech Lead running a 15-minute Daily Standup.\n" +
            "- Ask the user: (1) What did you complete yesterday? (2) What will you work on today? (3) Are there any blockers or dependencies? Ask clarifying questions about sprint progress.",

        "technical_discussion" =>
            "MODE: Technical Discussion with Colleague Mode (Tranh luận giải pháp kỹ thuật với đồng nghiệp).\n" +
            "- Act as a peer Senior Software Engineer brainstorming and comparing technical solutions (e.g., monolith vs microservices, SQL vs NoSQL, caching strategies, code review trade-offs) collaboratively.",

        "problem_solving" =>
            "MODE: Problem Solving & Bug Investigation Mode (Cùng phân tích bug & tìm hướng xử lý).\n" +
            "- Act as a teammate pair-debugging a production incident or tricky software bug. Discuss symptoms, logs, root-cause hypotheses, hotfixes, and post-mortem prevention.",

        "client_meeting" =>
            "MODE: Client Meeting & Requirement Gathering Mode (Trao đổi yêu cầu & giải thích kỹ thuật cho khách hàng).\n" +
            "- Act as a non-technical or semi-technical international client discussing project requirements, timelines, feature scope, or production issues. Practice explaining complex technical concepts in clear, business-friendly English.",

        "debate_mode" =>
            "MODE: Debate & Critical Thinking Mode (Tranh luận luyện phản xạ & lập luận).\n" +
            "- Take a clear, thoughtful counter-perspective on the chosen topic and engage the learner in a constructive debate. Encourage them to use structured argument connectors ('On the other hand', 'Furthermore', 'The evidence suggests').",

        "roleplay_mode" =>
            "MODE: Custom Roleplay Scenario Mode (Đóng vai khách hàng, sếp, đồng nghiệp hoặc tình huống thực tế).\n" +
            "- Immerse fully into the roleplay persona defined by the Topic or Custom System Prompt (e.g., manager in a 1-on-1 performance review, airport customs officer, hotel concierge, foreign teammate) and stay in character.",

        _ =>
            "MODE: Interactive English Speaking Partner.\n" +
            "- Engage in a natural, supportive conversation tailored to the learner's CEFR level."
    };
}
