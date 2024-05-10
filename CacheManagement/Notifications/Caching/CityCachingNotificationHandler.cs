using CacheManagement.DataAccessLayer;
using CacheManagement.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CacheManagement.Notifications.Caching;

public class CityCachingNotificationHandler(IMemoryCache cache, ApplicationDbContext dbContext) : INotificationHandler<CityUpdated>
{
    public async Task Handle(CityUpdated notification, CancellationToken cancellationToken)
    {
        cache.Remove($"City-{notification.Id}");
        cache.Remove("Cities");

        var peopleIds = await dbContext.People.AsNoTracking()
            .Where(p => p.CityId == notification.Id)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var personId in peopleIds)
        {
            cache.Remove($"Person-{personId}");
        }

        cache.Remove("People");
    }
}
