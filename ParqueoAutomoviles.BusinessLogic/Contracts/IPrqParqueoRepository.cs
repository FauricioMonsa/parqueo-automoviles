using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Contracts;

public interface IPrqParqueoRepository
{
    Task<Prqparqueo?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqparqueo>> GetByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqparqueo>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqparqueo>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);
}
