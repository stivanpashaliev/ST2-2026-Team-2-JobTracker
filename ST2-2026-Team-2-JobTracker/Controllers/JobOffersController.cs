namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using ST2_2026_Team_2_JobTracker.Models.Entities;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels;
    using ST2_2026_Team_2_JobTracker.Services.Interfaces;
    public class JobOffersController : Controller
    {
        private readonly IJobOfferService _jobOfferService;
        private readonly ICompanyService _companyService;

        public JobOffersController(
            IJobOfferService jobOfferService,
            ICompanyService companyService)
        {
            _jobOfferService = jobOfferService;
            _companyService = companyService;
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
    }
}
