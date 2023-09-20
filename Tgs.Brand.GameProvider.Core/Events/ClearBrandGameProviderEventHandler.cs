using MediatR;
using StackExchange.Redis;
using Tgs.Brand.GameProvider.Domain.Events;

namespace Tgs.Brand.GameProvider.Core.Events;

internal class ClearBrandGameProviderEventHandler : INotificationHandler<ClearBrandGameProviderEvent>
{
    private readonly IMediator _mediator;
    private readonly IRedisConnectionFactory _redisConnectionFactory;

    public ClearBrandGameProviderEventHandler(IRedisConnectionFactory redisConnectionFactory, IMediator mediator)
    {
        _redisConnectionFactory = redisConnectionFactory;
        _mediator = mediator;
    }

    public async Task Handle(ClearBrandGameProviderEvent @event, CancellationToken cancellationToken)
    {
        var db = _redisConnectionFactory.GetDataBase();

        var keys = @event.Keys.Select(x => (RedisKey)x).ToArray();

        _ = await db.KeyDeleteAsync(keys);
    }
}