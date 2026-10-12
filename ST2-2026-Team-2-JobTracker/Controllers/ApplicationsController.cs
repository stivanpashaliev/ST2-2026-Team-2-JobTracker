namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using ST2_2026_Team_2_JobTracker.DesignPatterns.Singleton;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    using ST2_2026_Team_2_JobTracker.Models.Enums;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels.Applications;
    using ST2_2026_Team_2_JobTracker.Services;
    using ST2_2026_Team_2_JobTracker.Services.Interfaces;

    public class ApplicationsController : Controller
    {
        private readonly IApplicationService _applicationService;
        private readonly IJobOfferService _jobOfferService;
        private readonly ICompanyService _companyService;
        private readonly ApplicationSettings _settings;


        public ApplicationsController(
            IApplicationService applicationService,
            IJobOfferService jobOfferService,
            ICompanyService companyService,
            ApplicationSettings settings)
        {
            _applicationService = applicationService;
            _jobOfferService = jobOfferService;
            _companyService = companyService;
            _settings = settings;
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ApplicationCreateViewModel();
            await LoadJobOffersAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationCreateViewModel model)
        {
            if (!await _jobOfferService.ExistsAsync(model.JobOfferId))
            {
                ModelState.AddModelError(
                    nameof(model.JobOfferId),
                    "The selected job offer does not exist.");
            }
            else if (await _jobOfferService.HasApplicationAsync(model.JobOfferId))
            {
                ModelState.AddModelError(
                    nameof(model.JobOfferId),
                    "This job offer already has an application.");
            }

            if (model.Status == ApplicationStatus.Saved &&
                model.AppliedAt.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.AppliedAt),
                    "A saved application cannot have an applied date.");
            }

            if (!ModelState.IsValid)
            {
                await LoadJobOffersAsync(model);
                return View(model);
            }

            var application = new Application
            {
                JobOfferId = model.JobOfferId,
                Status = model.Status,
                AppliedAt = model.Status == ApplicationStatus.Saved
                    ? null
                    : model.AppliedAt ?? DateTime.UtcNow,
                Notes = model.Notes?.Trim()
            };

            await _applicationService.CreateAsync(application);

            TempData["SuccessMessage"] = "Application created successfully.";

            return RedirectToAction("Index", "Home");
        }

        private async Task LoadJobOffersAsync(ApplicationCreateViewModel model)
        {
            var jobOffers = await _jobOfferService.GetAllAsync();

            var availableOffers = new List<SelectListItem>();

            foreach (var jobOffer in jobOffers)
            {
                if (!await _jobOfferService.HasApplicationAsync(jobOffer.Id))
                {
                    availableOffers.Add(new SelectListItem
                    {
                        Value = jobOffer.Id.ToString(),
                        Text = jobOffer.Title
                    });
                }
            }

            model.JobOffers = availableOffers;
        }



        [HttpGet]
        public async Task<IActionResult> Index(
            string? jobTitle,
            int? companyId,
            ApplicationStatus? status,
            int page = 1)
        {
            page = Math.Max(1, page);

            int pageSize = _settings.DefaultPageSize;

            var (applications, totalCount) =
                await _applicationService.GetFilteredAsync(
                    jobTitle,
                    companyId,
                    status,
                    page,
                    pageSize);

            int totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;

                (applications, totalCount) =
                    await _applicationService.GetFilteredAsync(
                        jobTitle,
                        companyId,
                        status,
                        page,
                        pageSize);
            }

            var companies = await _companyService.GetAllAsync();

            var model = new ApplicationIndexViewModel
            {
                JobTitle = jobTitle,
                CompanyId = companyId,
                Status = status,
                Applications = applications,
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
