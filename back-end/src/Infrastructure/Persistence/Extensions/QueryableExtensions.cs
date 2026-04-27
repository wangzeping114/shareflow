using Microsoft.EntityFrameworkCore;

namespace ShareFlow.Infrastructure.Persistence.Extensions;

/// <summary>
/// IQueryable 分页扩展。所有 Repository 的 GetPagedAsync 统一调用此方法，
/// 消除重复的 CountAsync + Skip + Take 样板代码。
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 对已构建完过滤/排序的 IQueryable 执行分页查询。
    /// 先 Count（触发一次 SQL COUNT），若总数为 0 则直接返回空列表，
    /// 否则执行 Skip/Take 取当页数据。
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="query">已应用过滤与排序的 IQueryable</param>
    /// <param name="page">页码（从 1 开始）</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>(当页数据列表, 总条数)</returns>
    public static async Task<(IReadOnlyList<T> Items, int Total)> ToPagedAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);
        if (total == 0) return ([], 0);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
