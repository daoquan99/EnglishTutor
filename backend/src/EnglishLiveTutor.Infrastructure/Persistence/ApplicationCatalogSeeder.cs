using EnglishLiveTutor.Domain.Identity;
using EnglishLiveTutor.Domain.TeacherVoices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EnglishLiveTutor.Infrastructure.Persistence;

/// <summary>Idempotent catalog seeding. Existing administrator-edited prompt/voice rows are preserved.</summary>
public static class ApplicationCatalogSeeder
{
    public static async Task SeedAsync(IDbContextFactory<AppDbContext> factory,
        IConfiguration configuration, IPasswordHasher<UserEntity> hasher, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var existingPrompts = await db.AiPromptTemplates.Select(x => x.Code).ToArrayAsync(ct);
        var promptSet = existingPrompts.ToHashSet(StringComparer.Ordinal);
        foreach (var row in CefrPromptSeedCatalog.CreatePrompts())
            if (promptSet.Add(row.Code)) db.AiPromptTemplates.Add(row);

        var existingVoices = (await db.TeacherVoices.Select(x => x.Code).ToArrayAsync(ct))
            .ToHashSet(StringComparer.Ordinal);
        foreach (var voice in TeacherVoices())
            if (existingVoices.Add(voice.Code)) db.TeacherVoices.Add(voice);

        var permissions = new (string Code, string Description)[]
        {
            ("users.read", "View user accounts"), ("users.manage", "Enable/disable and assign user roles"),
            ("roles.read", "View roles and permissions"), ("roles.manage", "Assign role permissions"),
            ("prompts.read", "View AI prompt templates"), ("prompts.manage", "Edit AI prompt templates"),
            ("voices.manage", "Enable/disable Google teacher voices")
        };
        var known = (await db.Permissions.Select(x => x.Code).ToArrayAsync(ct)).ToHashSet();
        foreach (var (code, desc) in permissions)
            if (known.Add(code)) db.Permissions.Add(new PermissionEntity(code, desc));
        var roles = new (string Name, string Description)[]
        {
            ("Admin", "Administrator"), ("Teacher", "Teacher"), ("Student", "English learner")
        };
        var roleNames = (await db.Roles.Select(x => x.Name).ToArrayAsync(ct)).ToHashSet();
        var newRoles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (name, desc) in roles)
            if (roleNames.Add(name))
            {
                db.Roles.Add(new RoleEntity(name, desc));
                newRoles.Add(name);
            }
        await db.SaveChangesAsync(ct);

        var allPermissions = await db.Permissions.ToDictionaryAsync(x => x.Code, x => x.Id, ct);
        var allRoles = await db.Roles.ToDictionaryAsync(x => x.Name, x => x.Id, ct);
        var granted = await db.RolePermissions.Select(x => new { x.RoleId, x.PermissionId }).ToArrayAsync(ct);
        var assigned = granted.Select(x => (x.RoleId, x.PermissionId)).ToHashSet();
        foreach (var code in allPermissions.Keys)
        {
            var key = (allRoles["Admin"], allPermissions[code]);
            if (assigned.Add(key)) db.RolePermissions.Add(new RolePermissionEntity(key.Item1, key.Item2));
        }
        // Teachers may inspect but not edit prompts.
        if (newRoles.Contains("Teacher"))
        {
            foreach (var code in new[] { "prompts.read", "roles.read" })
            {
                var key = (allRoles["Teacher"], allPermissions[code]);
                if (assigned.Add(key)) db.RolePermissions.Add(new RolePermissionEntity(key.Item1, key.Item2));
            }
        }

