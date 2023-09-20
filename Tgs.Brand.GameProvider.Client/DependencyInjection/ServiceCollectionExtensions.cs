using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tgs.Brand.GameProvider.Client.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrandGameProviderClient(
        this IServiceCollection services,
        Action<BrandGameProviderOptions, IServiceProvider> configure)
        => services
            .AddOptions<BrandGameProviderOptions>()
            .Configure(configure)
            .Services
            .AddHttpClient<IBrandGameProviderClient, BrandGameProviderClient>((sp, c) =>
            {
                var options = sp.GetRequiredService<IOptions<BrandGameProviderOptions>>().Value;

                c.BaseAddress = options.Url;
            }).Services;
}