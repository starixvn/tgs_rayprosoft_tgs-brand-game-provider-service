using Tgs.Brand.GameProvider.Domain;

namespace Tgs.Brand.GameProvider.Core;

public interface IBrandGameProviderRepository
{
    Task<Guid[]> GetBrandGameProviderIdsAsync(Status sourceStatus, Status targetStatus);
    Task UpdateBrandGameProvidersAsync(string toMaintenanceIds, string toActiveIds);
}