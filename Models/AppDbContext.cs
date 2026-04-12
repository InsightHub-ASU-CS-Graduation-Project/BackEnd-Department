using InsightHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsightHub.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<UserJob> UserJobs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // many-to-many relation between ApplicationUser and Job
            builder.Entity<UserJob>()
                .HasKey(uj => new { uj.UserId, uj.JobId });

            builder.Entity<UserJob>()
                .HasOne(uj => uj.User)
                .WithMany(u => u.UserJobs)
                .HasForeignKey(uj => uj.UserId);

            builder.Entity<UserJob>()
                .HasOne(uj => uj.Job)
                .WithMany(j => j.UserJobs)
                .HasForeignKey(uj => uj.JobId);
        }
    }
}