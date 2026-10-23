using NpgsqlTypes;

namespace ParqueoAutomoviles.Model;

public enum PRQTipoAutomovil
{
    [PgName("sedan")]
    Sedan,

    [PgName("4x4")]
    CuatroX4,

    [PgName("moto")]
    Moto
}
