namespace EnglishTutor.BuildingBlocks.Infrastructure.Options;

public sealed class DatabaseStartupOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; set; }
    public bool ApplySeedDataOnStartup { get; set; } = true;
    public bool? ApplyAuditMigrationsOnStartup { get; set; }
    public Dictionary<string, ModuleDatabaseStartupOptions> Modules { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool ShouldApplyMigrations(string moduleName)
    {
        if (Modules.TryGetValue(moduleName, out var module) && module.ApplyMigrationsOnStartup.HasValue)
        {
            return module.ApplyMigrationsOnStartup.Value;
        }

        if (string.Equals(moduleName, DatabaseStartupModuleNames.Audit, StringComparison.OrdinalIgnoreCase)
            && ApplyAuditMigrationsOnStartup.HasValue)
        {
            return ApplyAuditMigrationsOnStartup.Value;
        }

        return ApplyMigrationsOnStartup;
    }
}

public sealed class ModuleDatabaseStartupOptions
{
    public bool? ApplyMigrationsOnStartup { get; set; }
}

public static class DatabaseStartupModuleNames
{
    public const string Identity = "Identity";
    public const string Audit = "Audit";
    public const string Learning = "Learning";
    public const string AiGateway = "AiGateway";
    public const string Quota = "Quota";
    public const string Practice = "Practice";
    public const string Feedback = "Feedback";
}
