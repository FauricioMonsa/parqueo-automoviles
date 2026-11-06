using Microsoft.EntityFrameworkCore;
using Npgsql;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Repositories;
using ParqueoAutomoviles.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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

        var npgsqlDataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        npgsqlDataSourceBuilder.MapEnum<PRQTipoAutomovil>("user_08.PRQTipoAutomovil");
        var npgsqlDataSource = npgsqlDataSourceBuilder.Build();
        builder.Services.AddSingleton(npgsqlDataSource);
        builder.Services.AddDbContext<ParqueoAutomovilesContext>((serviceProvider, options) =>
            options.UseNpgsql(
                serviceProvider.GetRequiredService<NpgsqlDataSource>(),
                npgsqlOptions => npgsqlOptions.MapEnum<PRQTipoAutomovil>("user_08.PRQTipoAutomovil")));
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Automoviles}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
