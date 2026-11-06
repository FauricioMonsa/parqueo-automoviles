using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
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

    public async Task<PrqingresoAutomovileCustomAttributes?> GetByIdWithDetailsAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var records = await LoadDetailedEntriesAsync(cancellationToken);
        return records.FirstOrDefault(entry => entry.Consecutive == id);
    }

    public async Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetByCarIdAsync(
        long carId,
        CancellationToken cancellationToken = default) =>
        (await LoadDetailedEntriesAsync(cancellationToken))
            .Where(entry => entry.CarId == carId)
            .OrderBy(entry => entry.EntryDatetime)
            .ToList();

    public async Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> GetAllAsync(
        PRQTipoAutomovil? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? provinceName = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOptionalDateRange(fromDate, toDate);
        var entries = await LoadDetailedEntriesAsync(cancellationToken);
        return entries.Where(entry =>
                (!type.HasValue || entry.Car.Type == type.Value) &&
                (!fromDate.HasValue || (entry.EntryDatetime >= fromDate.Value && entry.EntryDatetime <= toDate!.Value)) &&
                (string.IsNullOrWhiteSpace(provinceName) || entry.Parking.ProvinceName.Contains(provinceName, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(entry => entry.EntryDatetime)
            .ToList();
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
        ArgumentNullException.ThrowIfNull(provinceName);
        ValidateDateRange(from, to);
        return await GetAllAsync(fromDate: from, toDate: to, provinceName: provinceName, cancellationToken: cancellationToken);
    }

    private async Task<IReadOnlyList<PrqingresoAutomovileCustomAttributes>> LoadDetailedEntriesAsync(
        CancellationToken cancellationToken)
    {
        var entries = await DataSource.LoadEntriesAsync(cancellationToken);
        var cars = await DataSource.LoadCarsAsync(cancellationToken);
        var parkings = await DataSource.LoadParkingsAsync(cancellationToken);
        var carsById = cars.ToDictionary(car => car.Id);
        var parkingsById = parkings.ToDictionary(parking => parking.Id);
        var detailedEntries = new List<PrqingresoAutomovileCustomAttributes>(entries.Count);

        foreach (var entry in entries)
        {
            if (!carsById.TryGetValue(entry.CarId, out var car)
                || !parkingsById.TryGetValue(entry.ParkingId, out var parking))
            {
                continue;
            }

            detailedEntries.Add(new PrqingresoAutomovileCustomAttributes
            {
                Consecutive = entry.Consecutive,
                ParkingId = entry.ParkingId,
                CarId = entry.CarId,
                EntryDatetime = entry.EntryDatetime,
                ExitDatetime = entry.ExitDatetime,
                Car = car,
                Parking = parking
            });
        }

        return detailedEntries;
    }

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
