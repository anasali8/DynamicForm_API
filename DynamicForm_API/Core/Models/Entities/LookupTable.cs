
using DynamicForm_API.Core.Models.Shared;

namespace DynamicForm_API.Core.Models.Entities
{
    public class LookupTable : AuditTableEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsArchived { get; set; } = false;

        public ICollection<LookupItem> Items { get; set; } = new List<LookupItem>();
    }
}
