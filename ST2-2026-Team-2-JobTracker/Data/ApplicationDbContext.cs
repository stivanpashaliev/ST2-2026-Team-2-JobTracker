namespace ST2_2026_Team_2_JobTracker.Data
{
    using Microsoft.EntityFrameworkCore;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<JobOffer> JobOffers { get; set; } = null!;
        public DbSet<Application> Applications { get; set; } = null!;

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
