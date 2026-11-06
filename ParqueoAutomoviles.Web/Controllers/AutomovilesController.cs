using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;
using ParqueoAutomoviles.Web.Models;

namespace ParqueoAutomoviles.Web.Controllers;

public sealed class AutomovilesController(
    IPrqAutomovilesRepository automovilesRepository,
    IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index(
        int? minYear,
        int? maxYear,
        bool search = false,
        CancellationToken cancellationToken = default)
    {
        if ((search || minYear.HasValue || maxYear.HasValue) && minYear.HasValue != maxYear.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Debe indicar ambos años.");
        }

        if (minYear.HasValue && maxYear.HasValue && minYear > maxYear)
        {
            ModelState.AddModelError(nameof(minYear), "El año mínimo no puede ser mayor que el año máximo.");
        }

        var viewModel = new AutomovilesIndexViewModel
        {
            MinYear = minYear,
            MaxYear = maxYear
        };

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        viewModel.Automoviles = minYear.HasValue
            ? await automovilesRepository.GetByYearRangeAsync(minYear.Value, maxYear!.Value, cancellationToken)
            : await automovilesRepository.GetAllAsync(cancellationToken: cancellationToken);

        return View(viewModel);
    }

    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken)
    {
        var automobile = await automovilesRepository.GetByIdAsync(id, cancellationToken);
        return automobile is null ? NotFound() : View(automobile);
    }

    public IActionResult Create()
    {
        SetTypeOptions();
        return View(new AutomovilFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AutomovilFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            SetTypeOptions(model.Type);
            return View(model);
        }

        var automobile = ToEntity(model);
        if (string.Equals(configuration["DataSource"] ?? "EfCore", "Json", StringComparison.OrdinalIgnoreCase))
        {
            var existingCars = await automovilesRepository.GetAllAsync(cancellationToken: cancellationToken);
            automobile.Id = existingCars.Count == 0 ? 1 : existingCars.Max(car => car.Id) + 1;
        }

        await automovilesRepository.InsertAsync(automobile, cancellationToken);
        return RedirectToAction(nameof(Details), new { id = automobile.Id });
    }

    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        var automobile = await automovilesRepository.GetByIdAsync(id, cancellationToken);
        if (automobile is null)
        {
            return NotFound();
        }

        var model = ToViewModel(automobile);
        SetTypeOptions(model.Type);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, AutomovilFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            SetTypeOptions(model.Type);
            return View(model);
        }

        if (await automovilesRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await automovilesRepository.UpdateAsync(ToEntity(model), cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var automobile = await automovilesRepository.GetByIdAsync(id, cancellationToken);
        return automobile is null ? NotFound() : View(automobile);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id, CancellationToken cancellationToken)
    {
        if (await automovilesRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await automovilesRepository.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private void SetTypeOptions(PRQTipoAutomovil? selectedType = null)
    {
        ViewData["TypeOptions"] = Enum.GetValues<PRQTipoAutomovil>()
            .Select(type => new SelectListItem(
                type == PRQTipoAutomovil.CuatroX4 ? "4x4" : type.ToString(),
                type.ToString(),
                selectedType == type));
    }

    private static Prqautomovile ToEntity(AutomovilFormViewModel model) => new()
    {
        Id = model.Id,
        Color = model.Color,
        Year = model.Year,
        Manufacturer = model.Manufacturer,
        Type = model.Type!.Value
    };

    private static AutomovilFormViewModel ToViewModel(Prqautomovile automobile) => new()
    {
        Id = automobile.Id,
        Color = automobile.Color,
        Year = automobile.Year,
        Manufacturer = automobile.Manufacturer,
        Type = automobile.Type
    };
}
