using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProyectoFinal.Domain.Entities;

namespace ProyectoFinal.WPFClient
{
    public class VehiculoConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vehiculo);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            string categoria = jsonObject["CategoriaVehiculo"]?.ToString() ?? "";

            Vehiculo vehiculo;
            if (categoria.Contains("Truck", StringComparison.OrdinalIgnoreCase))
                vehiculo = new Camion();
            else if (categoria.Contains("Moto", StringComparison.OrdinalIgnoreCase))
                vehiculo = new Moto();
            else
                vehiculo = new Coche();

            serializer.Populate(jsonObject.CreateReader(), vehiculo);
            return vehiculo;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

