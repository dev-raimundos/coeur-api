namespace CoeurApi.SharedKernel.Common;

public static class Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize) => (
        Math.Max(page ?? 1, 1),
        Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize)
    );
}
