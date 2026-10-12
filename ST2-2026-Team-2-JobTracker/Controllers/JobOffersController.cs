namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using ST2_2026_Team_2_JobTracker.DesignPatterns.Singleton;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    using ST2_2026_Team_2_JobTracker.Models.Enums;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels.JobOffers;
    using ST2_2026_Team_2_JobTracker.Services.Interfaces;
    public class JobOffersController : Controller
    {
        private readonly IJobOfferService _jobOfferService;
        private readonly ICompanyService _companyService;
        private readonly ApplicationSettings _settings;

        public JobOffersController(
            IJobOfferService jobOfferService,
            ICompanyService companyService,
            ApplicationSettings settings)
        {
            _jobOfferService = jobOfferService;
            _companyService = companyService;
            _settings = settings;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new JobOfferCreateViewModel();
            await LoadCompaniesAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobOfferCreateViewModel model)
        {
            if (!await _companyService.ExistsAsync(model.CompanyId))
            {
                ModelState.AddModelError(
                    nameof(model.CompanyId),
                    "The selected company does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCompaniesAsync(model);
                return View(model);
            }

            var jobOffer = new JobOffer
            {
                CompanyId = model.CompanyId,
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                Location = model.Location?.Trim(),
                Salary = model.Salary,
                RemoteType = model.RemoteType,
                CreatedAt = DateTime.UtcNow
            };

            await _jobOfferService.CreateAsync(jobOffer);

            TempData["SuccessMessage"] = "Job offer created successfully.";

            return RedirectToAction("Index", "Home");
        }

        private async Task LoadCompaniesAsync(JobOfferCreateViewModel model)
        {
            var companies = await _companyService.GetAllAsync();

            model.Companies = companies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }



        [HttpGet]
public async Task<IActionResult> Index(
    string? title,
    int? companyId,
    string? location,
    RemoteType? remoteType,
    decimal? minSalary,
    decimal? maxSalary,
    int page = 1)
{
    page = Math.Max(1, page);

    int pageSize = _settings.DefaultPageSize;

    var (jobOffers, totalCount) =
        await _jobOfferService.GetFilteredAsync(
            title,
            companyId,
            location,
            remoteType,
            minSalary,
            maxSalary,
            page,
            pageSize);

    int totalPages = (int)Math.Ceiling(
        (double)totalCount / pageSize);

    if (totalPages > 0 && page > totalPages)
    {
        page = totalPages;

        (jobOffers, totalCount) =
            await _jobOfferService.GetFilteredAsync(
                title,
                companyId,
                location,
                remoteType,
                minSalary,
                maxSalary,
                page,
                pageSize);
    }

    var companies = await _companyService.GetAllAsync();

    var model = new JobOfferIndexViewModel
    {
        Title = title,
        CompanyId = companyId,
        Location = location,
        RemoteType = remoteType,
        MinSalary = minSalary,
        MaxSalary = maxSalary,
        JobOffers = jobOffers,
        CurrentPage = page,
        TotalPages = totalPages,
        TotalCount = totalCount,

        Companies = companies.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList()
    };

    return View(model);
      }
    }
}
