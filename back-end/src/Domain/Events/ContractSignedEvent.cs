using MediatR;

namespace ShareFlow.Domain.Events;

public record ContractSignedEvent(
    Guid ContractId,
    Guid InvestorUserId,
    Guid ProjectId,
    Guid SlotId) : INotification;
