using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Entities;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;
using Xunit;
using JobApplication = ST2_2026_Team_2_JobTracker.Models.Entities.Application;

namespace JobTracker.Tests;

/// <summary>
/// Integration tests use the project's existing SQLite in-memory WebApplicationFactory.
/// No SQL Server installation or development database is touched.
/// </summary>
public sealed class DashboardTests : IDisposable
{
    private readonly JobTrackerFactory _factory = new();
    private readonly HttpClient _client;

    public DashboardTests() => _client = _factory.CreateSeededClient();

    [Fact]
    public async Task Dashboard_renders_and_is_linked_from_navigation()
    {
        using var response = await _client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Your dashboard", html);
        Assert.Contains("Application statuses", html);
        Assert.Contains("Latest applications", html);
        Assert.Contains("/Applications/Edit/3", html);

        var home = await _client.GetStringAsync("/");
        Assert.Contains("/Dashboard", home);
        Assert.DoesNotContain("/AiMatch", home);
    }

    [Fact]
    public async Task Dashboard_service_calculates_seeded_counts_and_zero_statuses()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IDashboardService>();
        var dashboard = await service.GetDashboardAsync();

        Assert.Equal(3, dashboard.TotalCompanies);
        Assert.Equal(3, dashboard.TotalJobOffers);
        Assert.Equal(3, dashboard.TotalApplications);
        Assert.Equal(0, dashboard.AvailableJobOffers);
        Assert.Equal(5, dashboard.ApplicationStatuses.Count);
        Assert.Equal(1, dashboard.ApplicationStatuses.Single(s => s.Status == ApplicationStatus.Saved).Count);
        Assert.Equal(1, dashboard.ApplicationStatuses.Single(s => s.Status == ApplicationStatus.Applied).Count);
        Assert.Equal(1, dashboard.ApplicationStatuses.Single(s => s.Status == ApplicationStatus.Interview).Count);
        Assert.Equal(0, dashboard.ApplicationStatuses.Single(s => s.Status == ApplicationStatus.Offer).Count);
        Assert.Equal(0, dashboard.ApplicationStatuses.Single(s => s.Status == ApplicationStatus.Rejected).Count);
        Assert.Equal(new[] { 3, 2, 1 }, dashboard.RecentApplications.Select(a => a.Id).ToArray());
        Assert.Equal("TechNova Solutions", dashboard.RecentApplications.Single(a => a.Id == 1).CompanyName);
    }

    [Fact]
    public async Task Dashboard_updates_after_an_application_is_removed()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Applications.Remove(await db.Applications.SingleAsync(a => a.Id == 3));
            await db.SaveChangesAsync();
        }

        using var verificationScope = _factory.Services.CreateScope();
        var dashboard = await verificationScope.ServiceProvider
            .GetRequiredService<IDashboardService>().GetDashboardAsync();
        Assert.Equal(2, dashboard.TotalApplications);
        Assert.Equal(1, dashboard.AvailableJobOffers);
        Assert.Equal(0, dashboard.ApplicationStatuses.Single(s => s.Status == ApplicationStatus.Saved).Count);
    }

    [Fact]
    public async Task Dashboard_handles_an_empty_database_without_dividing_by_zero()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Applications.RemoveRange(db.Applications);
            await db.SaveChangesAsync();
            db.JobOffers.RemoveRange(db.JobOffers);
            await db.SaveChangesAsync();
            db.Companies.RemoveRange(db.Companies);
            await db.SaveChangesAsync();
        }

        using var scope2 = _factory.Services.CreateScope();
        var dashboard = await scope2.ServiceProvider
            .GetRequiredService<IDashboardService>().GetDashboardAsync();
        Assert.Equal(0, dashboard.TotalCompanies);
        Assert.Equal(0, dashboard.TotalJobOffers);
        Assert.Equal(0, dashboard.TotalApplications);
        Assert.Equal(0, dashboard.AvailableJobOffers);
        Assert.All(dashboard.ApplicationStatuses, status => Assert.Equal(0, status.Count));
        Assert.Empty(dashboard.RecentApplications);

        var page = await _client.GetStringAsync("/Dashboard");
        Assert.Contains("No applications yet", page);
        Assert.Contains("Add your first application", page);
    }

    [Fact]
    public async Task Dashboard_returns_only_five_latest_added_applications()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var offers = Enumerable.Range(1, 4).Select(i => new JobOffer
            {
                CompanyId = 1,
                Title = $"Test role {i}",
                Description = "Dashboard test position description",
                RemoteType = RemoteType.Remote
            }).ToList();
            db.JobOffers.AddRange(offers);
            await db.SaveChangesAsync();

            db.Applications.AddRange(offers.Select(offer => new JobApplication
            {
                JobOfferId = offer.Id,
                Status = ApplicationStatus.Saved
            }));
            await db.SaveChangesAsync();
        }

        using var verificationScope = _factory.Services.CreateScope();
        var dashboard = await verificationScope.ServiceProvider
            .GetRequiredService<IDashboardService>().GetDashboardAsync();
        Assert.Equal(7, dashboard.TotalApplications);
        Assert.Equal(5, dashboard.RecentApplications.Count);
        Assert.Equal(new[] { 7, 6, 5, 4, 3 },
            dashboard.RecentApplications.Select(a => a.Id).ToArray());
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
