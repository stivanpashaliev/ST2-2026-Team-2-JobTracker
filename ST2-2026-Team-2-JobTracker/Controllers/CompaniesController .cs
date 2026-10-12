namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ST2_2026_Team_2_JobTracker.DesignPatterns.Singleton;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels.Companies;
    using ST2_2026_Team_2_JobTracker.Services.Interfaces;
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly ApplicationSettings _settings;


        public CompaniesController(
            ICompanyService companyService,
            ApplicationSettings settings)
        {
            _companyService = companyService;
            _settings = settings;
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new CompanyCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var company = new Company
            {
                Name = model.Name.Trim(),
                Website = model.Website?.Trim(),
                Location = model.Location?.Trim()
            };

            await _companyService.CreateAsync(company);

            TempData["SuccessMessage"] = "Company created successfully.";

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public async Task<IActionResult> Index(
            string? name,
            string? location,
            int page = 1)
        {
            page = Math.Max(1, page);

            int pageSize = _settings.DefaultPageSize;

            var (companies, totalCount) =
                await _companyService.GetFilteredAsync(
                    name,
                    location,
                    page,
                    pageSize);

            int totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;

                (companies, totalCount) =
                    await _companyService.GetFilteredAsync(
                        name,
                        location,
                        page,
                        pageSize);
            }

            var model = new CompanyIndexViewModel
            {
                Name = name,
                Location = location,
                Companies = companies,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            ViewData["ApplicationName"] = _settings.ApplicationName;

            return View(model);
        }

    }
}
