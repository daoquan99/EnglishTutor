using EnglishTutor.Identity.Contracts.Authorization;

namespace EnglishTutor.Learning.Presentation.Endpoints;

internal static class LearningEndpointAuthorization
{
    public const string OwnerRole = IdentityRoleNames.Owner;
    public const string AdminRole = IdentityRoleNames.Admin;

    public static readonly string[] AdminRoles = [OwnerRole, AdminRole];
}
