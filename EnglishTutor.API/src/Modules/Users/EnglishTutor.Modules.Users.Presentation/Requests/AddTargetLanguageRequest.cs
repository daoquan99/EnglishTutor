namespace EnglishTutor.Modules.Users.Presentation.Requests;

public sealed record AddTargetLanguageRequest(
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel);
