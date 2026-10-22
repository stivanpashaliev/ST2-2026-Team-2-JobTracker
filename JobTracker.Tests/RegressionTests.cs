using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ST2_2026_Team_2_JobTracker.Data;
using ST2_2026_Team_2_JobTracker.Models.Enums;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;
using Xunit;

namespace JobTracker.Tests;

// Regression tests for the targeted CRUD, validation, query, and date fixes.
// JobTrackerFactory is defined in CrudTests.cs and uses isolated SQLite in-memory.
public sealed class RegressionTests : IDisposable
{
    private readonly JobTrackerFactory factory = new();
    private readonly HttpClient client;

    public RegressionTests() => client = factory.CreateSeededClient();

    private async Task<HttpResponseMessage> Submit(string path, Dictionary<string, string> values)
    {
        var html = await client.GetStringAsync(path);
        var token = Regex.Match(html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(token.Success, "Antiforgery token is missing from the form.");
        values["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value);
        return await client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private async Task<T> Query<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await query(db);
    }

    [Fact]
    public async Task Application_create_rejects_an_already_occupied_offer_without_crashing()
    {
        var response = await Submit("/Applications/Create", new()
        {
            ["JobOfferId"] = "1", ["Status"] = "Applied"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("already has an application", await response.Content.ReadAsStringAsync());
        Assert.Equal(3, await Query(db => db.Applications.CountAsync()));
    }

    [Fact]
    public async Task Application_create_works_with_an_available_offer_and_trims_notes()
    {
        Assert.Equal(HttpStatusCode.Redirect,
            (await Submit("/Applications/Delete/2", new())).StatusCode);
        var response = await Submit("/Applications/Create", new()
        {
            ["JobOfferId"] = "2", ["Status"] = "Saved", ["Notes"] = "  To review  "
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var item = await Query(db => db.Applications.SingleAsync(a => a.JobOfferId == 2));
        Assert.Equal(ApplicationStatus.Saved, item.Status);
        Assert.Null(item.AppliedAt);
        Assert.Equal("To review", item.Notes);
    }

    [Fact]
    public async Task Available_offers_exclude_occupied_ones_but_allow_the_current_edit_offer()
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IJobOfferService>();
        Assert.Empty(await service.GetAvailableForApplicationAsync());
        Assert.Equal(new[] { 1 }, (await service.GetAvailableForApplicationAsync(1))
            .Select(x => x.Id).ToArray());

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Applications.Remove(await db.Applications.SingleAsync(a => a.JobOfferId == 2));
        await db.SaveChangesAsync();
        Assert.Equal(new[] { 2 }, (await service.GetAvailableForApplicationAsync())
            .Select(x => x.Id).ToArray());
        Assert.Equal(new[] { 1, 2 }, (await service.GetAvailableForApplicationAsync(1))
            .Select(x => x.Id).OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task Application_edit_with_the_same_date_preserves_the_original_time()
    {
        var response = await Submit("/Applications/Edit/1", new()
        {
            ["Id"] = "1", ["JobOfferId"] = "1", ["Status"] = "Applied",
            ["AppliedAt"] = "2026-10-01", ["Notes"] = "Changed the notes"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var appliedAt = await Query(db => db.Applications.Where(a => a.Id == 1)
            .Select(a => a.AppliedAt).SingleAsync());
        Assert.Equal(new DateTime(2026, 10, 1, 9, 0, 0), appliedAt);
    }

    [Fact]
    public async Task Application_edit_with_a_changed_date_uses_the_new_calendar_date()
    {
        var response = await Submit("/Applications/Edit/1", new()
        {
            ["Id"] = "1", ["JobOfferId"] = "1", ["Status"] = "Interview",
            ["AppliedAt"] = "2026-10-08"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var appliedAt = await Query(db => db.Applications.Where(a => a.Id == 1)
            .Select(a => a.AppliedAt).SingleAsync());
        Assert.Equal(new DateTime(2026, 10, 8), appliedAt);
    }

    [Fact]
    public async Task Company_name_length_is_validated_after_whitespace_is_removed()
    {
        var response = await Submit("/Companies/Create", new()
        {
            ["Name"] = "  A  "
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Company name must be between", await response.Content.ReadAsStringAsync());
        Assert.Equal(3, await Query(db => db.Companies.CountAsync()));
    }

    [Fact]
    public async Task Company_url_is_validated_and_saved_in_normalized_form()
    {
        var response = await Submit("/Companies/Create", new()
        {
            ["Name"] = "  New Team  ", ["Website"] = "  https://example.com  ",
            ["Location"] = "   "
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var company = await Query(db => db.Companies.SingleAsync(c => c.Name == "New Team"));
        Assert.Equal("https://example.com", company.Website);
        Assert.Null(company.Location);
    }

    [Fact]
    public async Task Job_offer_title_length_is_checked_after_trimming()
    {
        var response = await Submit("/JobOffers/Create", new()
        {
            ["CompanyId"] = "1", ["Title"] = "  AB  ",
            ["Description"] = "A sufficiently long job description.", ["RemoteType"] = "Remote"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Job title must be between", await response.Content.ReadAsStringAsync());
        Assert.Equal(3, await Query(db => db.JobOffers.CountAsync()));
    }

    [Theory]
    [InlineData("?minSalary=-1", "Minimum salary cannot be negative")]
    [InlineData("?maxSalary=-1", "Maximum salary cannot be negative")]
    [InlineData("?minSalary=5000&maxSalary=100", "Maximum salary must be greater")]
    [InlineData("?minSalary=not-a-number", "Correct the filters above")]
    public async Task Invalid_salary_filters_return_a_useful_validation_message(
        string queryString, string expectedMessage)
    {
        var response = await client.GetAsync("/JobOffers" + queryString);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedMessage, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/JobOffers?remoteType=999", "Please select a valid work type")]
    [InlineData("/JobOffers?companyId=-1", "Please select a valid company")]
    [InlineData("/Applications?status=999", "Please select a valid status")]
    [InlineData("/Applications?companyId=-1", "Please select a valid company")]
    public async Task Invalid_enum_and_company_filters_are_rejected(string path, string expected)
    {
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expected, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("Companies")]
    [InlineData("JobOffers")]
    [InlineData("Applications")]
    public async Task Extremely_large_page_numbers_are_clamped_without_an_overflow(string entity)
    {
        var response = await client.GetAsync($"/{entity}?page=2147483647");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Total ", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Navigation_does_not_link_to_unimplemented_controllers()
    {
        var html = await client.GetStringAsync("/");
        Assert.DoesNotContain("href=\"/Dashboard", html);
        Assert.DoesNotContain("href=\"/AiMatch", html);
    }

    public void Dispose()
    {
        client.Dispose();
        factory.Dispose();
    }
}
