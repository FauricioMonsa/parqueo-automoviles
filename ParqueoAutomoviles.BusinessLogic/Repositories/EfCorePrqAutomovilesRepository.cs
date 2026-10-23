using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class EfCorePrqAutomovilesRepository(ParqueoAutomovilesContext context) : IPrqAutomovilesRepository
{
    public Task<Prqautomovile?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        context.Prqautomoviles.AsNoTracking().FirstOrDefaultAsync(car => car.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Prqautomovile>> GetByColorAsync(string color, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(color);
        var pattern = $"%{color}%";
        return await context.Prqautomoviles.AsNoTracking()
            .Where(car => EF.Functions.ILike(car.Color, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByYearRangeAsync(int fromYear, int toYear, CancellationToken cancellationToken = default)
    {
        if (fromYear > toYear)
        {
            throw new ArgumentException("El año inicial no puede ser mayor que el año final.");
        }

        return await context.Prqautomoviles.AsNoTracking()
            .Where(car => car.Year >= fromYear && car.Year <= toYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manufacturer);
        var pattern = $"%{manufacturer}%";
        return await context.Prqautomoviles.AsNoTracking()
            .Where(car => EF.Functions.ILike(car.Manufacturer, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqautomovile>> GetByTypeAsync(PRQTipoAutomovil type, CancellationToken cancellationToken = default) =>
        await context.Prqautomoviles.AsNoTracking()
            .Where(car => car.Type == type)
            .ToListAsync(cancellationToken);
}
