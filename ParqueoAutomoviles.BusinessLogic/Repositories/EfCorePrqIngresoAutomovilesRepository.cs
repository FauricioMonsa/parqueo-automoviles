using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class EfCorePrqIngresoAutomovilesRepository(ParqueoAutomovilesContext context)
    : EfCoreRepository<PrqingresoAutomovile>(context), IPrqIngresoAutomovilesRepository
{
    public Task<decimal?> GetPricePerHourByParkingAsync(long parkingId, CancellationToken cancellationToken = default) =>
        Context.Prqparqueos.AsNoTracking()
            .Where(parking => parking.Id == parkingId)
            .Select(parking => (decimal?)parking.PricePerHour)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PrqingresoAutomovileCustomAttributes?> GetByIdWithDetailsAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var entry = await QueryEntries()
            .FirstOrDefaultAsync(item => item.Consecutive == id, cancellationToken);
        return entry is null ? null : ToCustomAttributes(entry);
    }

    public async Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetAllAsync(
        PRQTipoAutomovil? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOptionalDateRange(fromDate, toDate);
        var query = QueryEntries();
        if (type.HasValue)
        {
            query = query.Where(entry => entry.Car.Type == type.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(entry => entry.EntryDatetime >= fromDate.Value && entry.EntryDatetime <= toDate!.Value);
        }

        if (!string.IsNullOrWhiteSpace(provinceName))
        {
            query = query.Where(entry => EF.Functions.ILike(entry.Parking.ProvinceName, $"%{provinceName}%"));
        }

        var entries = await query.OrderBy(entry => entry.EntryDatetime).ToListAsync(cancellationToken);
        return entries.Select(ToCustomAttributes).ToList();
    }

    public async Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetByTypeAndDateRangeAsync(
        PRQTipoAutomovil type,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        return await GetAllAsync(type, from, to, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetByProvinceAndDateRangeAsync(
        string provinceName,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        ArgumentNullException.ThrowIfNull(provinceName);
        return await GetAllAsync(fromDate: from, toDate: to, provinceName: provinceName, cancellationToken: cancellationToken);
    }

    private IQueryable<PrqingresoAutomovile> QueryEntries() =>
        Context.PrqingresoAutomoviles.AsNoTracking()
            .Include(entry => entry.Car)
            .Include(entry => entry.Parking);

    private static PrqingresoAutomovileCustomAttributes ToCustomAttributes(PrqingresoAutomovile entry) => new()
    {
        Consecutive = entry.Consecutive,
        ParkingId = entry.ParkingId,
        CarId = entry.CarId,
        EntryDatetime = entry.EntryDatetime,
        ExitDatetime = entry.ExitDatetime,
        Car = entry.Car,
        Parking = entry.Parking
    };

    private static void ValidateOptionalDateRange(DateTime? from, DateTime? to)
    {
        if (from.HasValue != to.HasValue)
        {
            throw new ArgumentException("Debe indicar tanto la fecha inicial como la fecha final.");
        }

        if (from.HasValue)
        {
            ValidateDateRange(from.Value, to!.Value);
        }
    }

    private static void ValidateDateRange(DateTime from, DateTime to)
    {
        if (from > to)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");
        }
    }
}
