namespace ST2_2026_Team_2_JobTracker.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using ST2_2026_Team_2_JobTracker.Services;
    using Microsoft.EntityFrameworkCore;
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


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var existing = await _companyService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            var model = new CompanyEditViewModel
            {
                Id = existing.Id,
                Name = existing.Name,
                Website = existing.Website,
                Location = existing.Location,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, CompanyEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var existing = await _companyService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            existing.Name = model.Name.Trim();
            existing.Website = model.Website?.Trim();
            existing.Location = model.Location?.Trim();

            try
            {
                if (!await _companyService.UpdateAsync(existing))
                {
                    return NotFound();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _companyService.GetByIdAsync(id) == null)
                {
                    return NotFound();
                }

                ModelState.AddModelError(string.Empty, "This record changed while you were editing it. Reload the page and try again.");
                return View(model);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. A linked record may have changed. Reload the page and try again.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Company updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _companyService.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            return View(new DeleteViewModel
            {
                Id = id,
                EntityName = "Company",
                DisplayName = existing.Name,
                BlockingMessage = await _companyService.HasJobOffersAsync(id)
                    ? "Delete or reassign this company's job offers before deleting the company."
                    : null
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            try
            {
                var result = await _companyService.DeleteAsync(id);
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

            TempData["SuccessMessage"] = "Company deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
