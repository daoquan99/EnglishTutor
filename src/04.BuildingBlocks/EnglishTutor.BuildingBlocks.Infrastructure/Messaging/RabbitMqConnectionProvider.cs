using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed class RabbitMqConnectionProvider : IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;

    public RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options)
    {
        var value = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = value.HostName,
            Port = value.Port,
            UserName = value.UserName,
            Password = value.Password,
            VirtualHost = value.VirtualHost,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_connection is not { IsOpen: true })
            {
                if (_connection is not null)
                {
                    await _connection.DisposeAsync();
                }

                _connection = await _factory.CreateConnectionAsync(
                    clientProvidedName: "EnglishTutor.Worker",
                    cancellationToken: cancellationToken);
            }

            return _connection;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }
}
