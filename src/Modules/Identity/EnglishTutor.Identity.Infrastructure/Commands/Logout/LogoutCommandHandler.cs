using System.Security.Cryptography;
using System.Text;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Commands.Logout;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<MediatR.Unit>>
{
    private readonly IdentityDbContext _db;

    public LogoutCommandHandler(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MediatR.Unit>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = Hash(request.RefreshToken);
        var token = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (token is null)
        {
            return Result.Success(MediatR.Unit.Value);
        }

        // Revoke the entire family so any subsequent refresh attempts fail.
        token.RevokeFamily(DateTime.UtcNow, ipAddress: null);
        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success(MediatR.Unit.Value);
    }

    private static string Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
