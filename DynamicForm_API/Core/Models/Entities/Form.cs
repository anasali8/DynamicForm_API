using DynamicForm_API.Core.Models.Shared;
using System.Globalization;

namespace DynamicForm_API.Core.Models.Entities
{
    public class Form : AuditTableEntity
    {
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<FormVersion> Versions { get; set; } = new List<FormVersion>();
        public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
    }
}
