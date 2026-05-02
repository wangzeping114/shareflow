namespace ShareFlow.Application.Admin.DTOs;

public record AdminDashboardDto(
    AdminKpiDto Kpi,
    List<MonthlyTrendDto> RevenueByMonth,
    List<NameValueDto> RevenueByPlatform,
    List<NameValueDto> DividendByProject,
    List<NameValueDto> ContractStatusDist
);

public record AdminKpiDto(
    decimal TotalRevenue,
    decimal TotalDividends,
    int ActiveContracts,
    int PendingWithdrawals,
    int TotalClients,
    string Currency
);

/// <summary>月度趋势（收益 + 分红双轴）</summary>
public record MonthlyTrendDto(string Month, decimal Revenue, decimal Dividend);

/// <summary>通用 name/value 用于饼图/柱状图</summary>
public record NameValueDto(string Name, decimal Value);
