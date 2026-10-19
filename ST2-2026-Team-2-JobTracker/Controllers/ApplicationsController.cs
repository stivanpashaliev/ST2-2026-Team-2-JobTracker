namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ST2_2026_Team_2_JobTracker.Models.ViewModels;
    using Microsoft.EntityFrameworkCore;
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

        private async Task LoadJobOffersAsync(ApplicationCreateViewModel model, int? currentJobOfferId = null)
        {
            var jobOffers = await _jobOfferService.GetAllAsync();

            var availableOffers = new List<SelectListItem>();

            foreach (var jobOffer in jobOffers)
            {
                if (jobOffer.Id == currentJobOfferId || !await _jobOfferService.HasApplicationAsync(jobOffer.Id))
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var existing = await _applicationService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var model = new ApplicationEditViewModel
            {
                Id = existing.Id,
                JobOfferId = existing.JobOfferId,
                Status = existing.Status,
                AppliedAt = existing.AppliedAt,
                Notes = existing.Notes,
            };
            await LoadJobOffersAsync(model, existing.JobOfferId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, ApplicationEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var existing = await _applicationService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!await _jobOfferService.ExistsAsync(model.JobOfferId))
            {
                ModelState.AddModelError(nameof(model.JobOfferId), "The selected job offer does not exist.");
            }
            else if (model.JobOfferId != existing.JobOfferId &&
                await _jobOfferService.HasApplicationAsync(model.JobOfferId))
            {
                ModelState.AddModelError(nameof(model.JobOfferId), "This job offer already has an application.");
            }

            if (model.Status == ApplicationStatus.Saved && model.AppliedAt.HasValue)
            {
                ModelState.AddModelError(nameof(model.AppliedAt), "A saved application cannot have an applied date. Clear the date to save this status.");
            }

            if (!ModelState.IsValid)
            {
                await LoadJobOffersAsync(model, existing.JobOfferId);
                return View(model);
            }

            var currentJobOfferId = existing.JobOfferId;
            existing.JobOfferId = model.JobOfferId;
            existing.Status = model.Status;
            existing.AppliedAt = model.Status == ApplicationStatus.Saved ? null : model.AppliedAt ?? existing.AppliedAt ?? DateTime.UtcNow;
            existing.Notes = model.Notes?.Trim();

            try
            {
                if (!await _applicationService.UpdateAsync(existing))
                {
                    return NotFound();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _applicationService.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "This record changed while you were editing it. Reload the page and try again.");
                await LoadJobOffersAsync(model, currentJobOfferId);
                return View(model);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. A linked record may have changed. Reload the page and try again.");
                await LoadJobOffersAsync(model, currentJobOfferId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Application updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _applicationService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            return View(new DeleteViewModel
            {
                Id = id,
                EntityName = "Application",
                DisplayName = existing.JobOffer.Title
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            try
            {
                var result = await _applicationService.DeleteAsync(id);
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

            TempData["SuccessMessage"] = "Application deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
