using Microsoft.AspNetCore.Mvc;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ParqueoController(IPrqParqueoRepository parqueoRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Prqparqueo>>> GetAll(
        [FromQuery] string? provinceName,
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        CancellationToken cancellationToken)
    {
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
        {
            return BadRequest("minPrice no puede ser mayor que maxPrice.");
        }

        var parkings = await parqueoRepository.GetAllAsync(
            provinceName,
            name,
            minPrice,
            maxPrice,
            cancellationToken);

        return Ok(parkings);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Prqparqueo>> GetById(long id, CancellationToken cancellationToken)
    {
        var parking = await parqueoRepository.GetByIdAsync(id, cancellationToken);
        return parking is null ? NotFound() : Ok(parking);
    }

    [HttpGet("province/{provinceName}")]
    public async Task<ActionResult<IReadOnlyList<Prqparqueo>>> GetByProvinceName(
        string provinceName,
        CancellationToken cancellationToken) =>
        Ok(await parqueoRepository.GetByProvinceNameAsync(provinceName, cancellationToken));

    [HttpGet("name/{name}")]
    public async Task<ActionResult<IReadOnlyList<Prqparqueo>>> GetByName(
        string name,
        CancellationToken cancellationToken) =>
        Ok(await parqueoRepository.GetByNameAsync(name, cancellationToken));

    [HttpGet("price/{minPrice:decimal}/{maxPrice:decimal}")]
    public async Task<ActionResult<IReadOnlyList<Prqparqueo>>> GetByPriceRange(
        decimal minPrice,
        decimal maxPrice,
        CancellationToken cancellationToken)
    {
        if (minPrice > maxPrice)
        {
            return BadRequest("minPrice no puede ser mayor que maxPrice.");
        }

        return Ok(await parqueoRepository.GetByPriceRangeAsync(minPrice, maxPrice, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Prqparqueo>> Insert(
        [FromBody] Prqparqueo parking,
        CancellationToken cancellationToken)
    {
        await parqueoRepository.InsertAsync(parking, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = parking.Id }, parking);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] Prqparqueo parking, CancellationToken cancellationToken)
    {
        if (id != parking.Id)
        {
            return BadRequest("El id de la ruta debe coincidir con el id del parqueo.");
        }

        if (await parqueoRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await parqueoRepository.UpdateAsync(parking, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        if (await parqueoRepository.GetByIdAsync(id, cancellationToken) is null)
        {
            return NotFound();
        }

        await parqueoRepository.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}