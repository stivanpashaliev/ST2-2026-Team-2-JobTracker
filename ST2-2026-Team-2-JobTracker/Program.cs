namespace ST2_2026_Team_2_JobTracker
{
    using Microsoft.EntityFrameworkCore;
    using ST2_2026_Team_2_JobTracker.Data;
    using ST2_2026_Team_2_JobTracker.Services;
    using ST2_2026_Team_2_JobTracker.Services.Interfaces;
    using ST2_2026_Team_2_JobTracker.DesignPatterns.Singleton;
    using ST2_2026_Team_2_JobTracker.DesignPatterns.Factory;
    using ST2_2026_Team_2_JobTracker.DesignPatterns.Strategy;
    using ST2_2026_Team_2_JobTracker.Services.AI;
    using Microsoft.Extensions.Options;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<IJobOfferService, JobOfferService>();
            builder.Services.AddScoped<IApplicationService, ApplicationService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddSingleton(ApplicationSettings.Instance);

            // Gemini credentials come from User Secrets/environment, never from source control.
            builder.Services.Configure<GeminiOptions>(
                builder.Configuration.GetSection(GeminiOptions.SectionName));
            builder.Services.AddHttpClient<IGeminiClient, GeminiClient>((provider, client) =>
            {
                var settings = provider.GetRequiredService<IOptions<GeminiOptions>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(90);
            });

            // STRATEGY + FACTORY: DI resolves both strategies, factory selects by MatchingMode.
            builder.Services.AddScoped<IMatchingStrategy, GeminiMatchingStrategy>();
            builder.Services.AddScoped<IMatchingStrategy, RuleBasedMatchingStrategy>();
            builder.Services.AddScoped<MatchingStrategyFactory>();
            builder.Services.AddScoped<IAiMatchService, AiMatchService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
