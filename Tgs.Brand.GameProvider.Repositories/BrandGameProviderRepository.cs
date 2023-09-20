using Dapper;
using Microsoft.EntityFrameworkCore;
using Tgs.Brand.GameProvider.Core;
using Tgs.Brand.GameProvider.Domain;
using Tgs.Brand.GameProvider.Repositories.Entities;

namespace Tgs.Brand.GameProvider.Repositories;

public class BrandGameProviderRepository : IBrandGameProviderRepository
{
    private readonly tgsContext _tgsContext;

    public BrandGameProviderRepository(tgsContext tgsContext)
    {
        _tgsContext = tgsContext;
    }

    public async Task<Guid[]> GetBrandGameProviderIdsAsync(Status sourceStatus, Status targetStatus)
    {
        var offset = new TimeSpan(8, 0, 0);
        var dayOfWeek = DateTimeOffset.Now.ToOffset(offset).DayOfWeek;
        var now = DateTimeOffset.Now.ToOffset(offset);

        var sourceBrandGameProviders = await _tgsContext.BrandGameProviders
            .Where(x => x.Status == (short)sourceStatus && !x.IsManualChangeStatus)
            .Include(x => x.BrandGameProviderSchedules)
            .ToArrayAsync();

        var brandGameProvidersToUpdate = sourceBrandGameProviders
            .Where(x => (!x.BrandGameProviderSchedules.Any(Predicate) && targetStatus == Status.Active) ||
                        (x.BrandGameProviderSchedules.Any(Predicate) && targetStatus == Status.Maintenance))
            .ToArray();

        return brandGameProvidersToUpdate.Select(x => x.Id).ToArray();

        bool Predicate(BrandGameProviderSchedule s)
        {
            var from = s.From.ToOffset(offset);
            var to = s.To.ToOffset(offset);

            var fromHm = new DateTimeOffset(now.Year,
                now.Month,
                now.Day,
                from.Hour,
                from.Minute,
                0,
                offset);

            var toHm = new DateTimeOffset(now.Year,
                now.Month,
                now.Day,
                to.Hour,
                to.Minute,
                0,
                offset);

            return s.Status == (short)SwitchStatus.Enabled &&
                   ((s.IsDefault &&
                     (short)dayOfWeek == s.DayOfWeek &&
                     fromHm <= now &&
                     toHm >= now) ||
                    (!s.IsDefault &&
                     s.From <= DateTimeOffset.Now &&
                     s.To >= DateTimeOffset.Now));
        }
    }

    public Task UpdateBrandGameProvidersAsync(string toMaintenanceIds, string toActiveIds)
    {
        const string spName = "select * from dbo.brand_game_providers_status_changes(@_to_maintenance_text,@_to_active_text)";

        var conn = _tgsContext.Database.GetDbConnection();

        return conn
            .QueryFirstOrDefaultAsync<object>(spName,
                new Dictionary<string, object>
                {
                    { "_to_maintenance_text", toMaintenanceIds },
                    { "_to_active_text", toActiveIds }
                });
    }
}