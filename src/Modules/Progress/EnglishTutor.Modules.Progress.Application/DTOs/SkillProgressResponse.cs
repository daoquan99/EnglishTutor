namespace EnglishTutor.Modules.Progress.Application.DTOs;

public sealed record SkillProgressResponse(
    string Skill,
    int Score,
    int ActivityCount,
    DateTime UpdatedAtUtc);
