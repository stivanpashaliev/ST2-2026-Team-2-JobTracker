using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using Xunit;

namespace JobTracker.Tests;

// Every test uses a separate relational database; no development records are modified.
public sealed class JobTrackerFactory : WebApplicationFactory<ST2_2026_Team_2_JobTracker.Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:;Foreign Keys=True");

    public JobTrackerFactory() => connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
            new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = "Test database" }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        });
    }

    public HttpClient CreateSeededClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) connection.Dispose();
    }
}

public sealed class CrudTests : IDisposable
{
    private readonly JobTrackerFactory factory = new();
    private readonly HttpClient client;

    public CrudTests() => client = factory.CreateSeededClient();

    private async Task<HttpResponseMessage> Submit(string path, Dictionary<string, string>? values = null)
    {
        var page = await client.GetAsync(path);
        page.EnsureSuccessStatusCode();
        var html = await page.Content.ReadAsStringAsync();
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(token.Success, "The form must contain an antiforgery token.");
        values ??= new();
        values["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value);
        return await client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private async Task<T> Query<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = factory.Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    [Theory]
    [InlineData("Companies")]
    [InlineData("JobOffers")]
    [InlineData("Applications")]
    public async Task Lists_show_edit_and_delete_and_missing_records_return_404(string entity)
    {
        var html = await client.GetStringAsync($"/{entity}");
        Assert.Contains($"/{entity}/Edit/1", html);
        Assert.Contains($"/{entity}/Delete/1", html);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/{entity}/Edit/9999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/{entity}/Delete/9999")).StatusCode);
    }

    [Fact]
    public async Task Company_edit_persists_and_shows_success()
    {
        var response = await Submit("/Companies/Edit/1", new()
        {
            ["Id"] = "1", ["Name"] = " Updated Company ", ["Website"] = "https://example.com", ["Location"] = "Varna"
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var company = await Query(db => db.Companies.SingleAsync(c => c.Id == 1));
        Assert.Equal("Updated Company", company.Name);
        Assert.Equal("Varna", company.Location);
        Assert.Contains("Company updated successfully.", await client.GetStringAsync(response.Headers.Location));
    }

    private static Dictionary<string, string> JobOfferForm(string companyId = "2") => new()
    {
        ["Id"] = "1", ["CompanyId"] = companyId, ["Title"] = "Updated developer position",
        ["Description"] = "An updated description for this developer position.",
        ["Location"] = "Varna", ["Salary"] = "4200", ["RemoteType"] = "Remote"
    };

    [Fact]
    public async Task Job_offer_edit_preserves_creation_date_and_linked_application()
    {
        var before = await Query(db => db.JobOffers.SingleAsync(j => j.Id == 1));
        var response = await Submit("/JobOffers/Edit/1", JobOfferForm());
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var after = await Query(db => db.JobOffers.Include(j => j.Application).SingleAsync(j => j.Id == 1));
        Assert.Equal("Updated developer position", after.Title);
        Assert.Equal(2, after.CompanyId);
        Assert.Equal(4200m, after.Salary);
        Assert.Equal(RemoteType.Remote, after.RemoteType);
        Assert.Equal(before.CreatedAt, after.CreatedAt);
        Assert.Equal(1, after.Application!.Id);
    }

    [Fact]
    public async Task Application_edit_allows_current_offer_and_status_changes()
    {
        var response = await Submit("/Applications/Edit/3", new()
        {
            ["Id"] = "3", ["JobOfferId"] = "3", ["Status"] = "Applied", ["Notes"] = " Updated notes "
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var application = await Query(db => db.Applications.SingleAsync(a => a.Id == 3));
        Assert.Equal(ApplicationStatus.Applied, application.Status);
        Assert.NotNull(application.AppliedAt);
        Assert.Equal("Updated notes", application.Notes);

        response = await Submit("/Applications/Edit/3", new()
        {
            ["Id"] = "3", ["JobOfferId"] = "3", ["Status"] = "Saved", ["AppliedAt"] = ""
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Null(await Query(db => db.Applications.Where(a => a.Id == 3).Select(a => a.AppliedAt).SingleAsync()));
    }

    [Fact]
    public async Task Application_can_move_to_an_unoccupied_offer()
    {
        Assert.Equal(HttpStatusCode.Redirect, (await Submit("/Applications/Delete/2")).StatusCode);
        var response = await Submit("/Applications/Edit/1", new()
        {
            ["Id"] = "1", ["JobOfferId"] = "2", ["Status"] = "Interview"
        });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var application = await Query(db => db.Applications.SingleAsync(a => a.Id == 1));
        Assert.Equal(2, application.JobOfferId);
        Assert.Equal(ApplicationStatus.Interview, application.Status);
        Assert.Equal(new DateTime(2026, 10, 1, 9, 0, 0), application.AppliedAt);
    }

    [Theory]
    [InlineData("2", "Applied", "", "already has an application")]
    [InlineData("9999", "Applied", "", "does not exist")]
    [InlineData("1", "Saved", "2026-10-01", "cannot have an applied date")]
    [InlineData("1", "999", "", "The value")]
    public async Task Invalid_application_edits_do_not_change_records(string offerId, string status, string appliedAt, string error)
    {
        var response = await Submit("/Applications/Edit/1", new()
        {
            ["Id"] = "1", ["JobOfferId"] = offerId, ["Status"] = status, ["AppliedAt"] = appliedAt
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(error, await response.Content.ReadAsStringAsync());
        var application = await Query(db => db.Applications.SingleAsync(a => a.Id == 1));
        Assert.Equal(1, application.JobOfferId);
        Assert.Equal(ApplicationStatus.Applied, application.Status);
    }

    [Fact]
    public async Task Invalid_company_and_job_offer_edits_are_rejected()
    {
        var company = await Submit("/Companies/Edit/1", new() { ["Id"] = "1", ["Name"] = "" });
        Assert.Contains("Company name is required", await company.Content.ReadAsStringAsync());
        var offer = await Submit("/JobOffers/Edit/1", JobOfferForm("9999"));
        Assert.Contains("selected company does not exist", await offer.Content.ReadAsStringAsync());
        Assert.Equal(1, await Query(db => db.JobOffers.Where(j => j.Id == 1).Select(j => j.CompanyId).SingleAsync()));
        var mismatch = await Submit("/Companies/Edit/1", new() { ["Id"] = "2", ["Name"] = "Changed name" });
        Assert.Equal(HttpStatusCode.BadRequest, mismatch.StatusCode);
    }

    [Fact]
    public async Task Delete_requires_confirmation_and_dependency_order()
    {
        var companyPage = await client.GetStringAsync("/Companies/Delete/1");
        Assert.Contains("job offers before deleting", companyPage);
        Assert.DoesNotContain("<form", companyPage);
        var offerPage = await client.GetStringAsync("/JobOffers/Delete/1");
        Assert.Contains("application before deleting", offerPage);
        Assert.True(await Query(db => db.Companies.AnyAsync(c => c.Id == 1)));

        // A crafted POST must enforce the same restrictions as the confirmation page.
        var tokenPage = await client.GetStringAsync("/Applications/Delete/1");
        var token = Regex.Match(tokenPage, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        foreach (var entity in new[] { "Companies", "JobOffers" })
        {
            var blocked = await client.PostAsync($"/{entity}/Delete/1", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token)
            }));
            Assert.Equal(HttpStatusCode.OK, blocked.StatusCode);
            Assert.Contains("before deleting", await blocked.Content.ReadAsStringAsync());
        }

        foreach (var entity in new[] { "Applications", "JobOffers", "Companies" })
        {
            var response = await Submit($"/{entity}/Delete/1");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/{entity}/Delete/1")).StatusCode);
        }
        Assert.True(await Query(db => db.Applications.AnyAsync(a => a.Id == 2)));
    }

    [Theory]
    [InlineData("Companies", "Edit")]
    [InlineData("Companies", "Delete")]
    [InlineData("JobOffers", "Edit")]
    [InlineData("JobOffers", "Delete")]
    [InlineData("Applications", "Edit")]
    [InlineData("Applications", "Delete")]
    public async Task Mutations_require_antiforgery_token(string entity, string action)
    {
        var response = await client.PostAsync($"/{entity}/{action}/1", new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        client.Dispose();
        factory.Dispose();
    }
}
