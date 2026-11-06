using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class JsonPrqAutomovilesRepository(JsonPrqDataSource dataSource)
    : JsonRepositoryBase<Prqautomovile>(dataSource, "PRQAutomoviles.json", car => car.Id, (car, id) => car.Id = id), IPrqAutomovilesRepository
{
    public async Task<IReadOnlyList<Prqautomovile>> GetAllAsync(
        string? color = null,
        int? fromYear = null,
        int? toYear = null,
        string? manufacturer = null,
        PRQTipoAutomovil? type = null,
        CancellationToken cancellationToken = default)
    {
        if (fromYear.HasValue != toYear.HasValue || (fromYear.HasValue && fromYear > toYear))
        {
            throw new ArgumentException("Debe indicar ambos años y el año inicial no puede superar al final.");
        }

        var cars = await DataSource.LoadCarsAsync(cancellationToken);
        return cars.Where(car =>
                (string.IsNullOrWhiteSpace(color) || car.Color.Contains(color, StringComparison.OrdinalIgnoreCase)) &&
                (!fromYear.HasValue || (car.Year >= fromYear.Value && car.Year <= toYear!.Value)) &&
                (string.IsNullOrWhiteSpace(manufacturer) || car.Manufacturer.Contains(manufacturer, StringComparison.OrdinalIgnoreCase)) &&
                (!type.HasValue || car.Type == type.Value))
            .ToList();
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(color);
        return (await DataSource.LoadCarsAsync(cancellationToken))
            .Where(car => car.Color.Contains(color, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByYearRangeAsync(int fromYear, int toYear, CancellationToken cancellationToken = default)
    {
        ValidateRange(fromYear, toYear);
        return (await DataSource.LoadCarsAsync(cancellationToken))
            .Where(car => car.Year >= fromYear && car.Year <= toYear).ToList();
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manufacturer);
        return (await DataSource.LoadCarsAsync(cancellationToken))
            .Where(car => car.Manufacturer.Contains(manufacturer, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByTypeAsync(PRQTipoAutomovil type, CancellationToken cancellationToken = default) =>
        (await DataSource.LoadCarsAsync(cancellationToken)).Where(car => car.Type == type).ToList();

    private static void ValidateRange(int fromYear, int toYear)
    {
        if (fromYear > toYear)
        {
            throw new ArgumentException("El año inicial no puede ser mayor que el año final.");
        }
    }
}
