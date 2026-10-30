using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class EfCorePrqAutomovilesRepository(ParqueoAutomovilesContext context)
    : EfCoreRepository<Prqautomovile>(context), IPrqAutomovilesRepository
{
    public async Task<IReadOnlyList<Prqautomovile>> GetAllAsync(
        string? color = null,
        int? fromYear = null,
        int? toYear = null,
        string? manufacturer = null,
        PRQTipoAutomovil? type = null,
        CancellationToken cancellationToken = default)
    {
        if (fromYear.HasValue != toYear.HasValue || (fromYear.HasValue && fromYear > toYear))
        {
            throw new ArgumentException("Debe indicar ambos años y el año inicial no puede superar al final.");
        }

        var query = Context.Prqautomoviles.AsNoTracking().Include(car => car.PrqingresoAutomoviles).AsQueryable();
        if (!string.IsNullOrWhiteSpace(color))
        {
            query = query.Where(car => EF.Functions.ILike(car.Color, $"%{color}%"));
        }

        if (fromYear.HasValue)
        {
            query = query.Where(car => car.Year >= fromYear.Value && car.Year <= toYear!.Value);
        }

        if (!string.IsNullOrWhiteSpace(manufacturer))
        {
            query = query.Where(car => EF.Functions.ILike(car.Manufacturer, $"%{manufacturer}%"));
        }

        if (type.HasValue)
        {
            query = query.Where(car => car.Type == type.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(color);
        var pattern = $"%{color}%";
        return await Context.Prqautomoviles.AsNoTracking()
            .Where(car => EF.Functions.ILike(car.Color, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByYearRangeAsync(int fromYear, int toYear, CancellationToken cancellationToken = default)
    {
        if (fromYear > toYear)
        {
            throw new ArgumentException("El año inicial no puede ser mayor que el año final.");
        }

        return await Context.Prqautomoviles.AsNoTracking()
            .Where(car => car.Year >= fromYear && car.Year <= toYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manufacturer);
        var pattern = $"%{manufacturer}%";
        return await Context.Prqautomoviles.AsNoTracking()
            .Where(car => EF.Functions.ILike(car.Manufacturer, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByTypeAsync(PRQTipoAutomovil type, CancellationToken cancellationToken = default) =>
        await Context.Prqautomoviles.AsNoTracking()
            .Where(car => car.Type == type)
            .ToListAsync(cancellationToken);
}
