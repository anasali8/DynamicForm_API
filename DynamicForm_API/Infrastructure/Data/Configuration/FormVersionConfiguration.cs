using DynamicForm_API.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DynamicForm_API.Infrastructure.Data.Configuration
{
    public class FormVersionConfiguration : IEntityTypeConfiguration<FormVersion>
    {
        public void Configure(EntityTypeBuilder<FormVersion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.VersionNumber)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.IsCurrent)
                .IsRequired();

            builder.HasIndex(x => new { x.FormId, x.VersionNumber })
                .IsUnique();

            builder.HasIndex(x => x.FormId)
                .IsUnique()
                .HasFilter("\"IsCurrent\" = true AND \"Status\" = 2");

            builder.HasIndex(x => new { x.FormId, x.Status, x.IsCurrent });

            builder.HasAlternateKey(x => new { x.Id, x.FormId });

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_FormVersion_PublishedAt_WhenPublished",
                "\"Status\" <> 2 OR \"PublishedAt\" IS NOT NULL"));

            builder.HasMany(x => x.Fields)
                .WithOne(x => x.FormVersion)
                .HasForeignKey(x => x.FormVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Submissions)
                .WithOne(x => x.FormVersion)
                .HasForeignKey(x => new { x.FormVersionId, x.FormId })
                .HasPrincipalKey(x => new { x.Id, x.FormId })
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
