namespace TeacherSuite.Application.Common.Models;

public static class PaginationDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 12;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;

    public static (int page, int pageSize) Normalize(int? page, int? pageSize)
    {
        var normalizedPage = Math.Max(DefaultPage, page ?? DefaultPage);
        var normalizedPageSize = Math.Clamp(pageSize ?? DefaultPageSize, MinPageSize, MaxPageSize);
        return (normalizedPage, normalizedPageSize);
    }
}
