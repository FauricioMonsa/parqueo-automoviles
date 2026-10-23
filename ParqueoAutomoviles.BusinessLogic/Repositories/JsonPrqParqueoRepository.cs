using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class JsonPrqParqueoRepository(JsonPrqDataSource dataSource) : IPrqParqueoRepository
{
    public async Task<Prqparqueo?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        (await dataSource.LoadParkingsAsync(cancellationToken)).FirstOrDefault(parking => parking.Id == id);

    public async Task<IReadOnlyList<Prqparqueo>> GetByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provinceName);
        return (await dataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.ProvinceName.Contains(provinceName, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        return (await dataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        if (minPrice > maxPrice)
        {
            throw new ArgumentException("El precio mínimo no puede ser mayor que el precio máximo.");
        }

        return (await dataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.PricePerHour >= minPrice && parking.PricePerHour <= maxPrice).ToList();
    }
}
