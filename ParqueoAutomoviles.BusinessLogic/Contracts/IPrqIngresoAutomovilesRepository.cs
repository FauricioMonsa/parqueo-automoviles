using ParqueoAutomoviles.BusinessLogic.Models;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Contracts;

public interface IPrqIngresoAutomovilesRepository
{
    Task<PrqingresoAutomovile?> GetByIdAsync(long consecutive, CancellationToken cancellationToken = default);
    Task<decimal?> GetPricePerHourByParkingAsync(long parkingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomobileParkingEntryReport>> GetByTypeAndDateRangeAsync(PRQTipoAutomovil type, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomobileParkingEntryReport>> GetByProvinceAndDateRangeAsync(string provinceName, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
