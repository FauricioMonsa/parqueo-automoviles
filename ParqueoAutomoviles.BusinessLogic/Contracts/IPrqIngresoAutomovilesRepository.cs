using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Contracts;

public interface IPrqIngresoAutomovilesRepository : IRepository<PrqingresoAutomovile>
{
    Task<decimal?> GetPricePerHourByParkingAsync(long parkingId, CancellationToken cancellationToken = default);
    Task<PrqingresoAutomovileCustomAttributes?> GetByIdWithDetailsAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetAllAsync(
        PRQTipoAutomovil? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetByTypeAndDateRangeAsync(PRQTipoAutomovil type, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetByProvinceAndDateRangeAsync(string provinceName, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
