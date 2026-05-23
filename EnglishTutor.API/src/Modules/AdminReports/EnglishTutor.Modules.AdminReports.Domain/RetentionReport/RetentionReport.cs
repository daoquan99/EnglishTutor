using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.AdminReports.Domain.Shared;

namespace EnglishTutor.Modules.AdminReports.Domain.RetentionReport;

public sealed class RetentionReport : Entity<Guid>
{
    public DateOnly ReportDate { get; private set; }
    public ReportPeriod Period { get; private set; }
    public int ActiveUsers { get; private set; }
    public int ReturningUsers { get; private set; }
    public decimal RetentionRate { get; private set; }

    private RetentionReport() { }

    public static RetentionReport Create(DateOnly reportDate, ReportPeriod period, int activeUsers, int returningUsers) =>
        new RetentionReport
        {
            Id = Guid.NewGuid(),
            ReportDate = reportDate,
            Period = period
        }.Update(activeUsers, returningUsers);

    public RetentionReport Update(int activeUsers, int returningUsers)
    {
        ActiveUsers = Math.Max(0, activeUsers);
        ReturningUsers = Math.Clamp(returningUsers, 0, ActiveUsers);
        RetentionRate = ActiveUsers == 0 ? 0 : Math.Round(ReturningUsers / (decimal)ActiveUsers, 4, MidpointRounding.AwayFromZero);
        return this;
    }
}
