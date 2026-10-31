using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Contracts;

public interface IPrqParqueoRepository : IRepository<Prqparqueo>
{
    Task<IReadOnlyList<Prqparqueo>> GetAllAsync(
        string? provinceName = null,
        string? name = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqparqueo>> GetByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqparqueo>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqparqueo>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);
}
