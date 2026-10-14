namespace EnglishLiveTutor.Api.Models;

public sealed record StartTutorSessionRequest(
    string SessionId,
    string ApiKey,
    string CefrLevel = "B1",
    bool ExtendedThinking = false,
    string ThinkingLevel = "medium",
    string VoiceName = "Puck"
);

public sealed record StartSubHubSessionRequest(
    string SessionId,
    string ApiKey
);

public sealed record AudioChunkPayload(
    string SessionId,
    string Speaker, // "user" | "ai"
    string TurnId,
    string Base64Pcm16k
);

public sealed record TranscribeEventDto(
    string SessionId,
    string Speaker, // "user" | "ai"
    string TurnId,
    string Text,
    bool IsFinal,
    DateTimeOffset Timestamp
);

public sealed record TranslateEventDto(
    string SessionId,
    string Speaker, // "user" | "ai"
    string TurnId,
    string VietnameseText,
    bool IsFinal,
    DateTimeOffset Timestamp
);

public sealed record LanguageCorrectionDto(
    string SessionId,
    string TurnId,
    string OriginalText,
    string CorrectedText,
    string ErrorCategory, // "Spelling & Word Choice" | "Grammar" | "Pronunciation"
    string ExplanationVi,
    string CefrTip,
    DateTimeOffset Timestamp
);

public sealed record TutorAudioEventDto(
    string SessionId,
    string TurnId,
    string Base64Pcm24k,
    string InteractionStatus // "IN_PROGRESS" | "IDLE"
);
