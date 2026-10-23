using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public sealed class EfCorePrqParqueoRepository(ParqueoAutomovilesContext context)
    : EfCoreRepository<Prqparqueo>(context), IPrqParqueoRepository
{
    public async Task<IReadOnlyList<Prqparqueo>> GetByProvinceNameAsync(string provinceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provinceName);
        var pattern = $"%{provinceName}%";
        return await Context.Prqparqueos.AsNoTracking()
            .Where(parking => EF.Functions.ILike(parking.ProvinceName, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        var pattern = $"%{name}%";
        return await Context.Prqparqueos.AsNoTracking()
            .Where(parking => EF.Functions.ILike(parking.Name, pattern))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Prqparqueo>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        if (minPrice > maxPrice)
        {
            throw new ArgumentException("El precio mínimo no puede ser mayor que el precio máximo.");
        }

        return await Context.Prqparqueos.AsNoTracking()
            .Where(parking => parking.PricePerHour >= minPrice && parking.PricePerHour <= maxPrice)
            .ToListAsync(cancellationToken);
    }
}
