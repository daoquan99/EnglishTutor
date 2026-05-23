namespace EnglishTutor.Modules.Notifications.Application.Shared.Services;

public static class NotificationTemplateRenderer
{
    public static string Render(string template, IReadOnlyDictionary<string, string> values)
    {
        var result = template;
        foreach (var (key, value) in values)
        {
            result = result.Replace("{" + key + "}", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
