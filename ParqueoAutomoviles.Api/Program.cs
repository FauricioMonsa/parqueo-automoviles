using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Repositories;
using ParqueoAutomoviles.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var dataSource = builder.Configuration["DataSource"] ?? "EfCore";

switch (dataSource)
{
    case "EfCore":
        var connectionString = builder.Configuration.GetConnectionString("MySecretPostgresConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Falta configurar ConnectionStrings:MySecretPostgresConnection en los secretos del usuario.");
        }

        builder.Services.AddDbContext<ParqueoAutomovilesContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddScoped<PrqRepositorySet>(serviceProvider =>
            PrqRepositoryFactory.FromEfCore(serviceProvider.GetRequiredService<ParqueoAutomovilesContext>()));
        break;
    case "Json":
        var databaseFolder = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "database"));
        builder.Services.AddScoped<PrqRepositorySet>(_ => PrqRepositoryFactory.FromJson(databaseFolder));
        break;
    default:
        throw new InvalidOperationException("La configuración DataSource debe ser 'EfCore' o 'Json'.");
}

builder.Services.AddScoped<IPrqAutomovilesRepository>(serviceProvider =>
    serviceProvider.GetRequiredService<PrqRepositorySet>().Automoviles);
builder.Services.AddScoped<IPrqParqueoRepository>(serviceProvider =>
    serviceProvider.GetRequiredService<PrqRepositorySet>().Parqueos);
builder.Services.AddScoped<IPrqIngresoAutomovilesRepository>(serviceProvider =>
    serviceProvider.GetRequiredService<PrqRepositorySet>().Ingresos);

var app = builder.Build();

app.MapControllers();

app.Run();
