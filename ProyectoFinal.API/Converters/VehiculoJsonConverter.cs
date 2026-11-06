using ProyectoFinal.Domain.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProyectoFinal.API.Converters;

public class VehiculoJsonConverter : JsonConverter<Vehiculo>
{
    public override Vehiculo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonObject = jsonDoc.RootElement;

        if (jsonObject.TryGetProperty("categoriaVehiculo", out var categoriaProp))
        {
            string categoria = categoriaProp.GetString() ?? "";
            if (categoria.Contains("Truck", StringComparison.OrdinalIgnoreCase))
                return JsonSerializer.Deserialize<Camion>(jsonObject.GetRawText(), options)!;

            if (categoria.Contains("Moto", StringComparison.OrdinalIgnoreCase))
                return JsonSerializer.Deserialize<Moto>(jsonObject.GetRawText(), options)!;
        }

        return JsonSerializer.Deserialize<Coche>(jsonObject.GetRawText(), options)!;
    }

    public override void Write(Utf8JsonWriter writer, Vehiculo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
