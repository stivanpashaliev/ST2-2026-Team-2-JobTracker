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
                // An explicitly selected date represents UTC midnight; when no date
                // is supplied, record the actual time of application.
                AppliedAt = model.Status == ApplicationStatus.Saved
                    ? null
                    : model.AppliedAt.HasValue
                        ? DateTime.SpecifyKind(model.AppliedAt.Value.Date, DateTimeKind.Utc)
                        : DateTime.UtcNow,
                Notes = model.Notes
            };

            try
            {
                await _applicationService.CreateAsync(application);
            }
            catch (DbUpdateException)
            {
                // Checking before SaveChanges is not atomic. A second request can
                // create an application for this offer between the check and insert.
                if (await _jobOfferService.HasApplicationAsync(model.JobOfferId))
                {
                    ModelState.AddModelError(nameof(model.JobOfferId),
                        "This job offer already has an application.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to create the application. The job offer may have been removed.");
                }

                await LoadJobOffersAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Application created successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadJobOffersAsync(ApplicationCreateViewModel model, int? currentJobOfferId = null)
        {
            var jobOffers = await _jobOfferService
                .GetAvailableForApplicationAsync(currentJobOfferId);

            model.JobOffers = jobOffers.Select(j => new SelectListItem
            {
                Value = j.Id.ToString(),
                Text = j.Title
            }).ToList();
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

            if (companyId is <= 0)
            {
                ModelState.AddModelError(nameof(ApplicationIndexViewModel.CompanyId),
                    "Please select a valid company.");
            }
            if (status.HasValue && !Enum.IsDefined(status.Value))
            {
                ModelState.AddModelError(nameof(ApplicationIndexViewModel.Status),
                    "Please select a valid status.");
            }

            var (applications, totalCount) = ModelState.IsValid
                ? await _applicationService.GetFilteredAsync(
                    jobTitle, companyId, status, page, pageSize)
                : (new List<Application>(), 0);

            int totalPages = (int)Math.Ceiling(
                (double)totalCount / pageSize);

            page = totalPages > 0 ? Math.Min(page, totalPages) : 1;

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
            // HTML date inputs omit the time. Editing only the notes/status must
            // not silently reset the time of an existing application.
            existing.AppliedAt = model.Status == ApplicationStatus.Saved
                ? null
                : model.AppliedAt.HasValue
                    ? existing.AppliedAt.HasValue &&
                      existing.AppliedAt.Value.Date == model.AppliedAt.Value.Date
                        ? existing.AppliedAt
                        : DateTime.SpecifyKind(model.AppliedAt.Value.Date, DateTimeKind.Utc)
                    : existing.AppliedAt ?? DateTime.UtcNow;
            existing.Notes = model.Notes;

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
                if (await _jobOfferService.HasApplicationAsync(model.JobOfferId) &&
                    model.JobOfferId != currentJobOfferId)
                {
                    ModelState.AddModelError(nameof(model.JobOfferId),
                        "This job offer already has an application.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty,
                        "Unable to save changes. A linked record may have changed. Reload the page and try again.");
                }

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
