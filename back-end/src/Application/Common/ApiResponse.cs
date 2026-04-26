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
}
