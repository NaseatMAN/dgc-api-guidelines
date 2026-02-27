using DGC.Sample.Application.Common.Queue;
using DGC.Sample.Application.Interfaces.Queue;
using DGC.Sample.Domain.Exceptions;

namespace DGC.Sample.Infrastructure.Queue;

public sealed class TransportResolver<T> : ITransportResolver<T>
{
    private readonly IReadOnlyDictionary<QueueTransport, IMessageQueueTransport<T>> _map;

    public TransportResolver(IEnumerable<IMessageQueueTransport<T>> transports)
    {
        var byType = transports
            .GroupBy(transport => transport.TransportType)
            .ToDictionary(group => group.Key, group => group.ToArray());

        var duplicates = byType
            .Where(pair => pair.Value.Length > 1)
            .Select(pair => pair.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new InternalErrorException(
                InternalErrorCode.QueueTransportInitializationError,
                $"Duplicate queue transports registered for message type '{typeof(T).Name}': {string.Join(", ", duplicates)}",
                azureErrorDetails: null);
        }

        _map = byType.ToDictionary(pair => pair.Key, pair => pair.Value[0]);
    }

    public IMessageQueueTransport<T> Resolve(QueueTransport transport)
    {
        return _map.TryGetValue(transport, out var transportImpl)
            ? transportImpl
            : throw new InternalErrorException(
                InternalErrorCode.QueueTransportNotRegistered,
                $"Queue transport '{transport}' is not registered for message type '{typeof(T).Name}'.",
                azureErrorDetails: null);
    }

    public bool TryResolve(QueueTransport transport, out IMessageQueueTransport<T>? transportImpl)
    {
        return _map.TryGetValue(transport, out transportImpl);
    }
}