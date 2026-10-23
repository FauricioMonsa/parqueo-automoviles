using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Models;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class EfCorePrqIngresoAutomovilesRepository(ParqueoAutomovilesContext context) : IPrqIngresoAutomovilesRepository
{
    public Task<PrqingresoAutomovile?> GetByIdAsync(long consecutive, CancellationToken cancellationToken = default) =>
        context.PrqingresoAutomoviles.AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.Consecutive == consecutive, cancellationToken);

    public Task<decimal?> GetPricePerHourByParkingAsync(long parkingId, CancellationToken cancellationToken = default) =>
        context.Prqparqueos.AsNoTracking()
            .Where(parking => parking.Id == parkingId)
            .Select(parking => (decimal?)parking.PricePerHour)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AutomobileParkingEntryReport>> GetByTypeAndDateRangeAsync(
        PRQTipoAutomovil type,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var entries = await context.PrqingresoAutomoviles.AsNoTracking()
            .Include(entry => entry.Car)
            .Include(entry => entry.Parking)
            .Where(entry => entry.Car.Type == type && entry.EntryDatetime >= from && entry.EntryDatetime <= to)
            .OrderBy(entry => entry.EntryDatetime)
            .ToListAsync(cancellationToken);

        return await BuildReportsAsync(entries, cancellationToken);
    }

    public async Task<IReadOnlyList<AutomobileParkingEntryReport>> GetByProvinceAndDateRangeAsync(
        string provinceName,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provinceName);
        ValidateDateRange(from, to);
        var provincePattern = $"%{provinceName}%";
        var entries = await context.PrqingresoAutomoviles.AsNoTracking()
            .Include(entry => entry.Car)
            .Include(entry => entry.Parking)
            .Where(entry => EF.Functions.ILike(entry.Parking.ProvinceName, provincePattern)
                && entry.EntryDatetime >= from && entry.EntryDatetime <= to)
            .OrderBy(entry => entry.EntryDatetime)
            .ToListAsync(cancellationToken);

        return await BuildReportsAsync(entries, cancellationToken);
    }

    private async Task<IReadOnlyList<AutomobileParkingEntryReport>> BuildReportsAsync(
        IReadOnlyList<PrqingresoAutomovile> entries,
        CancellationToken cancellationToken)
    {
        var reports = new List<AutomobileParkingEntryReport>(entries.Count);
        foreach (var entry in entries)
        {
            var pricePerHour = await GetPricePerHourByParkingAsync(entry.ParkingId, cancellationToken);
            var amountToPay = entry.ExitDatetime is null || pricePerHour is null
                ? null
                : (decimal?)(decimal)(entry.ExitDatetime.Value - entry.EntryDatetime).TotalHours * pricePerHour.Value;

            reports.Add(new AutomobileParkingEntryReport(
                entry.Consecutive,
                entry.CarId,
                entry.Car.Color,
                entry.Car.Year,
                entry.Car.Manufacturer,
                entry.Car.Type,
                entry.ParkingId,
                entry.Parking.Name,
                entry.Parking.ProvinceName,
                entry.EntryDatetime,
                entry.ExitDatetime,
                amountToPay));
        }

        return reports;
    }

    private static void ValidateDateRange(DateTime from, DateTime to)
    {
        if (from > to)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");
        }
    }
}
