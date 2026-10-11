
namespace ST2_2026_Team_2_JobTracker.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ST2_2026_Team_2_JobTracker.Common.Constants;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    using ST2_2026_Team_2_JobTracker.Models.Enums;
    public class JobOfferConfiguration : IEntityTypeConfiguration<JobOffer>
    {
        public void Configure(EntityTypeBuilder<JobOffer> builder)
        {
            builder.HasKey(j => j.Id);

            builder.Property(j => j.Title)
                .IsRequired()
                .HasMaxLength(ValidationConstants.JobOffer.TitleMaxLength);

            builder.Property(j => j.Description)
                .IsRequired()
                .HasMaxLength(ValidationConstants.JobOffer.DescriptionMaxLength);

            builder.Property(j => j.Location)
                .HasMaxLength(ValidationConstants.JobOffer.LocationMaxLength)
                .IsRequired(false);

            builder.Property(j => j.Salary)
                .HasPrecision(
                    ValidationConstants.JobOffer.SalaryPrecision,
                    ValidationConstants.JobOffer.SalaryScale)
                .IsRequired(false);

            builder.Property(j => j.RemoteType)
                .IsRequired();

            builder.Property(j => j.CreatedAt)
                .IsRequired();

            builder.HasOne(j => j.Company)
                .WithMany(c => c.JobOffers)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(GetSeedData());
        }

        private static IEnumerable<JobOffer> GetSeedData()
        {
            return new List<JobOffer>
            {
                new JobOffer
                {
                    Id = 1,
                    CompanyId = 1,
                    Title = "Junior .NET Developer",
                    Description = "Develop ASP.NET Core applications using C#, SQL Server and Entity Framework Core.",
                    Location = "Sofia, Bulgaria",
                    Salary = 2500m,
                    RemoteType = RemoteType.Hybrid,
                    CreatedAt = new DateTime(
                        2026, 9, 20, 10, 0, 0, DateTimeKind.Utc)
                },
                new JobOffer
                {
                    Id = 2,
                    CompanyId = 2,
                    Title = "Junior Python Developer",
                    Description = "Build backend services using Python, Django, REST APIs and PostgreSQL.",
                    Location = "Plovdiv, Bulgaria",
                    Salary = 2300m,
                    RemoteType = RemoteType.OnSite,
                    CreatedAt = new DateTime(
                        2026, 9, 25, 9, 0, 0, DateTimeKind.Utc)
                },
                new JobOffer
                {
                    Id = 3,
                    CompanyId = 3,
                    Title = "Frontend React Developer",
                    Description = "Develop modern web interfaces using React, TypeScript, HTML and CSS.",
                    Location = "Remote",
                    Salary = 3200m,
                    RemoteType = RemoteType.Remote,
                    CreatedAt = new DateTime(
                        2026, 9, 28, 14, 0, 0, DateTimeKind.Utc)
                }
            };
        }
    }
}
