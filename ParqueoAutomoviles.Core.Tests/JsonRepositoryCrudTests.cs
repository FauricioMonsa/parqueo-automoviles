using ParqueoAutomoviles.BusinessLogic.Repositories;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.Core.Tests;

public sealed class JsonRepositoryCrudTests
{
    [Fact]
    public async Task AllJsonRepositories_InsertUpdateAndDeleteByPrimaryKey()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, $"json-crud-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQAutomoviles.json"), "[]");
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQParqueo.json"), "[]");
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQIngresoAutomoviles.json"), "[]");

        try
        {
            var repositories = PrqRepositoryFactory.FromJson(folder);

            var car = new Prqautomovile
            {
                Id = 1,
                Color = "Azul",
                Year = 2024,
                Manufacturer = "Honda",
                Type = PRQTipoAutomovil.Sedan
            };
            await repositories.Automoviles.InsertAsync(car);
            car.Color = "Rojo";
            await repositories.Automoviles.UpdateAsync(car);
            Assert.Equal("Rojo", (await repositories.Automoviles.GetByIdAsync(1))!.Color);
            await repositories.Automoviles.DeleteAsync(1);
            Assert.Null(await repositories.Automoviles.GetByIdAsync(1));

            var parking = new Prqparqueo
            {
                Id = 2,
                ProvinceName = "San José",
                Name = "Parqueo Prueba",
                PricePerHour = 1000m
            };
            await repositories.Parqueos.InsertAsync(parking);
            parking.PricePerHour = 1200m;
            await repositories.Parqueos.UpdateAsync(parking);
            Assert.Equal(1200m, (await repositories.Parqueos.GetByIdAsync(2))!.PricePerHour);
            await repositories.Parqueos.DeleteAsync(2);
            Assert.Null(await repositories.Parqueos.GetByIdAsync(2));

            var entry = new PrqingresoAutomovile
            {
                Consecutive = 3,
                ParkingId = 2,
                CarId = 1,
                EntryDatetime = new DateTime(2026, 10, 23, 8, 0, 0),
                ExitDatetime = null
            };
            await repositories.Ingresos.InsertAsync(entry);
            entry.ExitDatetime = new DateTime(2026, 10, 23, 9, 0, 0);
            await repositories.Ingresos.UpdateAsync(entry);
            Assert.Equal(entry.ExitDatetime, (await repositories.Ingresos.GetByIdAsync(3))!.ExitDatetime);
            await repositories.Ingresos.DeleteAsync(3);
            Assert.Null(await repositories.Ingresos.GetByIdAsync(3));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }
}
