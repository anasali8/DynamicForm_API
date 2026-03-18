using DynamicForm_API.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DynamicForm_API.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Form> Forms => Set<Form>();
        public DbSet<FormVersion> FormVersions => Set<FormVersion>();
        public DbSet<FormField> FormFields => Set<FormField>();
        public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
        public DbSet<LookupTable> LookupTables => Set<LookupTable>();
        public DbSet<LookupItem> LookupItems => Set<LookupItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
