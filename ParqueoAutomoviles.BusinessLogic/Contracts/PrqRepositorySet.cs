namespace ParqueoAutomoviles.BusinessLogic.Contracts;

public sealed record PrqRepositorySet(
    IPrqAutomovilesRepository Automoviles,
    IPrqParqueoRepository Parqueos,
    IPrqIngresoAutomovilesRepository Ingresos);
