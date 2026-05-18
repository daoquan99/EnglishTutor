using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;
using EnglishTutor.Modules.Users.Application.Errors;
using EnglishTutor.Modules.Users.Domain.Entities;

namespace EnglishTutor.Modules.Users.Application.Commands.AddTargetLanguage;

public sealed class AddTargetLanguageCommandHandler(
    IUserTargetLanguageRepository userTargetLanguageRepository,
    IUsersUnitOfWork unitOfWork)
    : ICommandHandler<AddTargetLanguageCommand, TargetLanguageResponse>
{
    public async Task<Result<TargetLanguageResponse>> Handle(AddTargetLanguageCommand request, CancellationToken cancellationToken)
    {
        var existing = await userTargetLanguageRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (existing.Any(language => language.TargetLanguageCode.Value == request.TargetLanguageCode))
        {
            return Result.Failure<TargetLanguageResponse>(UserErrors.TargetLanguageAlreadyExists);
        }

        if (!Enum.TryParse<LanguageLevel>(request.CurrentLevel, true, out var currentLevel) ||
            !Enum.TryParse<LanguageLevel>(request.TargetLevel, true, out var targetLevel))
        {
            return Result.Failure<TargetLanguageResponse>(Error.Validation("Language levels are invalid."));
        }

        var targetLanguage = UserTargetLanguage.Create(
            request.UserId,
            LanguageCode.Create(request.TargetLanguageCode),
            currentLevel,
            targetLevel);

        await userTargetLanguageRepository.AddAsync(targetLanguage, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TargetLanguageResponse(
            targetLanguage.Id,
            targetLanguage.UserId,
            targetLanguage.TargetLanguageCode.Value,
            targetLanguage.CurrentLevel.ToString(),
            targetLanguage.TargetLevel.ToString(),
            targetLanguage.IsActive);
    }
}
