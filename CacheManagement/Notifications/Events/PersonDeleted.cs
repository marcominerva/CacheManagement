using MediatR;

namespace CacheManagement.Notifications.Events;

public record class PersonDeleted(Guid Id) : INotification;
