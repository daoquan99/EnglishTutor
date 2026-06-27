using EnglishTutor.Identity.Contracts.Authorization;

namespace EnglishTutor.AiGateway.Presentation.Endpoints;

internal static class AiGatewayEndpointAuthorization
{
    public const string OwnerRole = IdentityRoleNames.Owner;
    public const string AdminRole = IdentityRoleNames.Admin;

    // Provider/model/routing management: Admin or Owner.
    public static readonly string[] AdminRoles = [OwnerRole, AdminRole];

    // Provider key management: Owner only.
    public static readonly string[] OwnerOnlyRoles = [OwnerRole];
}