        var email = configuration["Auth:BootstrapAdmin:Email"];
        var password = configuration["Auth:BootstrapAdmin:Password"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            if (password.Length < 12) throw new InvalidOperationException("Bootstrap admin password must be at least 12 characters.");
            string normalized = UserEntity.NormalizeEmail(email);
            var admin = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalized, ct);
            var adminRoleId = allRoles["Admin"];
            if (admin is null)
            {
                admin = new UserEntity(email, "Administrator", "");
                admin.SetPasswordHash(hasher.HashPassword(admin, password));
                db.Users.Add(admin);
                db.UserRoles.Add(new UserRoleEntity(admin.Id, adminRoleId));
            }
            else if (!await db.UserRoles.AnyAsync(x => x.UserId == admin.Id && x.RoleId == adminRoleId, ct))
            {
                // Never elevate a pre-existing untrusted account just because the email matches configuration.
                throw new InvalidOperationException("Bootstrap admin email belongs to a non-admin account. Refusing privilege escalation.");
            }
        }
        await db.SaveChangesAsync(ct);
    }

    private static IEnumerable<TeacherVoiceEntity> TeacherVoices()
    {
        // Google Live API: case-sensitive prebuilt voice codes, currently 30 voices.
        var entries = new (string Code, string Gender, string Persona, string Description)[]
        {
            ("Zephyr", "Female", "Bright", "Bright and clear female voice."),
            ("Puck", "Male", "Upbeat", "Upbeat and lively male voice, ideal for conversational interaction."),
            ("Charon", "Male", "Informative", "Calm and informative male voice, well suited for instruction."),
            ("Kore", "Female", "Firm", "Firm, articulate female voice suitable for general conversation."),
            ("Fenrir", "Male", "Excitable", "Excitable and energetic male voice with crisp delivery."),
            ("Leda", "Female", "Youthful", "Youthful and soft female voice, great for patient guidance."),
            ("Orus", "Male", "Firm", "Firm, confident and direct male voice."),
            ("Aoede", "Female", "Breezy", "Breezy, natural female voice suited for narration."),
            ("Callirrhoe", "Female", "Easy-going", "Easy-going and pleasant female voice."),
            ("Autonoe", "Female", "Bright", "Bright and expressive female voice."),
            ("Enceladus", "Male", "Breathy", "Soft and breathy male voice."),
            ("Iapetus", "Male", "Clear", "Clear and clean male voice with casual tone."),
            ("Umbriel", "Male", "Easy-going", "Easy-going and relaxed male voice."),
            ("Algieba", "Female", "Smooth", "Smooth and articulate female voice."),
            ("Despina", "Female", "Smooth", "Smooth and soothing female voice."),
            ("Erinome", "Female", "Clear", "Clear and poised female voice."),
            ("Algenib", "Male", "Gravelly", "Gravelly, textured male voice."),
            ("Rasalgethi", "Male", "Informative", "Informative and measured male voice."),
            ("Laomedeia", "Female", "Upbeat", "Upbeat and cheerful female voice."),
            ("Achernar", "Female", "Soft", "Soft and gentle female voice."),
            ("Alnilam", "Male", "Firm", "Firm and resonant male voice."),
            ("Schedar", "Male", "Even", "Even and balanced male voice."),
            ("Gacrux", "Male", "Mature", "Mature and grounded male voice."),
            ("Pulcherrima", "Female", "Forward", "Forward and engaging female voice."),
            ("Achird", "Male", "Friendly", "Friendly and approachable male voice."),
            ("Zubenelgenubi", "Male", "Casual", "Casual and conversational male voice."),
            ("Vindemiatrix", "Female", "Gentle", "Gentle and compassionate female voice."),
            ("Sadachbia", "Female", "Lively", "Lively and energetic female voice."),
            ("Sadaltager", "Male", "Knowledgeable", "Knowledgeable and articulate male voice."),
            ("Sulafat", "Female", "Warm", "Warm and inviting female voice.")
        };
        var recommended = new Dictionary<string, (string Label, string Style)>(StringComparer.Ordinal)
        {
            ["Aoede"] = ("Aoede (Nữ)", "Truyền cảm, ấm áp & rõ chữ"),
            ["Puck"] = ("Puck (Nam)", "Thân thiện, tự nhiên & năng động"),
            ["Kore"] = ("Kore (Nữ)", "Điềm tĩnh, chuẩn mực sư phạm"),
            ["Charon"] = ("Charon (Nam)", "Trầm ấm, phong thái học thuật"),
            ["Fenrir"] = ("Fenrir (Nam)", "Dứt khoát, rõ ràng, tốc độ tốt"),
            ["Leda"] = ("Leda (Nữ)", "Nhẹ nhàng, kiên nhẫn cho A1-B1")
        };
        int order = 0;
        foreach (var row in entries)
        {
            var isRecommended = recommended.TryGetValue(row.Code, out var vi);
            yield return new TeacherVoiceEntity(row.Code, row.Gender, row.Persona,
                row.Description, isRecommended ? vi.Label : null,
                isRecommended ? vi.Style : null, ++order, isRecommended);
        }
    }
}
