namespace DynamicForm_API.API.DTOs.Common
{
    public class PagedRequestDto
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 50;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = DefaultPageSize;
    }

    public class PagedResponseDto<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
