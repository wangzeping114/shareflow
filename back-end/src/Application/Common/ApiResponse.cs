namespace ShareFlow.Application.Common;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string TraceId { get; set; } = string.Empty;

    public static ApiResponse<T> Success(T data, string message = "Success") =>
        new() { Code = 200, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, int code = 400) =>
        new() { Code = code, Message = message };
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    /// <summary>
    /// 从仓储返回的 (Items, Total) 元组和 Mapster 映射函数构造分页结果。
    /// 消除 Service 层手动赋值四个字段的样板代码。
    /// </summary>
    public static PagedResult<TDto> MapFrom<TEntity, TDto>(
        (IReadOnlyList<TEntity> Items, int Total) paged,
        int page,
        int pageSize,
        Func<IReadOnlyList<TEntity>, IReadOnlyList<TDto>> mapItems)
        => new()
        {
            Items = mapItems(paged.Items),
            Total = paged.Total,
            Page = page,
            PageSize = pageSize,
        };
}
