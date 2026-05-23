using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Infrastructure.Seed;

public static class NotificationTemplateSeedData
{
    public static IReadOnlyList<NotificationTemplate> CreateDefaultTemplates(DateTime utcNow) =>
        DefaultTemplates
            .Select(template => NotificationTemplate.Create(
                template.Type,
                template.LanguageCode,
                template.Title,
                template.Body,
                utcNow))
            .ToArray();

    private static readonly IReadOnlyList<DefaultNotificationTemplate> DefaultTemplates =
    [
        new(NotificationType.StudyReminder, "en", "Time to study!", "Your {targetLanguageCode} session starts at {studyTime}."),
        new(NotificationType.StudyReminder, "vi", "Sap den gio hoc!", "Buoi hoc {targetLanguageCode} cua ban bat dau luc {studyTime}."),
        new(NotificationType.MissedStudyReminder, "en", "You missed today's session", "You missed your {targetLanguageCode} session on {date} at {studyTime}."),
        new(NotificationType.MissedStudyReminder, "vi", "Ban da bo lo buoi hoc hom nay", "Ban da bo lo buoi hoc {targetLanguageCode} ngay {date} luc {studyTime}."),
        new(NotificationType.MistakeReviewReminder, "en", "Review your mistakes", "A short review now will help you retain today's corrections."),
        new(NotificationType.MistakeReviewReminder, "vi", "On lai loi sai", "On tap nhanh bay gio se giup ban ghi nho cac loi da sua."),
        new(NotificationType.VocabularyReviewReminder, "en", "Vocabulary review is ready", "Your due vocabulary cards are waiting."),
        new(NotificationType.VocabularyReviewReminder, "vi", "Den lich on tu vung", "Cac the tu vung can on da san sang."),
        new(NotificationType.DailyTargetCompleted, "en", "Daily target completed", "You studied {actualMinutes}/{targetMinutes} minutes for {targetLanguageCode} on {date}."),
        new(NotificationType.DailyTargetCompleted, "vi", "Da hoan thanh muc tieu ngay", "Ban da hoc {actualMinutes}/{targetMinutes} phut cho {targetLanguageCode} vao ngay {date}."),
        new(NotificationType.WeeklyProgressSummary, "en", "Your weekly summary is ready", "Open your dashboard to review this week's progress."),
        new(NotificationType.WeeklyProgressSummary, "vi", "Tong ket tuan da san sang", "Mo dashboard de xem tien do tuan nay."),
        new(NotificationType.MonthlyProgressSummary, "en", "Your monthly summary is ready", "Review your monthly progress and plan the next goal."),
        new(NotificationType.MonthlyProgressSummary, "vi", "Tong ket thang da san sang", "Xem tien do thang va len muc tieu tiep theo."),
        new(NotificationType.AssessmentReminder, "en", "Assessment reminder", "Your assessment is ready when you are."),
        new(NotificationType.AssessmentReminder, "vi", "Nhac lam bai danh gia", "Bai danh gia cua ban da san sang."),
        new(NotificationType.LevelUpCongratulations, "en", "Level up!", "You have reached {level}. Keep practicing."),
        new(NotificationType.LevelUpCongratulations, "vi", "Len cap!", "Ban da dat trinh do {level}. Hay tiep tuc luyen tap.")
    ];

    private sealed record DefaultNotificationTemplate(
        NotificationType Type,
        string LanguageCode,
        string Title,
        string Body);
}
