using MediatR;

namespace CacheManagement.Notifications.Events;

public record class PersonUpdated(Guid Id) : INotification;