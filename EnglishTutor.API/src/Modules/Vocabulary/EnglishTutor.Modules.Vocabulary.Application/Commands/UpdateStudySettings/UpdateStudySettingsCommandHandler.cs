using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudySettings;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.UpdateStudySettings;

public sealed class UpdateStudySettingsCommandHandler(
    IVocabularyStudySettingsRepository settingsRepository,
    IDateTimeProvider dateTimeProvider,
    IVocabularyUnitOfWork unitOfWork)
    : ICommandHandler<UpdateStudySettingsCommand, StudySettingsResponse>
{
    public async Task<Result<StudySettingsResponse>> Handle(UpdateStudySettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.GetAsync(request.UserId, request.TargetLanguageCode, cancellationToken);
        if (settings is null)
        {
            settings = VocabularyStudySettings.CreateDefault(
                request.UserId,
                LanguageCode.Create(request.TargetLanguageCode),
                dateTimeProvider.UtcNow);
            await settingsRepository.AddAsync(settings, cancellationToken);
        }

        settings.Update(request.NewWordsPerDay, request.ReviewWordsPerDay, request.IncludeMasteredInReview);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new StudySettingsResponse(
            settings.NewWordsPerDay,
            settings.ReviewWordsPerDay,
            settings.IncludeMasteredInReview);
    }
}
