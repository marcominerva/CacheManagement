using MediatR;

namespace CacheManagement.Notifications.Events;

public record class CityUpdated(Guid Id) : INotification;
