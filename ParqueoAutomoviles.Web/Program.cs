using Microsoft.EntityFrameworkCore;
using Npgsql;
using ParqueoAutomoviles.BusinessLogic.Contracts;
using ParqueoAutomoviles.BusinessLogic.Repositories;
using ParqueoAutomoviles.Model;
using ParqueoAutomoviles.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var defaultDataSource = builder.Configuration["DataSource"] ?? "EfCore";
var automobileDataSource = ReadDataSource(builder.Configuration["DataSources:Automoviles"], "DataSources:Automoviles", defaultDataSource);
var parkingDataSource = ReadDataSource(builder.Configuration["DataSources:Parqueos"], "DataSources:Parqueos", defaultDataSource);
var entryDataSource = ReadDataSource(builder.Configuration["DataSources:Ingresos"], "DataSources:Ingresos", defaultDataSource);
var usesEfCore = automobileDataSource == "EfCore"
    || parkingDataSource == "EfCore"
    || entryDataSource == "EfCore";
var usesJson = automobileDataSource == "Json"
    || parkingDataSource == "Json"
    || entryDataSource == "Json";

if (usesEfCore)
{
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
}

var jsonDatabaseFolder = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["JsonDatabaseFolder"] ?? "..\\database"));

builder.Services.AddScoped<ConfiguredPrqRepositorySets>(serviceProvider =>
    new ConfiguredPrqRepositorySets(
        usesEfCore
            ? PrqRepositoryFactory.FromEfCore(serviceProvider.GetRequiredService<ParqueoAutomovilesContext>())
            : null,
        usesJson
            ? PrqRepositoryFactory.FromJson(jsonDatabaseFolder)
            : null));

builder.Services.AddScoped<IPrqAutomovilesRepository>(serviceProvider =>
    serviceProvider.GetRequiredService<ConfiguredPrqRepositorySets>().GetAutomoviles(automobileDataSource));
builder.Services.AddScoped<IPrqParqueoRepository>(serviceProvider =>
    serviceProvider.GetRequiredService<ConfiguredPrqRepositorySets>().GetParqueos(parkingDataSource));
builder.Services.AddScoped<IPrqIngresoAutomovilesRepository>(serviceProvider =>
    serviceProvider.GetRequiredService<ConfiguredPrqRepositorySets>().GetIngresos(entryDataSource));

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

static string ReadDataSource(string? configuredValue, string configurationKey, string fallback)
{
    var value = configuredValue ?? fallback;
    return value.Trim().ToUpperInvariant() switch
    {
        "EFCORE" => "EfCore",
        "JSON" => "Json",
        _ => throw new InvalidOperationException(
            $"La configuración {configurationKey} debe ser 'EfCore' o 'Json'.")
    };
}
