using CacheManagement.Notifications.Events;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CacheManagement.Notifications.Caching;

public class PeopleCachingNotificationHandler(IMemoryCache cache, ILogger<PeopleCachingNotificationHandler> logger) : INotificationHandler<PersonCreated>,
    INotificationHandler<PersonUpdated>,
    INotificationHandler<PersonDeleted>
{
    public Task Handle(PersonCreated notification, CancellationToken cancellationToken)
    {
        logger.LogDebug("Cleaning cache...");
        cache.Remove("People");        

        return Task.CompletedTask;
    }

    public Task Handle(PersonUpdated notification, CancellationToken cancellationToken)
    {
        cache.Remove($"Person-{notification.Id}");
        cache.Remove("People");

        return Task.CompletedTask;
    }

    public Task Handle(PersonDeleted notification, CancellationToken cancellationToken)
    {
        cache.Remove($"Person-{notification.Id}");
        cache.Remove("People");

        return Task.CompletedTask;
    }
}
