using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Application.Commands.Refresh;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Commands.Refresh;

public sealed class RefreshCommandHandler : IRequestHandler<RefreshCommand, Result<RefreshResult>>
{
    private readonly IdentityDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenLifetimeProvider _refreshTokenLifetime;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenHasher _refreshTokenHasher;

    public RefreshCommandHandler(
        IdentityDbContext db,
        IJwtTokenService jwtTokenService,
        IRefreshTokenLifetimeProvider refreshTokenLifetime,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenHasher refreshTokenHasher)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _refreshTokenLifetime = refreshTokenLifetime;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenHasher = refreshTokenHasher;
    }

    public async Task<Result<RefreshResult>> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = _refreshTokenHasher.Hash(request.RefreshToken);
        var existing = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return RefreshResults.InvalidRefreshToken();
        }

        // Reuse detection: if token already used, revoke entire family.
        if (existing.UsedAtUtc is not null)
        {
            existing.DetectReuse(request.IpAddress);
            await _db.SaveChangesAsync(cancellationToken);
            return RefreshResults.ReuseDetected(existing.FamilyId);
        }

        // Mark used + issue replacement (raw + hash + lifetime via Application abstractions).
        var newValue = _refreshTokenGenerator.Generate();
        var newHash = _refreshTokenHasher.Hash(newValue);
        var newExpires = DateTime.UtcNow.Add(_refreshTokenLifetime.RefreshTokenLifetime);

        var replacement = RefreshToken.Issue(
            existing.UserId, existing.FamilyId, newHash, newExpires, request.IpAddress);
        _db.RefreshTokens.Add(replacement);

        existing.MarkUsed(replacement.Id, DateTime.UtcNow);

        // Issue new access token
        var user = await _db.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == existing.UserId, cancellationToken);
        var roleIds = user.RoleIds.ToList();
        var roleNames = await _db.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToListAsync(cancellationToken);
        var jwt = _jwtTokenService.IssueAccessToken(user.Id, user.Email.Value, user.DisplayName, roleNames, Array.Empty<string>());

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshResult(jwt.Token, newValue, jwt.ExpiresAt));
    }
}
