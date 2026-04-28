using ShareFlow.Application.Revenue.DTOs;
using ShareFlow.Domain.Enums;

namespace ShareFlow.Application.Revenue.Interfaces;

public interface IPlatformRevenueHandler
{
    RevenueImportSource SupportedSource { get; }
    Task<IReadOnlyList<ParsedRevenueItem>> ParseAsync(RevenueParseRequest request, CancellationToken ct = default);
}

public interface IPlatformRevenueHandlerFactory
{
    IPlatformRevenueHandler GetHandler(string platformName, RevenueImportSource source);
}
