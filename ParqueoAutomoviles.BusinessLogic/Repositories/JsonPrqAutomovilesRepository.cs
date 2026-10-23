using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class JsonPrqAutomovilesRepository(JsonPrqDataSource dataSource)
    : JsonRepositoryBase<Prqautomovile>(dataSource, "PRQAutomoviles.json", car => car.Id), IPrqAutomovilesRepository
{
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
