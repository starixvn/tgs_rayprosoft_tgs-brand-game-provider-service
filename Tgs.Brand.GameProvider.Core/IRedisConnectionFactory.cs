using StackExchange.Redis;

namespace Tgs.Brand.GameProvider.Core;

public interface IRedisConnectionFactory
{
    public IDatabase GetDataBase();
}