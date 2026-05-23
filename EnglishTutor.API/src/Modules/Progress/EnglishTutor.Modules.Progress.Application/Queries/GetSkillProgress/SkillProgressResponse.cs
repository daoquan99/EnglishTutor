namespace EnglishTutor.Modules.Progress.Application.Queries.GetSkillProgress;

public sealed record SkillProgressResponse(
    string Skill,
    int Score,
    int ActivityCount,
    DateTime UpdatedAtUtc);
