-- Reinicia los datos de prueba y las secuencias para obtener IDs reproducibles.
TRUNCATE TABLE
    "PRQIngresoAutomoviles",
    "PRQParqueo",
    "PRQAutomoviles"
RESTART IDENTITY;

INSERT INTO "PRQAutomoviles" (color, year, manufacturer, type) VALUES
    ('Rojo', 2020, 'Toyota', 'sedan'),
    ('Azul', 2022, 'Hyundai', 'sedan'),
    ('Negro', 2019, 'Suzuki', '4x4'),
    ('Blanco', 2023, 'Honda', 'moto'),
    ('Gris', 2021, 'Nissan', '4x4');

INSERT INTO "PRQParqueo" (province_name, name, price_per_hour) VALUES
    ('San José', 'Parqueo Central', 1250.00),
    ('Alajuela', 'Parqueo Campus Norte', 900.00);

INSERT INTO "PRQIngresoAutomoviles"
    (parking_id, car_id, entry_datetime, exit_datetime) VALUES
    (1, 1, '2026-10-18 07:30:00', '2026-10-18 09:15:00'),
    (1, 2, '2026-10-18 08:00:00', '2026-10-18 10:30:00'),
    (2, 3, '2026-10-18 08:10:00', '2026-10-18 12:00:00'),
    (2, 4, '2026-10-18 09:00:00', '2026-10-18 09:45:00'),
    (1, 5, '2026-10-18 09:20:00', '2026-10-18 11:50:00'),
    (2, 1, '2026-10-18 13:00:00', '2026-10-18 15:10:00'),
    (1, 3, '2026-10-18 13:15:00', '2026-10-18 16:40:00'),
    (2, 2, '2026-10-18 14:00:00', '2026-10-18 17:20:00'),
    (1, 4, '2026-10-19 07:40:00', '2026-10-19 08:25:00'),
    (2, 5, '2026-10-19 08:05:00', '2026-10-19 11:00:00'),
    (1, 2, '2026-10-19 08:30:00', '2026-10-19 12:15:00'),
    (2, 3, '2026-10-19 09:10:00', '2026-10-19 13:30:00'),
    (1, 1, '2026-10-19 10:00:00', '2026-10-19 12:00:00'),
    (2, 4, '2026-10-19 12:30:00', NULL),
    (1, 5, '2026-10-19 13:00:00', NULL);
