using Microsoft.EntityFrameworkCore;
using StudyHub.Models;

namespace StudyHub.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Assignment> Assignments { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent duplicate usernames
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            // Prevent duplicate emails
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // Prevent duplicate course codes
            modelBuilder.Entity<Course>().HasIndex(c => new { c.UserId, c.Code }).IsUnique();

            // Prevent duplicate enrollments
            modelBuilder
                .Entity<Enrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique();
        }
    }
}
