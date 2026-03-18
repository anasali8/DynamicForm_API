using DynamicForm_API.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DynamicForm_API.Infrastructure.Data.Configuration
{
    public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
    {
        public void Configure(EntityTypeBuilder<FormSubmission> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.SubmissionData)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(x => x.SubmittedAt)
                .IsRequired();

            builder.HasIndex(x => new { x.FormId, x.FormVersionId, x.SubmittedAt });

            builder.HasOne(x => x.Form)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FormVersion)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => new { x.FormVersionId, x.FormId })
                .HasPrincipalKey(x => new { x.Id, x.FormId })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
