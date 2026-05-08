using Microsoft.EntityFrameworkCore;
using ShareFlow.Application.Admin.DTOs;
using ShareFlow.Application.Admin.Interfaces;
using ShareFlow.Domain.Enums;
using ShareFlow.Domain.Interfaces;
using ShareFlow.Infrastructure.Persistence;

namespace ShareFlow.Infrastructure.Services;

public class AdminDashboardService(
    AppDbContext db,
    IRegionContext regionContext) : IAdminDashboardService
{
    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var currency = regionContext.DefaultCurrency;

        // ── KPI ────────────────────────────────────────────────────────
        var totalRevenue = await db.PlatformRevenues
            .Where(r => r.Status == RevenueStatus.Approved)
            .SumAsync(r => r.Amount, ct);

        var totalDividend = await db.DividendRecords
            .Where(d => d.Status == DividendStatus.Distributed)
            .SumAsync(d => d.DividendAmount, ct);

        var activeContracts = await db.Contracts
            .CountAsync(c => c.Status == ContractStatus.Executed || c.Status == ContractStatus.Signed, ct);

        var pendingWithdrawals = await db.WithdrawalRequests
            .CountAsync(w => w.Status == WithdrawalStatus.Pending, ct);

        var totalClients = await db.Users
            .CountAsync(u => u.Role == UserRole.Client, ct);

        var kpi = new AdminKpiDto(totalRevenue, totalDividend, activeContracts, pendingWithdrawals, totalClients, currency);

        // ── 近 6 个月收益 + 分红趋势 ────────────────────────────────────
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var firstDay = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var revenueByMonth = await db.PlatformRevenues
            .Where(r => r.Status == RevenueStatus.Approved && r.RevenueDate >= firstDay)
            .GroupBy(r => new { r.RevenueDate.Year, r.RevenueDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(r => r.Amount) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        var dividendByMonth = await db.DividendRecords
            .Where(d => d.Status == DividendStatus.Distributed && d.CalculatedAt >= firstDay)
            .GroupBy(d => new { d.CalculatedAt.Year, d.CalculatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(d => d.DividendAmount) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        // 补全 6 个月（确保每月都有数据点）
        var months = new List<MonthlyTrendDto>();
        for (var i = 0; i < 6; i++)
        {
            var d = firstDay.AddMonths(i);
            var label = d.ToString("yyyy-MM");
            var rev = revenueByMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Amount ?? 0;
            var div = dividendByMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Amount ?? 0;
            months.Add(new MonthlyTrendDto(label, rev, div));
        }

        // ── 收益按平台 ──────────────────────────────────────────────────
        var revenueByPlatform = await db.PlatformRevenues
            .Where(r => r.Status == RevenueStatus.Approved)
            .GroupBy(r => r.PlatformName)
            .Select(g => new { Name = g.Key, Value = g.Sum(r => r.Amount) })
            .OrderByDescending(x => x.Value)
            .ToListAsync(ct);

        var revenueByPlatformDtos = revenueByPlatform
            .Select(x => new NameValueDto(x.Name, x.Value))
            .ToList();

        // ── 各项目分红 Top 10 ────────────────────────────────────────────
        var dividendByProject = await db.DividendRecords
            .Where(d => d.Status == DividendStatus.Distributed)
            .Join(db.VideoProjects, d => d.ProjectId, p => p.Id, (d, p) => new { p.Title, d.DividendAmount })
            .GroupBy(x => x.Title)
            .Select(g => new { Name = g.Key, Value = g.Sum(x => x.DividendAmount) })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);

        var dividendByProjectDtos = dividendByProject
            .Select(x => new NameValueDto(x.Name, x.Value))
            .ToList();

        // ── 合同状态分布 ─────────────────────────────────────────────────
        var contractStatusDist = await db.Contracts
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(ct);

        var contractDtos = contractStatusDist
            .Select(x => new NameValueDto(x.Status, x.Count))
            .ToList();

        return new AdminDashboardDto(kpi, months, revenueByPlatformDtos, dividendByProjectDtos, contractDtos);
    }
}
