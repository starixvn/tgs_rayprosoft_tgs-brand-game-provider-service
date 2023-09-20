using MediatR;
using NSubstitute;
using NUnit.Framework;
using Tgs.Brand.GameProvider.Core;
using Tgs.Brand.GameProvider.Core.Models;
using Tgs.Brand.GameProvider.Domain.Events;

namespace Tgs.Brand.GameProvider.UnitTests;

[TestFixture]
[Parallelizable]
public class BrandGameProviderHandlerTests
{
    [Test]
    public async Task Status_Change_By_Schedule()
    {
        // Arrange
        var fakeMediator = Substitute.For<IMediator>();
        var fakeBrandGameProviderRepository = Substitute.For<IBrandGameProviderRepository>();

        var sut = new BrandGameProviderHandler(
            fakeBrandGameProviderRepository,
            fakeMediator);

        var request = new BrandGameProviderScheduleRequest();

        // Act
        await sut.Handle(request, default);

        // Assert
        _ = fakeMediator.Received(1)
            .Publish(Arg.Any<ClearBrandGameProviderEvent>(), Arg.Any<CancellationToken>());

        _ = fakeBrandGameProviderRepository.Received(1)
            .UpdateBrandGameProvidersAsync(Arg.Any<string>(), Arg.Any<string>());
    }
}