using MediatR;

namespace Tgs.Brand.GameProvider.Domain.Events;

public record ClearBrandGameProviderEvent(string[] Keys) : INotification;