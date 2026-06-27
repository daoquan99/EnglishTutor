namespace EnglishTutor.AiGateway.Presentation.Endpoints;

internal static class AiGatewayEndpointAuthorization
{
    public const string OwnerRole = "Owner";
    public const string AdminRole = "Admin";

    // Provider/model/routing management: Admin or Owner.
    public static readonly string[] AdminRoles = [OwnerRole, AdminRole];

    // Provider key management: Owner only.
    public static readonly string[] OwnerOnlyRoles = [OwnerRole];
}
