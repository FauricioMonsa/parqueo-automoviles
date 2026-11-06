using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Repositories;

namespace ParqueoAutomoviles.Web;

internal sealed class ConfiguredPrqRepositorySets(
    PrqRepositorySet? efCore,
    PrqRepositorySet? json)
{
    public IPrqAutomovilesRepository GetAutomoviles(string dataSource) =>
        GetSet(dataSource).Automoviles;

    public IPrqParqueoRepository GetParqueos(string dataSource) =>
        GetSet(dataSource).Parqueos;

    public IPrqIngresoAutomovilesRepository GetIngresos(string dataSource) =>
        GetSet(dataSource).Ingresos;

    private PrqRepositorySet GetSet(string dataSource) =>
        dataSource switch
        {
            "EfCore" => efCore ?? throw new InvalidOperationException("El conjunto de repositorios EF Core no está registrado."),
            "Json" => json ?? throw new InvalidOperationException("El conjunto de repositorios JSON no está registrado."),
            _ => throw new InvalidOperationException($"Origen de datos no admitido: {dataSource}.")
        };
}
