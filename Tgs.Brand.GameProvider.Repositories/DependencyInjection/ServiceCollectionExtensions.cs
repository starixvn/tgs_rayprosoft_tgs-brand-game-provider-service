using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tgs.Brand.GameProvider.Core;
using Tgs.Brand.GameProvider.Repositories.Entities;

namespace Tgs.Brand.GameProvider.Repositories.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrandGameProviderRepositories(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextConfigure)
    {
        return services
            .AddDbContext<tgsContext>(dbContextConfigure)
            .AddTransient<IBrandGameProviderRepository, BrandGameProviderRepository>();
    }

    public static IServiceCollection AddRedis(
        this IServiceCollection service,
        Action<RedisOptions, IServiceProvider> configure)
    {
        _ = service
            .AddOptions<RedisOptions>()
            .Configure(configure)
            .Services
            .AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();

        return service;
    }
}