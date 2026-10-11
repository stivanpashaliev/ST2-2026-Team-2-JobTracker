
namespace ST2_2026_Team_2_JobTracker.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ST2_2026_Team_2_JobTracker.Common.Constants;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    using ST2_2026_Team_2_JobTracker.Models.Enums;

    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.AppliedAt)
                .IsRequired(false);

            builder.Property(a => a.Notes)
                .HasMaxLength(ValidationConstants.Application.NotesMaxLength)
                .IsRequired(false);

            builder.HasOne(a => a.JobOffer)
                .WithOne(j => j.Application)
                .HasForeignKey<Application>(a => a.JobOfferId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(GetSeedData());
        }

        private static IEnumerable<Application> GetSeedData()
        {
            return new List<Application>
            {
                new Application
                {
                    Id = 1,
                    JobOfferId = 1,
                    Status = ApplicationStatus.Applied,
                    AppliedAt = new DateTime(
                        2026, 10, 1, 9, 0, 0, DateTimeKind.Utc),
                    Notes = "Applied through the company website."
                },
                new Application
                {
                    Id = 2,
                    JobOfferId = 2,
                    Status = ApplicationStatus.Interview,
                    AppliedAt = new DateTime(
                        2026, 10, 3, 11, 30, 0, DateTimeKind.Utc),
                    Notes = "Technical interview scheduled."
                },
                new Application
                {
                    Id = 3,
                    JobOfferId = 3,
                    Status = ApplicationStatus.Saved,
                    AppliedAt = null,
                    Notes = "Review the requirements before applying."
                }
            };
        }
    }
}
