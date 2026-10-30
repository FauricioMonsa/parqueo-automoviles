using Microsoft.AspNetCore.Mvc;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AutomovilesController(IPrqAutomovilesRepository automovilesRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Prqautomovile>>> GetAll(
        [FromQuery] string? color,
        [FromQuery] int? fromYear,
        [FromQuery] int? toYear,
        [FromQuery] string? manufacturer,
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        if (fromYear.HasValue != toYear.HasValue || (fromYear.HasValue && fromYear > toYear))
        {
            return BadRequest("Debe indicar ambos años y fromYear no puede ser mayor que toYear.");
        }

        var parsedType = default(PRQTipoAutomovil);
        if (type is not null && !TryParseType(type, out parsedType))
        {
            return BadRequest($"Tipo de automóvil no válido: {type}.");
        }

        var result = await automovilesRepository.GetAllAsync(
            color,
            fromYear,
            toYear,
            manufacturer,
            type is null ? null : parsedType,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Prqautomovile>> GetById(long id, CancellationToken cancellationToken)
    {
        var automobile = await automovilesRepository.GetByIdAsync(id, cancellationToken);
        return automobile is null ? NotFound() : Ok(automobile);
    }

    [HttpGet("color/{color}")]
    public async Task<ActionResult<IReadOnlyList<Prqautomovile>>> GetByColor(string color, CancellationToken cancellationToken) =>
        Ok(await automovilesRepository.GetByColorAsync(color, cancellationToken));

    [HttpGet("year/{fromYear:int}/{toYear:int}")]
    public async Task<ActionResult<IReadOnlyList<Prqautomovile>>> GetByYearRange(
        int fromYear,
        int toYear,
        CancellationToken cancellationToken)
    {
        if (fromYear > toYear)
        {
            return BadRequest("fromYear no puede ser mayor que toYear.");
        }

        return Ok(await automovilesRepository.GetByYearRangeAsync(fromYear, toYear, cancellationToken));
    }

    [HttpGet("manufacturer/{manufacturer}")]
    public async Task<ActionResult<IReadOnlyList<Prqautomovile>>> GetByManufacturer(
        string manufacturer,
        CancellationToken cancellationToken) =>
        Ok(await automovilesRepository.GetByManufacturerAsync(manufacturer, cancellationToken));

    [HttpGet("type/{type}")]
    public async Task<ActionResult<IReadOnlyList<Prqautomovile>>> GetByType(string type, CancellationToken cancellationToken)
    {
        if (!TryParseType(type, out var parsedType))
        {
            return BadRequest($"Tipo de automóvil no válido: {type}.");
        }

        return Ok(await automovilesRepository.GetByTypeAsync(parsedType, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Prqautomovile>> Insert(
        [FromBody] Prqautomovile automobile,
        CancellationToken cancellationToken)
    {
        await automovilesRepository.InsertAsync(automobile, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = automobile.Id }, automobile);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] Prqautomovile automobile, CancellationToken cancellationToken)
    {
        if (id != automobile.Id)
        {
            return BadRequest("El id de la ruta debe coincidir con el id del automóvil.");
        }

        if (await automovilesRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await automovilesRepository.UpdateAsync(automobile, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (await automovilesRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await automovilesRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static bool TryParseType(string value, out PRQTipoAutomovil type)
    {
        if (string.Equals(value, "4x4", StringComparison.OrdinalIgnoreCase))
        {
            type = PRQTipoAutomovil.CuatroX4;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out type) && Enum.IsDefined(type);
    }
}