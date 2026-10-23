using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public abstract class JsonRepositoryBase<TEntity>(
    JsonPrqDataSource dataSource,
    string fileName,
    Func<TEntity, long> keySelector) : IRepository<TEntity>
    where TEntity : class
{
    protected JsonPrqDataSource DataSource { get; } = dataSource;

    public async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        (await DataSource.ReadRecordsAsync<TEntity>(fileName, cancellationToken))
            .FirstOrDefault(entity => keySelector(entity) == id);

    public Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        DataSource.InsertRecordAsync(fileName, entity, keySelector, cancellationToken);

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        DataSource.UpdateRecordAsync(fileName, entity, keySelector, cancellationToken);

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default) =>
        DataSource.DeleteRecordAsync(fileName, id, keySelector, cancellationToken);
}
