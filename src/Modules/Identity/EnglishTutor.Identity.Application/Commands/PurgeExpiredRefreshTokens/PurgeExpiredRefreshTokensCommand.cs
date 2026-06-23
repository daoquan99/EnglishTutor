using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Identity.Application.Commands.PurgeExpiredRefreshTokens;

/// <summary>
/// Command to purge expired refresh tokens that are older than the retention threshold.
/// </summary>
public sealed record PurgeExpiredRefreshTokensCommand(int RetentionDays) : ICommand<int>;
