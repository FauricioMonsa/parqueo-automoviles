using System.Text.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Json;

public sealed class JsonPrqDataSource
{
    private readonly string _databaseFolder;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonPrqDataSource(string databaseFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFolder);
        _databaseFolder = Path.GetFullPath(databaseFolder);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
        _jsonOptions.Converters.Add(new PRQTipoAutomovilJsonConverter());
    }

    public async Task<IReadOnlyList<Prqautomovile>> LoadCarsAsync(CancellationToken cancellationToken = default) =>
        await ReadRecordsAsync<Prqautomovile>("PRQAutomoviles.json", cancellationToken);

    public async Task<IReadOnlyList<Prqparqueo>> LoadParkingsAsync(CancellationToken cancellationToken = default) =>
        await ReadRecordsAsync<Prqparqueo>("PRQParqueo.json", cancellationToken);

    public async Task<IReadOnlyList<PrqingresoAutomovile>> LoadEntriesAsync(CancellationToken cancellationToken = default) =>
        await ReadRecordsAsync<PrqingresoAutomovile>("PRQIngresoAutomoviles.json", cancellationToken);

    internal async Task<IReadOnlyList<TEntity>> ReadRecordsAsync<TEntity>(string fileName, CancellationToken cancellationToken)
    {
        var records = await ReadAsync<TEntity[]>(fileName, cancellationToken);
        return records;
    }

    internal Task InsertRecordAsync<TEntity>(
        string fileName,
        TEntity entity,
        Func<TEntity, long> keySelector,
        CancellationToken cancellationToken) where TEntity : class =>
        MutateRecordsAsync<TEntity>(fileName, records =>
        {
            if (records.Any(record => keySelector(record) == keySelector(entity)))
            {
                throw new InvalidOperationException($"Ya existe un registro con clave {keySelector(entity)} en {fileName}.");
            }

            records.Add(entity);
        }, cancellationToken);

    internal Task UpdateRecordAsync<TEntity>(
        string fileName,
        TEntity entity,
        Func<TEntity, long> keySelector,
        CancellationToken cancellationToken) where TEntity : class =>
        MutateRecordsAsync<TEntity>(fileName, records =>
        {
            var index = records.FindIndex(record => keySelector(record) == keySelector(entity));
            if (index < 0)
            {
                throw new KeyNotFoundException($"No existe un registro con clave {keySelector(entity)} en {fileName}.");
            }

            records[index] = entity;
        }, cancellationToken);

    internal Task DeleteRecordAsync<TEntity>(
        string fileName,
        long id,
        Func<TEntity, long> keySelector,
        CancellationToken cancellationToken) where TEntity : class =>
        MutateRecordsAsync<TEntity>(fileName, records =>
        {
            var index = records.FindIndex(record => keySelector(record) == id);
            if (index < 0)
            {
                throw new KeyNotFoundException($"No existe un registro con clave {id} en {fileName}.");
            }

            records.RemoveAt(index);
        }, cancellationToken);

    private async Task MutateRecordsAsync<TEntity>(
        string fileName,
        Action<List<TEntity>> mutation,
        CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);
        string? temporaryPath = null;
        try
        {
            var records = (await ReadAsync<TEntity[]>(fileName, cancellationToken)).ToList();
            mutation(records);

            var path = Path.Combine(_databaseFolder, fileName);
            temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, records, _jsonOptions, cancellationToken);
            }

            File.Move(temporaryPath, path, overwrite: true);
            temporaryPath = null;
        }
        finally
        {
            if (temporaryPath is not null && File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            _fileLock.Release();
        }
    }

    private async Task<T> ReadAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_databaseFolder, fileName);
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken)
            ?? throw new JsonException($"El archivo {fileName} no contiene datos JSON válidos.");
    }
}
