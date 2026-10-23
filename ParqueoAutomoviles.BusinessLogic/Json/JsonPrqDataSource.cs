using System.Text.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Json;

public sealed class JsonPrqDataSource
{
    private readonly string _databaseFolder;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonPrqDataSource(string databaseFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFolder);
        _databaseFolder = Path.GetFullPath(databaseFolder);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        _jsonOptions.Converters.Add(new PRQTipoAutomovilJsonConverter());
    }

    public async Task<IReadOnlyList<Prqautomovile>> LoadCarsAsync(CancellationToken cancellationToken = default) =>
        await ReadAsync<Prqautomovile[]>("PRQAutomoviles.json", cancellationToken);

    public async Task<IReadOnlyList<Prqparqueo>> LoadParkingsAsync(CancellationToken cancellationToken = default) =>
        await ReadAsync<Prqparqueo[]>("PRQParqueo.json", cancellationToken);

    public async Task<IReadOnlyList<PrqingresoAutomovile>> LoadEntriesAsync(CancellationToken cancellationToken = default) =>
        await ReadAsync<PrqingresoAutomovile[]>("PRQIngresoAutomoviles.json", cancellationToken);

    private async Task<T> ReadAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(_databaseFolder, fileName);
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken)
            ?? throw new JsonException($"El archivo {fileName} no contiene datos JSON válidos.");
    }
}
