using MediatR;
using Tgs.Brand.GameProvider.Core.Models;
using Tgs.Brand.GameProvider.Domain;
using Tgs.Brand.GameProvider.Domain.Events;

namespace Tgs.Brand.GameProvider.Core;

public class
    BrandGameProviderHandler : IRequestHandler<BrandGameProviderScheduleRequest>
{
    private readonly IMediator _mediator;
    private readonly IBrandGameProviderRepository _repository;

    public BrandGameProviderHandler(
        IBrandGameProviderRepository repository,
        IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task Handle(
        BrandGameProviderScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var brandGameProvidersToBeMaintenanceIds =
            await _repository.GetBrandGameProviderIdsAsync(Status.Active, Status.Maintenance);

        var brandGameProvidersToBeActivatedIds =
            await _repository.GetBrandGameProviderIdsAsync(Status.Maintenance, Status.Active);

        var brandGameProviders = brandGameProvidersToBeMaintenanceIds
            .Union(brandGameProvidersToBeActivatedIds)
            .ToArray();

        var cacheKeys = brandGameProviders
            .Select(id => $"BrandGameProviders:{id}")
            .Distinct()
            .ToArray();

        await _repository.UpdateBrandGameProvidersAsync(string.Join(",", brandGameProvidersToBeMaintenanceIds),
            string.Join(",", brandGameProvidersToBeActivatedIds));

        await _mediator.Publish(new ClearBrandGameProviderEvent(cacheKeys), cancellationToken);
    }
}