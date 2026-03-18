using DynamicForm_API.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DynamicForm_API.Infrastructure.Data.Configuration
{
    public class LookupItemConfiguration : IEntityTypeConfiguration<LookupItem>
    {
        public void Configure(EntityTypeBuilder<LookupItem> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.ParentValue)
                .HasMaxLength(200);

            builder.HasIndex(x => new { x.LookupTableId, x.Value })
                .IsUnique();

            builder.HasIndex(x => new { x.LookupTableId, x.IsActive, x.Order });
        }
    }
}
