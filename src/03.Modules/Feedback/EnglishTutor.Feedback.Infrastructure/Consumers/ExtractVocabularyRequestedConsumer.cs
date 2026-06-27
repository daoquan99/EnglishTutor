using System.Threading.Tasks;
using EnglishTutor.Feedback.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Feedback.Infrastructure.Consumers;

public sealed class ExtractVocabularyRequestedConsumer : IConsumer<ExtractVocabularyRequestedV1>
{
    private readonly ILogger<ExtractVocabularyRequestedConsumer> _logger;

    public ExtractVocabularyRequestedConsumer(ILogger<ExtractVocabularyRequestedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ExtractVocabularyRequestedV1> context)
    {
        var ev = context.Message;
        _logger.LogInformation("Consuming ExtractVocabularyRequested for session {SessionId} (vocabulary is extracted inline during feedback generation).", ev.SessionId);
        await Task.CompletedTask;
    }
}
