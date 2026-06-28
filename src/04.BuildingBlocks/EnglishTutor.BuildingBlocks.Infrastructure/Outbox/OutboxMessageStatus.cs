namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

public enum OutboxMessageStatus
{
    Pending = 0,
    Publishing = 1,
    Published = 2,
    Failed = 3,
    Dead = 4
}
