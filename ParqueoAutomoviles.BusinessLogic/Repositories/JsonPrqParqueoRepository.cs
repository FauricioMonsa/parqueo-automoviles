using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class JsonPrqParqueoRepository(JsonPrqDataSource dataSource)
    : JsonRepositoryBase<Prqparqueo>(dataSource, "PRQParqueo.json", parking => parking.Id), IPrqParqueoRepository
{
    public async Task<IReadOnlyList<Prqparqueo>> GetAllAsync(
        string? provinceName = null,
        string? name = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
        {
            throw new ArgumentException("El precio mínimo no puede ser mayor que el precio máximo.");
        }

        var parkings = await DataSource.LoadParkingsAsync(cancellationToken);
        return parkings.Where(parking =>
                (string.IsNullOrWhiteSpace(provinceName) || parking.ProvinceName.Contains(provinceName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(name) || parking.Name.Contains(name, StringComparison.OrdinalIgnoreCase)) &&
                (!minPrice.HasValue || parking.PricePerHour >= minPrice.Value) &&
                (!maxPrice.HasValue || parking.PricePerHour <= maxPrice.Value))
            .ToList();
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provinceName);
        return (await DataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.ProvinceName.Contains(provinceName, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        return (await DataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        if (minPrice > maxPrice)
        {
            throw new ArgumentException("El precio mínimo no puede ser mayor que el precio máximo.");
        }

        return (await DataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.PricePerHour >= minPrice && parking.PricePerHour <= maxPrice).ToList();
    }
}
