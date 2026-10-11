
namespace ST2_2026_Team_2_JobTracker.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ST2_2026_Team_2_JobTracker.Common.Constants;
    using ST2_2026_Team_2_JobTracker.Models.Entities;

    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(ValidationConstants.Company.NameMaxLength);

            builder.Property(c => c.Website)
                .HasMaxLength(ValidationConstants.Company.WebsiteMaxLength)
                .IsRequired(false);

            builder.Property(c => c.Location)
                .HasMaxLength(ValidationConstants.Company.LocationMaxLength)
                .IsRequired(false);

            builder.HasMany(c => c.JobOffers)
                .WithOne(j => j.Company)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(GetSeedData());
        }

        private static IEnumerable<Company> GetSeedData()
        {
            return new List<Company>
            {
                new Company
                {
                    Id = 1,
                    Name = "TechNova Solutions",
                    Website = "https://example.com",
                    Location = "Sofia, Bulgaria"
                },
                new Company
                {
                    Id = 2,
                    Name = "CodeBridge",
                    Website = "https://example.org",
                    Location = "Plovdiv, Bulgaria"
                },
                new Company
                {
                    Id = 3,
                    Name = "Digital Horizon",
                    Website = "https://example.net",
                    Location = "Remote"
                }
            };
        }
    }
}
