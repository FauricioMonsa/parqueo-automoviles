using Microsoft.AspNetCore.Mvc;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IngresoAutomovilesController(IPrqIngresoAutomovilesRepository ingresosRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PrqingresoAutomovileCustomAttributes>>> GetAll(
        [FromQuery] string? type,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? provinceName,
        CancellationToken cancellationToken)
    {
        if (fromDate.HasValue != toDate.HasValue || (fromDate.HasValue && fromDate > toDate))
        {
            return BadRequest("Debe indicar ambas fechas y fromDate no puede ser posterior a toDate.");
        }

        PRQTipoAutomovil? parsedType = null;
        if (type is not null)
        {
            if (!TryParseType(type, out var value))
            {
                return BadRequest($"Tipo de automóvil no válido: {type}.");
            }

            parsedType = value;
        }

        return Ok(await ingresosRepository.GetAllAsync(parsedType, fromDate, toDate, provinceName, cancellationToken));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PrqingresoAutomovileCustomAttributes>> GetById(long id, CancellationToken cancellationToken)
    {
        var entry = await ingresosRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpGet("type/{type}/dates/{fromDate}/{toDate}")]
    public async Task<ActionResult<IReadOnlyList<PrqingresoAutomovileCustomAttributes>>> GetByTypeAndDateRange(
        string type,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        if (!TryParseType(type, out var parsedType))
        {
            return BadRequest($"Tipo de automóvil no válido: {type}.");
        }

        if (fromDate > toDate)
        {
            return BadRequest("fromDate no puede ser posterior a toDate.");
        }

        return Ok(await ingresosRepository.GetByTypeAndDateRangeAsync(parsedType, fromDate, toDate, cancellationToken));
    }

    [HttpGet("province/{provinceName}/dates/{fromDate}/{toDate}")]
    public async Task<ActionResult<IReadOnlyList<PrqingresoAutomovileCustomAttributes>>> GetByProvinceAndDateRange(
        string provinceName,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        if (fromDate > toDate)
        {
            return BadRequest("fromDate no puede ser posterior a toDate.");
        }

        return Ok(await ingresosRepository.GetByProvinceAndDateRangeAsync(provinceName, fromDate, toDate, cancellationToken));
    }

    [HttpGet("parking/{parkingId:long}/price")]
    public async Task<ActionResult<decimal>> GetPricePerHourByParking(long parkingId, CancellationToken cancellationToken)
    {
        var price = await ingresosRepository.GetPricePerHourByParkingAsync(parkingId, cancellationToken);
        return price.HasValue ? Ok(price.Value) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<PrqingresoAutomovile>> Insert(
        [FromBody] PrqingresoAutomovile entry,
        CancellationToken cancellationToken)
    {
        await ingresosRepository.InsertAsync(entry, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entry.Consecutive }, entry);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PrqingresoAutomovile entry, CancellationToken cancellationToken)
    {
        if (id != entry.Consecutive)
        {
            return BadRequest("El id de la ruta debe coincidir con el consecutivo del ingreso.");
        }

        if (await ingresosRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await ingresosRepository.UpdateAsync(entry, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (await ingresosRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await ingresosRepository.DeleteAsync(id, cancellationToken);
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