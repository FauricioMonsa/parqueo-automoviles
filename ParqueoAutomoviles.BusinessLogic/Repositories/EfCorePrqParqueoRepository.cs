using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class EfCorePrqParqueoRepository(ParqueoAutomovilesContext context) : IPrqParqueoRepository
{
    public Task<Prqparqueo?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        context.Prqparqueos.AsNoTracking().FirstOrDefaultAsync(parking => parking.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Prqparqueo>> GetByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provinceName);
        var pattern = $"%{provinceName}%";
        return await context.Prqparqueos.AsNoTracking()
            .Where(parking => EF.Functions.ILike(parking.ProvinceName, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        var pattern = $"%{name}%";
        return await context.Prqparqueos.AsNoTracking()
            .Where(parking => EF.Functions.ILike(parking.Name, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        if (minPrice > maxPrice)
        {
            throw new ArgumentException("El precio mínimo no puede ser mayor que el precio máximo.");
        }

        return await context.Prqparqueos.AsNoTracking()
            .Where(parking => parking.PricePerHour >= minPrice && parking.PricePerHour <= maxPrice)
            .ToListAsync(cancellationToken);
    }
}
