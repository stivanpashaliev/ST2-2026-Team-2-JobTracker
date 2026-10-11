namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels;
    using ST2_2026_Team_2_JobTracker.Services.Interfaces;
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
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
    }
}
