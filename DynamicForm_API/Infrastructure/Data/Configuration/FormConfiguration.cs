using DynamicForm_API.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DynamicForm_API.Infrastructure.Data.Configuration
{
    public class FormConfiguration : IEntityTypeConfiguration<Form>
    {
        public void Configure(EntityTypeBuilder<Form> builder)
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

            builder.HasMany(x => x.Versions)
                .WithOne(x => x.Form)
                .HasForeignKey(x => x.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Submissions)
                .WithOne(x => x.Form)
                .HasForeignKey(x => x.FormId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
