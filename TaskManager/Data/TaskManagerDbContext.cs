using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Data
{
    public class TaskManagerDbContext : DbContext
    {
        public DbSet<KanbanTask> Tasks { get; set; }

        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KanbanTask>(entity =>
            {
                entity.Property(t => t.Title).IsRequired();
                entity.Property(t => t.Description).IsRequired(false);
            });
        }
    }
} 