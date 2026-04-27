namespace ShareFlow.Domain.Common;

/// <summary>
/// 分页查询基类。所有带分页的 Query/Request 均继承此类。
/// 自动对 Page（最小 1）和 PageSize（1~200，默认 20）做边界校验。
/// </summary>
public abstract record PagedQuery
{
    private readonly int _page = 1;
    private readonly int _pageSize = 20;

    public int Page
    {
        get => _page;
        init => _page = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Clamp(value, 1, 200);
    }
}
