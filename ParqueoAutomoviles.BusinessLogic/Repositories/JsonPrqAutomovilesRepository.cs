using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class JsonPrqAutomovilesRepository(JsonPrqDataSource dataSource) : IPrqAutomovilesRepository
{
    public async Task<Prqautomovile?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        (await dataSource.LoadCarsAsync(cancellationToken)).FirstOrDefault(car => car.Id == id);

    public async Task<IReadOnlyList<Prqautomovile>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(color);
        return (await dataSource.LoadCarsAsync(cancellationToken))
            .Where(car => car.Color.Contains(color, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByYearRangeAsync(int fromYear, int toYear, CancellationToken cancellationToken = default)
    {
        ValidateRange(fromYear, toYear);
        return (await dataSource.LoadCarsAsync(cancellationToken))
            .Where(car => car.Year >= fromYear && car.Year <= toYear).ToList();
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manufacturer);
        return (await dataSource.LoadCarsAsync(cancellationToken))
            .Where(car => car.Manufacturer.Contains(manufacturer, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByTypeAsync(PRQTipoAutomovil type, CancellationToken cancellationToken = default) =>
        (await dataSource.LoadCarsAsync(cancellationToken)).Where(car => car.Type == type).ToList();

    private static void ValidateRange(int fromYear, int toYear)
    {
        if (fromYear > toYear)
        {
            throw new ArgumentException("El año inicial no puede ser mayor que el año final.");
        }
    }
}
