namespace EnglishTutor.Learning.Presentation.Endpoints;

internal static class LearningEndpointAuthorization
{
    public const string OwnerRole = "Owner";
    public const string AdminRole = "Admin";

    public static readonly string[] AdminRoles = [OwnerRole, AdminRole];
}
