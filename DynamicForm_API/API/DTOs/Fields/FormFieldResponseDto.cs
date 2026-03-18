using DynamicForm_API.Shared.Enums;

namespace DynamicForm_API.API.DTOs.Fields
{
    public class FormFieldResponseDto
    {
        public int Id { get; set; }
        public int FormVersionId { get; set; }
        public FieldType Type { get; set; }
        public string Label { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public string? GroupKey { get; set; }
        public string? BindingKey { get; set; }
        public string? RegexPattern { get; set; }
        public string? ConfigJson { get; set; }
        public OptionsMode? OptionsMode { get; set; }
        public string? LookupKey { get; set; }
        public bool ActiveOnly { get; set; } = true;
        public IReadOnlyList<FormFieldOptionResponseDto> Options { get; set; } = Array.Empty<FormFieldOptionResponseDto>();
    }

    public class FormFieldOptionResponseDto
    {
        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Order { get; set; }
        public string? ParentValue { get; set; }
    }
}
