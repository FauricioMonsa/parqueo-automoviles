using Microsoft.AspNetCore.Mvc;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;
using ParqueoAutomoviles.Web.Models;

namespace ParqueoAutomoviles.Web.Controllers;

public sealed class ConsultasController(
    IPrqAutomovilesRepository automovilesRepository,
    IPrqIngresoAutomovilesRepository ingresosRepository,
    IPrqParqueoRepository parqueosRepository) : Controller
{
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> AutomovilesPorTipo(string type, CancellationToken cancellationToken)
    {
        if (!TryParseType(type, out var parsedType))
        {
            return BadRequest(new { message = $"Tipo de automóvil no válido: {type}." });
        }

        var automobiles = await automovilesRepository.GetByTypeAsync(parsedType, cancellationToken);
        return Json(automobiles.Select(car => new
        {
            car.Id,
            car.Color,
            car.Year,
            car.Manufacturer,
            Type = car.Type == PRQTipoAutomovil.CuatroX4 ? "4x4" : car.Type.ToString()
        }));
    }

    [HttpGet]
    public async Task<IActionResult> IngresosPorAutomovil(long carId, CancellationToken cancellationToken)
    {
        if (await automovilesRepository.GetByIdAsync(carId, cancellationToken) is null)
        {
            return NotFound(new { message = "No se encontró el automóvil seleccionado." });
        }

        var entries = await ingresosRepository.GetByCarIdAsync(carId, cancellationToken);
        var result = new List<IngresoAutomovilConsultaViewModel>(entries.Count);
        var parkingPrices = new Dictionary<long, decimal>();

        foreach (var entry in entries)
        {
            if (!parkingPrices.TryGetValue(entry.ParkingId, out var pricePerHour))
            {
                var parking = await parqueosRepository.GetByIdAsync(entry.ParkingId, cancellationToken);
                if (parking is null)
                {
                    return Conflict(new { message = $"No se encontró el parqueo {entry.ParkingId} de un ingreso." });
                }

                pricePerHour = parking.PricePerHour;
                parkingPrices.Add(entry.ParkingId, pricePerHour);
            }

            var endDateTime = entry.ExitDatetime ?? DateTime.Now;
            var duration = endDateTime > entry.EntryDatetime
                ? endDateTime - entry.EntryDatetime
                : TimeSpan.Zero;
            var billedHours = (decimal)Math.Ceiling(duration.TotalHours);

            result.Add(new IngresoAutomovilConsultaViewModel(
                entry.ParkingId,
                entry.EntryDatetime,
                entry.ExitDatetime,
                duration.TotalMinutes,
                duration.TotalHours,
                billedHours * pricePerHour));
        }

        return Json(result);
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
