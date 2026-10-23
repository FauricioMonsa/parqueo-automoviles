using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
using ParqueoAutomoviles.BusinessLogic.Models;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class JsonPrqIngresoAutomovilesRepository(JsonPrqDataSource dataSource)
    : JsonRepositoryBase<PrqingresoAutomovile>(dataSource, "PRQIngresoAutomoviles.json", entry => entry.Consecutive), IPrqIngresoAutomovilesRepository
{
    public async Task<decimal?> GetPricePerHourByParkingAsync(long parkingId, CancellationToken cancellationToken = default) =>
        (await DataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.Id == parkingId)
            .Select(parking => (decimal?)parking.PricePerHour)
            .FirstOrDefault();

    public async Task<IReadOnlyList<AutomobileParkingEntryReport>> GetByTypeAndDateRangeAsync(
        PRQTipoAutomovil type,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(from, to);
        var cars = await DataSource.LoadCarsAsync(cancellationToken);
        var matchingCarIds = cars.Where(car => car.Type == type).Select(car => car.Id).ToHashSet();
        var entries = (await DataSource.LoadEntriesAsync(cancellationToken))
            .Where(entry => matchingCarIds.Contains(entry.CarId) && entry.EntryDatetime >= from && entry.EntryDatetime <= to)
            .OrderBy(entry => entry.EntryDatetime)
            .ToList();
        return await BuildReportsAsync(entries, cars, cancellationToken);
    }

    public async Task<IReadOnlyList<AutomobileParkingEntryReport>> GetByProvinceAndDateRangeAsync(
        string provinceName,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provinceName);
        ValidateDateRange(from, to);
        var provinceParkingIds = (await DataSource.LoadParkingsAsync(cancellationToken))
            .Where(parking => parking.ProvinceName.Contains(provinceName, StringComparison.OrdinalIgnoreCase))
            .Select(parking => parking.Id)
            .ToHashSet();
        var cars = await DataSource.LoadCarsAsync(cancellationToken);
        var entries = (await DataSource.LoadEntriesAsync(cancellationToken))
            .Where(entry => provinceParkingIds.Contains(entry.ParkingId) && entry.EntryDatetime >= from && entry.EntryDatetime <= to)
            .OrderBy(entry => entry.EntryDatetime)
            .ToList();
        return await BuildReportsAsync(entries, cars, cancellationToken);
    }

    private async Task<IReadOnlyList<AutomobileParkingEntryReport>> BuildReportsAsync(
        IReadOnlyList<PrqingresoAutomovile> entries,
        IReadOnlyList<Prqautomovile> cars,
        CancellationToken cancellationToken)
    {
        var parkings = await DataSource.LoadParkingsAsync(cancellationToken);
        var carsById = cars.ToDictionary(car => car.Id);
        var parkingsById = parkings.ToDictionary(parking => parking.Id);
        var reports = new List<AutomobileParkingEntryReport>(entries.Count);

        foreach (var entry in entries)
        {
            if (!carsById.TryGetValue(entry.CarId, out var car)
                || !parkingsById.TryGetValue(entry.ParkingId, out var parking))
            {
                continue;
            }

            var pricePerHour = await GetPricePerHourByParkingAsync(entry.ParkingId, cancellationToken);
            var amountToPay = entry.ExitDatetime is null || pricePerHour is null
                ? null
                : (decimal?)(decimal)(entry.ExitDatetime.Value - entry.EntryDatetime).TotalHours * pricePerHour.Value;

            reports.Add(new AutomobileParkingEntryReport(
                entry.Consecutive,
                entry.CarId,
                car.Color,
                car.Year,
                car.Manufacturer,
                car.Type,
                entry.ParkingId,
                parking.Name,
                parking.ProvinceName,
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
