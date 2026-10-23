using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public abstract class EfCoreRepository<TEntity>(ParqueoAutomovilesContext context) : IRepository<TEntity>
    where TEntity : class
{
    protected ParqueoAutomovilesContext Context { get; } = context;

    public async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        await Context.Set<TEntity>().FindAsync(new object?[] { id }, cancellationToken);

    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Context.Set<TEntity>().Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<TEntity>().FindAsync(new object?[] { id }, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe {typeof(TEntity).Name} con clave {id}.");

        Context.Set<TEntity>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
