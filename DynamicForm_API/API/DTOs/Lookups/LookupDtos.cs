namespace DynamicForm_API.API.DTOs.Lookups
{
    public class CreateLookupTableRequestDto
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateLookupTableRequestDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class ArchiveLookupTableRequestDto
    {
        public string? Reason { get; set; }
    }

    public class CreateLookupItemRequestDto
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Order { get; set; }
        public string? ParentValue { get; set; }
    }

    public class UpdateLookupItemRequestDto
    {
        public string Label { get; set; } = null!;
        public int Order { get; set; }
        public string? ParentValue { get; set; }
    }

    public class SetLookupItemActiveStateRequestDto
    {
        public bool IsActive { get; set; }
    }

    public class ReorderLookupItemsRequestDto
    {
        public IReadOnlyList<ReorderLookupItemDto> Items { get; set; } = Array.Empty<ReorderLookupItemDto>();
    }

    public class ReorderLookupItemDto
    {
        public int LookupItemId { get; set; }
        public int Order { get; set; }
    }

    public class LookupTableSummaryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
    }

    public class LookupTableDetailResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
        public IReadOnlyList<LookupItemResponseDto> Items { get; set; } = Array.Empty<LookupItemResponseDto>();
    }

    public class LookupItemResponseDto
    {
        public int Id { get; set; }
        public int LookupTableId { get; set; }
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public string? ParentValue { get; set; }
    }

    public class LookupOptionsResponseDto
    {
        public string LookupCode { get; set; } = null!;
        public IReadOnlyList<LookupOptionItemResponseDto> Items { get; set; } = Array.Empty<LookupOptionItemResponseDto>();
    }

    public class LookupOptionItemResponseDto
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Order { get; set; }
        public string? ParentValue { get; set; }
    }
}
