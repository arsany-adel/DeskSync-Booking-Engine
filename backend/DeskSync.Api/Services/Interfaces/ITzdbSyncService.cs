using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeskSync.Api.Services.Interfaces;

public interface ITzdbSyncService
{
    Task SyncReservationsAsync(CancellationToken cancellationToken = default);
}
