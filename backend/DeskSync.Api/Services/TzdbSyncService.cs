using Microsoft.EntityFrameworkCore;
using DeskSync.Api.Data;
using DeskSync.Api.Services.Interfaces;
using DeskSync.Api.Constants;
using NodaTime;

namespace DeskSync.Api.Services;

public class TzdbSyncService(
    AppDbContext dbContext,
    IDateTimeZoneProvider tzProvider,
    IClock clock,
    ILogger<TzdbSyncService> logger
): ITzdbSyncService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IDateTimeZoneProvider _tzProvider = tzProvider;
    private readonly IClock _clock = clock;
    private readonly ILogger<TzdbSyncService> _logger = logger;

    public async Task SyncReservationsAsync(CancellationToken cancellationToken = default)
    {
        var currentTzVersion = _tzProvider.VersionId;
        const int batchSize = WorkerConstant.DefaultBatchSize;
        var currentInstant = _clock.GetCurrentInstant();
        bool hasMoreRecords = true; 

        while(hasMoreRecords)
        {
            var outdatedReservations = await _dbContext.Reservations
                .Where(r=> r.TzdbVersion != currentTzVersion && r.UtcStartTime > currentInstant)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
            
            if (outdatedReservations.Count == 0)
            {
                hasMoreRecords = false;
                continue;
            }

            foreach (var reservation in outdatedReservations)
            {
                reservation.SyncWithTzdbVersion(currentTzVersion, _tzProvider);
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed batch of {Count} reservations for TZDB {Version}.", outdatedReservations.Count, currentTzVersion);
        }
    }
}
