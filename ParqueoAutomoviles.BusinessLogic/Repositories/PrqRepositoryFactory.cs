using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Json;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Repositories;

public static class PrqRepositoryFactory
{
    public static PrqRepositorySet FromEfCore(ParqueoAutomovilesContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new PrqRepositorySet(
            new EfCorePrqAutomovilesRepository(context),
            new EfCorePrqParqueoRepository(context),
            new EfCorePrqIngresoAutomovilesRepository(context));
    }

    public static PrqRepositorySet FromJson(string databaseFolder)
    {
        var dataSource = new JsonPrqDataSource(databaseFolder);
        return new PrqRepositorySet(
            new JsonPrqAutomovilesRepository(dataSource),
            new JsonPrqParqueoRepository(dataSource),
            new JsonPrqIngresoAutomovilesRepository(dataSource));
    }
}
