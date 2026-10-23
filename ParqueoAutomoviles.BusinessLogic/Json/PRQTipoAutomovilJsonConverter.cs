using System.Text.Json;
using System.Text.Json.Serialization;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Json;

internal sealed class PRQTipoAutomovilJsonConverter : JsonConverter<PRQTipoAutomovil>
{
    public override PRQTipoAutomovil Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString()?.ToLowerInvariant() switch
        {
            "sedan" => PRQTipoAutomovil.Sedan,
            "4x4" => PRQTipoAutomovil.CuatroX4,
            "moto" => PRQTipoAutomovil.Moto,
            var value => throw new JsonException($"Tipo de automóvil desconocido: {value}")
        };

    public override void Write(Utf8JsonWriter writer, PRQTipoAutomovil value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            PRQTipoAutomovil.Sedan => "sedan",
            PRQTipoAutomovil.CuatroX4 => "4x4",
            PRQTipoAutomovil.Moto => "moto",
            _ => throw new JsonException($"Tipo de automóvil no admitido: {value}")
        });
}
