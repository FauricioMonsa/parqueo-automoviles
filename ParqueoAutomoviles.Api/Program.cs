using Microsoft.EntityFrameworkCore;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Repositories;
using ParqueoAutomoviles.Model;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.Run();
