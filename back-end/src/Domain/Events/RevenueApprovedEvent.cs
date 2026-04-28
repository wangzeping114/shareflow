using MediatR;

namespace ShareFlow.Domain.Events;

public record RevenueApprovedEvent(
    Guid RevenueId,
    Guid ProjectId,
    decimal Amount,
    string Currency,
    DateTime RevenueDate) : INotification;
