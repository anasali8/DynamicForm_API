using DynamicForm_API.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DynamicForm_API.Infrastructure.Data.Configuration
{
    public class LookupTableConfiguration : IEntityTypeConfiguration<LookupTable>
    {
        public void Configure(EntityTypeBuilder<LookupTable> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.HasMany(x => x.Items)
                .WithOne(x => x.LookupTable)
                .HasForeignKey(x => x.LookupTableId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
