namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels;
    using Microsoft.EntityFrameworkCore;
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
                Title = model.Title,
                Description = model.Description,
                Location = model.Location,
                Salary = model.Salary,
                RemoteType = model.RemoteType,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _jobOfferService.CreateAsync(jobOffer);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to create the job offer. The selected company may have been removed.");
                await LoadCompaniesAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Job offer created successfully.";

            return RedirectToAction(nameof(Index));
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

    if (companyId is <= 0)
    {
        ModelState.AddModelError(nameof(JobOfferIndexViewModel.CompanyId),
            "Please select a valid company.");
    }
    if (remoteType.HasValue && !Enum.IsDefined(remoteType.Value))
    {
        ModelState.AddModelError(nameof(JobOfferIndexViewModel.RemoteType),
            "Please select a valid work type.");
    }
    if (minSalary < 0)
    {
        ModelState.AddModelError(nameof(JobOfferIndexViewModel.MinSalary),
            "Minimum salary cannot be negative.");
    }
    if (maxSalary < 0)
    {
        ModelState.AddModelError(nameof(JobOfferIndexViewModel.MaxSalary),
            "Maximum salary cannot be negative.");
    }
    if (minSalary.HasValue && maxSalary.HasValue && minSalary > maxSalary)
    {
        ModelState.AddModelError(nameof(JobOfferIndexViewModel.MaxSalary),
            "Maximum salary must be greater than or equal to minimum salary.");
    }

    // Also respects MVC model-binding errors (e.g. minSalary=not-a-number).
    // Invalid filters must not execute misleading database queries.
    var (jobOffers, totalCount) = ModelState.IsValid
        ? await _jobOfferService.GetFilteredAsync(
            title, companyId, location, remoteType, minSalary, maxSalary,
            page, pageSize)
        : (new List<JobOffer>(), 0);

    int totalPages = (int)Math.Ceiling(
        (double)totalCount / pageSize);

    page = totalPages > 0 ? Math.Min(page, totalPages) : 1;

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var existing = await _jobOfferService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var model = new JobOfferEditViewModel
            {
                Id = existing.Id,
                CompanyId = existing.CompanyId,
                Title = existing.Title,
                Description = existing.Description,
                Location = existing.Location,
                Salary = existing.Salary,
                RemoteType = existing.RemoteType,
            };
            await LoadCompaniesAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, JobOfferEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var existing = await _jobOfferService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!await _companyService.ExistsAsync(model.CompanyId))
            {
                ModelState.AddModelError(nameof(model.CompanyId), "The selected company does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCompaniesAsync(model);
                return View(model);
            }

            existing.CompanyId = model.CompanyId;
            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Location = model.Location;
            existing.Salary = model.Salary;
            existing.RemoteType = model.RemoteType;

            try
            {
                if (!await _jobOfferService.UpdateAsync(existing))
                {
                    return NotFound();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _jobOfferService.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "This record changed while you were editing it. Reload the page and try again.");
                await LoadCompaniesAsync(model);
                return View(model);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. A linked record may have changed. Reload the page and try again.");
                await LoadCompaniesAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Job offer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _jobOfferService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            return View(new DeleteViewModel
            {
                Id = id,
                EntityName = "Job offer",
                DisplayName = existing.Title,
                BlockingMessage = await _jobOfferService.HasApplicationAsync(id)
                    ? "Delete this job offer's application before deleting the job offer."
                    : null
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            try
            {
                var result = await _jobOfferService.DeleteAsync(id);
                if (result == DeleteResult.NotFound)
                {
                    return NotFound();
                }

                if (result == DeleteResult.HasDependencies)
                {
                    return await Delete(id);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this record. It may still be linked to another record. Reload the page and try again.");
                return await Delete(id);
            }

            TempData["SuccessMessage"] = "Job offer deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
