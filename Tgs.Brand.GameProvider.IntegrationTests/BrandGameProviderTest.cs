using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StackExchange.Redis;
using Tgs.Brand.GameProvider.Client;
using Tgs.Brand.GameProvider.Core;
using Tgs.Brand.GameProvider.Domain;

namespace Tgs.Brand.GameProvider.IntegrationTests;

[TestFixture]
[Parallelizable]
public class BrandGameProviderTest
{
    [Test]
    public async Task Do_Not_Change_Brand_Game_Provider_Status_When_Query_No_Data()
    {
        var fakeBrandGameProviderRepository = Substitute.For<IBrandGameProviderRepository>();
        var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        {
            fakeRedisConnectionFactory.GetDataBase().Returns(fakeRedisDatabase);
        }

        await using var web = new TestApplication(builder =>
            builder.ConfigureServices(service =>
                {
                    service.AddSingleton(fakeBrandGameProviderRepository);
                    service.AddSingleton(fakeRedisConnectionFactory);
                    service.AddSingleton(fakeRedisDatabase);
                }
            ));

        var httpClient = web.CreateClient();
        {
            var sut = new BrandGameProviderClient(httpClient);

            await sut.ChangeStatusByScheduleAsync();

            await fakeRedisDatabase.Received(1).KeyDeleteAsync(Arg.Is<RedisKey[]>(Array.Empty<RedisKey>()));

            await fakeBrandGameProviderRepository.Received(2).GetBrandGameProviderIdsAsync(
                Arg.Any<Status>(),
                Arg.Any<Status>());

            await fakeBrandGameProviderRepository.UpdateBrandGameProvidersAsync(
                Arg.Is(""),
                Arg.Is(""));
        }
    }

    [Test]
    public async Task Change_Brand_Game_Provider_Status()
    {
        var fakeBrandGameProviderRepository = Substitute.For<IBrandGameProviderRepository>();
        var fakeRedisConnectionFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();

        var brandGameProvider1 = Guid.NewGuid();
        var brandGameProvider2 = Guid.NewGuid();
        {
            fakeRedisConnectionFactory.GetDataBase().Returns(fakeRedisDatabase);

            fakeBrandGameProviderRepository
                .GetBrandGameProviderIdsAsync(Arg.Is(Status.Active), Arg.Is(Status.Maintenance))
                .Returns(new Guid[] { brandGameProvider1 });

            fakeBrandGameProviderRepository
                .GetBrandGameProviderIdsAsync(Arg.Is(Status.Maintenance), Arg.Is(Status.Active))
                .Returns(new Guid[] { brandGameProvider2 });
        }

        await using var web = new TestApplication(builder =>
            builder.ConfigureServices(service =>
                {
                    service.AddSingleton(fakeBrandGameProviderRepository);
                    service.AddSingleton(fakeRedisConnectionFactory);
                    service.AddSingleton(fakeRedisDatabase);
                }
            ));

        var httpClient = web.CreateClient();
        {
            var sut = new BrandGameProviderClient(httpClient);

            await sut.ChangeStatusByScheduleAsync();

            var readyForDeleteKeys = new RedisKey[]
            {
                $"BrandGameProviders:{brandGameProvider1}",
                $"BrandGameProviders:{brandGameProvider2}"
            };

            await fakeRedisDatabase.Received(1)
                .KeyDeleteAsync(Arg.Is<RedisKey[]>(x => readyForDeleteKeys.SequenceEqual(x)));

            await fakeBrandGameProviderRepository.Received(1).GetBrandGameProviderIdsAsync(
                Arg.Is<Status>(Status.Active),
                Arg.Is<Status>(Status.Maintenance));

            await fakeBrandGameProviderRepository.Received(1).GetBrandGameProviderIdsAsync(
                Arg.Is<Status>(Status.Maintenance),
                Arg.Is<Status>(Status.Active));

            await fakeBrandGameProviderRepository.UpdateBrandGameProvidersAsync(
                Arg.Is(string.Join(",", new Guid[] { brandGameProvider1 })),
                Arg.Is(string.Join(",", new Guid[] { brandGameProvider2 })));
        }
    }
}