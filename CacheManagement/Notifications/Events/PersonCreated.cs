using MediatR;

namespace CacheManagement.Notifications.Events;

public record class PersonCreated(Guid Id) : INotification;
