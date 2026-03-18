using DynamicForm_API.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DynamicForm_API.Infrastructure.Data.Configuration
{
    public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
    {
        public void Configure(EntityTypeBuilder<FormField> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.GroupKey)
                .HasMaxLength(100);

            builder.Property(x => x.BindingKey)
                .HasMaxLength(100);

            builder.Property(x => x.RegexPattern)
                .HasMaxLength(500);

            builder.Property(x => x.ConfigJson)
                .HasColumnType("jsonb");

            builder.HasIndex(x => new { x.FormVersionId, x.Name })
                .IsUnique();

            builder.HasIndex(x => new { x.FormVersionId, x.Order })
                .IsUnique();
        }
    }
}
