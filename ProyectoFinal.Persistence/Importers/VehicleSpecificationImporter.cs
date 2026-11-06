using CsvHelper;
using CsvHelper.Configuration;
using ProyectoFinal.Domain.Entities;
using ProyectoFinal.Domain.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ProyectoFinal.Persistence.Importers;

public class VehicleSpecificationImporter : IDataImporter
{
    public async Task<IEnumerable<Vehiculo>> ImportAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"No se encontró el archivo CSV: {filePath}");

        var vehiculos = new List<Vehiculo>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<dynamic>().ToList();

        int idCounter = 1;

        await Task.Run(() =>
        {
            foreach (var record in records)
            {
                // Convertimos el objeto dinámico a un diccionario
                var recordDict = (IDictionary<string, object>)record;

                // Función auxiliar para obtener valores de forma segura
                string? GetValue(string key)
                {
                    var match = recordDict.Keys.FirstOrDefault(k =>
                        k.Equals(key, StringComparison.OrdinalIgnoreCase) ||
                        k.Replace(" ", "").Replace("_", "").Equals(key.Replace("_", ""), StringComparison.OrdinalIgnoreCase));

                    return match != null ? recordDict[match]?.ToString() : null;
                }

                string categoria = GetValue("Vehicle_category") ?? GetValue("Category") ?? "Desconocido";

                Vehiculo vehiculo;

                if (categoria.Contains("Truck", StringComparison.OrdinalIgnoreCase))
                {
                    vehiculo = new Camion { CapacidadCargaKg = 5000, NumEjes = 2 };
                }
                else if (categoria.Contains("Moto", StringComparison.OrdinalIgnoreCase))
                {
                    vehiculo = new Moto { TipoMoto = "Estándar", TieneABS = true };
                }
                else
                {
                    vehiculo = new Coche { NumPuertas = 4, NumPlazas = 5, CapacidadMaleteroLitros = 300 };
                }

                // Rellenamos TODAS las propiedades usando únicamente GetValue
                vehiculo.Id = idCounter++;
                vehiculo.Marca = GetValue("brand") ?? "Desconocido";  // <-- Corregido
                vehiculo.Modelo = GetValue("model") ?? "Desconocido";
                vehiculo.Año = TryParseInt(GetValue("year")); // Por si acaso se llama "year"
                vehiculo.Precio = TryParseDecimal(GetValue("price_inr")); // <-- Corregido
                vehiculo.TipoCombustible = GetValue("fuel_type") ?? "Desconocido";
                vehiculo.Transmision = GetValue("transmission") ?? "Desconocido";
                vehiculo.CategoriaVehiculo = categoria;
                vehiculo.CilindradaCC = TryParseDouble(GetValue("engine_cc")); // <-- Corregido
                vehiculo.PotenciaHP = TryParseDouble(GetValue("power")); // Ojo, aquí pone "...erpower", será "max_power" o similar
                vehiculo.ConsumoKmPorLitro = TryParseDouble(GetValue("mileage_kmpl")); // <-- Corregido
                vehiculo.EmisionesCO2 = TryParseDouble(GetValue("emissions")); // No la veo en la cabecera, se quedará en 0


                vehiculos.Add(vehiculo);
            }
        });

        return vehiculos;
    }

    // Métodos auxiliares
    private int TryParseInt(string? value) => int.TryParse(value, out int result) ? result : 0;
    private double TryParseDouble(string? value) => double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) ? result : 0;
    private decimal TryParseDecimal(string? value) => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : 0;
}

