using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST2_2026_Team_2_JobTracker.Models.ViewModels.AiMatch;
using ST2_2026_Team_2_JobTracker.Services.AI;
using ST2_2026_Team_2_JobTracker.Services.Interfaces;

namespace ST2_2026_Team_2_JobTracker.Controllers;

public sealed class AiMatchController(IAiMatchService aiMatchService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? jobOfferId, CancellationToken cancellationToken)
    {
        var model = new AiMatchViewModel { JobOfferId = jobOfferId ?? 0 };
        await LoadJobOffersAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AiMatchViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                model.Result = await aiMatchService.AnalyzeAsync(
                    model.JobOfferId, model.SkillsAndExperience, model.Mode, cancellationToken);

                if (model.Result is null)
                    ModelState.AddModelError(nameof(model.JobOfferId),
                        "The selected job offer no longer exists.");
            }
            catch (GeminiApiException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await LoadJobOffersAsync(model, cancellationToken);
        return View(model);
    }

    private async Task LoadJobOffersAsync(AiMatchViewModel model, CancellationToken cancellationToken)
    {
        var offers = await aiMatchService.GetJobOffersAsync(cancellationToken);
        model.JobOffers = offers.Select(o => new SelectListItem
        {
            Value = o.Id.ToString(),
            Text = $"{o.Title} — {o.CompanyName}"
        }).ToArray();
    }
}
