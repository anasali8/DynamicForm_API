using DynamicForm_API.Core.Models.Shared;

namespace DynamicForm_API.Core.Models.Entities
{
    public class LookupItem : AuditTableEntity
    {
        public int LookupTableId { get; set; }

        public string Value { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ParentValue { get; set; }

        public LookupTable LookupTable { get; set; } = null!;
    }
}
