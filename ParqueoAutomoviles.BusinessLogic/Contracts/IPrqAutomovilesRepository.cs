using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Contracts;

public interface IPrqAutomovilesRepository
{
    Task<Prqautomovile?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqautomovile>> GetByColorAsync(string color, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqautomovile>> GetByYearRangeAsync(int fromYear, int toYear, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqautomovile>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prqautomovile>> GetByTypeAsync(PRQTipoAutomovil type, CancellationToken cancellationToken = default);
}
