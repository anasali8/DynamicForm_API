using DynamicForm_API.Core.Models.Shared;
using DynamicForm_API.Shared.Enums;

namespace DynamicForm_API.Core.Models.Entities
{
    public class FormField : AuditTableEntity
    {
        public int FormVersionId { get; set; }

        public FieldType Type { get; set; }
        public string Label { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; } = false;
        public int Order { get; set; }

        public string? GroupKey { get; set; }
        public string? BindingKey { get; set; }

        public string? RegexPattern { get; set; }
        public string? ConfigJson { get; set; }

        public FormVersion FormVersion { get; set; } = null!;
    }
}
