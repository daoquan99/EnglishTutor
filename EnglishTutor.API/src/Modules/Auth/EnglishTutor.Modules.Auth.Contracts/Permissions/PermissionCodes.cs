namespace EnglishTutor.Modules.Auth.Contracts.Permissions;

public static class PermissionCodes
{
    public const string FullAccess = "admin.full_access";

    public const string AuthUsersRead = "auth.users.read";
    public const string AuthUsersManage = "auth.users.manage";
    public const string AuthSessionsRead = "auth.sessions.read";
    public const string AuthSecurityEventsRead = "auth.security-events.read";
    public const string AuthSecurityEventsReview = "auth.security-events.review";
    public const string AuthRolesRead = "auth.roles.read";
    public const string AuthRolesManage = "auth.roles.manage";
    public const string AuthPermissionsRead = "auth.permissions.read";
    public const string AuthPermissionsManage = "auth.permissions.manage";

    public const string UsersProfilesRead = "users.profiles.read";
    public const string UsersProfilesManage = "users.profiles.manage";
    public const string UsersLanguagesManage = "users.languages.manage";

    public const string VocabularyItemsRead = "vocabulary.items.read";
    public const string VocabularyItemsManage = "vocabulary.items.manage";
    public const string VocabularyReviewsRead = "vocabulary.reviews.read";

    public const string StudyPlansRead = "studyplans.read";
    public const string StudyPlansManage = "studyplans.manage";

    public const string LearningContentRead = "learningcontent.read";
    public const string LearningContentManage = "learningcontent.manage";

    public const string ExercisesRead = "exercises.read";
    public const string ExercisesManage = "exercises.manage";

    public const string SpeakingSessionsRead = "speaking.sessions.read";
    public const string SpeakingSessionsManage = "speaking.sessions.manage";

    public const string MistakesRead = "mistakes.read";
    public const string MistakesManage = "mistakes.manage";

    public const string AssessmentsRead = "assessments.read";
    public const string AssessmentsManage = "assessments.manage";

    public const string ProgressRead = "progress.read";
    public const string ProgressManage = "progress.manage";

    public const string NotificationsRead = "notifications.read";
    public const string NotificationsManage = "notifications.manage";

    public const string AiProvidersRead = "ai.providers.read";
    public const string AiProvidersManage = "ai.providers.manage";
    public const string AiRoutesRead = "ai.routes.read";
    public const string AiRoutesManage = "ai.routes.manage";
    public const string AiPromptsRead = "ai.prompts.read";
    public const string AiPromptsManage = "ai.prompts.manage";
    public const string AiLogsRead = "ai.logs.read";

    public const string ReportsRead = "reports.read";
    public const string ReportsManage = "reports.manage";

    public static readonly IReadOnlyList<string> All =
    [
        FullAccess,
        AuthUsersRead,
        AuthUsersManage,
        AuthSessionsRead,
        AuthSecurityEventsRead,
        AuthSecurityEventsReview,
        AuthRolesRead,
        AuthRolesManage,
        AuthPermissionsRead,
        AuthPermissionsManage,
        UsersProfilesRead,
        UsersProfilesManage,
        UsersLanguagesManage,
        VocabularyItemsRead,
        VocabularyItemsManage,
        VocabularyReviewsRead,
        StudyPlansRead,
        StudyPlansManage,
        LearningContentRead,
        LearningContentManage,
        ExercisesRead,
        ExercisesManage,
        SpeakingSessionsRead,
        SpeakingSessionsManage,
        MistakesRead,
        MistakesManage,
        AssessmentsRead,
        AssessmentsManage,
        ProgressRead,
        ProgressManage,
        NotificationsRead,
        NotificationsManage,
        AiProvidersRead,
        AiProvidersManage,
        AiRoutesRead,
        AiRoutesManage,
        AiPromptsRead,
        AiPromptsManage,
        AiLogsRead,
        ReportsRead,
        ReportsManage
    ];
}
