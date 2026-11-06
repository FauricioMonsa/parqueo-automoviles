using ParqueoAutomoviles.BusinessLogic.Repositories;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.Core.Tests;

public sealed class JsonRepositoryCrudTests
{
    [Fact]
    public async Task JsonRepositories_AssignNextIdWhenInsertingZeroIdRecords()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, $"json-generated-ids-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQAutomoviles.json"),
            """[{"id":2,"color":"Azul","year":2024,"manufacturer":"Honda","type":"sedan"},{"id":7,"color":"Rojo","year":2023,"manufacturer":"Toyota","type":"moto"}]""");
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQParqueo.json"),
            """[{"id":4,"province_name":"San José","name":"Parqueo Central","price_per_hour":1250.00},{"id":9,"province_name":"Alajuela","name":"Parqueo Norte","price_per_hour":900.00}]""");
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQIngresoAutomoviles.json"),
            """[{"consecutive":3,"parking_id":4,"car_id":2,"entry_datetime":"2026-10-18T07:30:00","exit_datetime":"2026-10-18T09:15:00"},{"consecutive":8,"parking_id":9,"car_id":7,"entry_datetime":"2026-10-18T08:00:00","exit_datetime":null}]""");

        try
        {
            var repositories = PrqRepositoryFactory.FromJson(folder);
            var car = new Prqautomovile
            {
                Color = "Negro",
                Year = 2025,
                Manufacturer = "Nissan",
                Type = PRQTipoAutomovil.Sedan
            };
            var parking = new Prqparqueo
            {
                ProvinceName = "Heredia",
                Name = "Parqueo Nuevo",
                PricePerHour = 1000m
            };
            var entry = new PrqingresoAutomovile
            {
                ParkingId = 4,
                CarId = 2,
                EntryDatetime = new DateTime(2026, 10, 23, 8, 0, 0)
            };

            await repositories.Automoviles.InsertAsync(car);
            await repositories.Parqueos.InsertAsync(parking);
            await repositories.Ingresos.InsertAsync(entry);

            Assert.Equal(8, car.Id);
            Assert.Equal(10, parking.Id);
            Assert.Equal(9, entry.Consecutive);
            Assert.NotNull(await repositories.Automoviles.GetByIdAsync(8));
            Assert.NotNull(await repositories.Parqueos.GetByIdAsync(10));
            Assert.NotNull(await repositories.Ingresos.GetByIdAsync(9));
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task JsonIngresos_GetByCarIdReturnsOnlyMatchingEntriesWithDetails()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, $"json-car-entries-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQAutomoviles.json"),
            """[{"id":1,"color":"Azul","year":2024,"manufacturer":"Honda","type":"sedan"},{"id":2,"color":"Rojo","year":2023,"manufacturer":"Toyota","type":"moto"}]""");
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQParqueo.json"),
            """[{"id":10,"province_name":"San José","name":"Parqueo Central","price_per_hour":1250.00}]""");
        await File.WriteAllTextAsync(Path.Combine(folder, "PRQIngresoAutomoviles.json"),
            """[{"consecutive":1,"parking_id":10,"car_id":1,"entry_datetime":"2026-10-18T07:30:00","exit_datetime":"2026-10-18T09:15:00"},{"consecutive":2,"parking_id":10,"car_id":2,"entry_datetime":"2026-10-18T08:00:00","exit_datetime":"2026-10-18T09:00:00"}]""");

        try
        {
            var repository = PrqRepositoryFactory.FromJson(folder).Ingresos;

            var entries = await repository.GetByCarIdAsync(1);

            var entry = Assert.Single(entries);
            Assert.Equal(1, entry.CarId);
            Assert.Equal(10, entry.Parking.Id);
            Assert.Equal(1250m, entry.Parking.PricePerHour);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

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
