using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ProyectoFinal.Domain.Entities;

namespace ProyectoFinal.ConsoleClient;

class Program
{
    // =================================================================================
    // CONFIGURACIÓN INICIAL
    // =================================================================================
    // ¡OJO! Cambia este puerto por el que te salga a ti cuando arrancas la API
    static string apiBaseUrl = "http://localhost:5168";
    static string token = "";
    static HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        httpClient.BaseAddress = new Uri(apiBaseUrl);

        // =================================================================================
        // 1. LOGIN
        // =================================================================================
        Console.WriteLine("=== INICIANDO SESIÓN ===");
        Console.Write("Usuario: ");
        var username = Console.ReadLine() ?? "";
        Console.Write("Contraseña: ");
        var password = Console.ReadLine() ?? "";

        var loginResponse = await httpClient.PostAsJsonAsync("/api/Auth/login", new { username, password });

        if (!loginResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("❌ Error de login. Credenciales incorrectas.");
            return;
        }

        var loginData = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        token = loginData.GetProperty("token").GetString()!;

        // Configuramos el token para futuras peticiones
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Console.WriteLine("✅ Login exitoso. Token obtenido.\n");

        // =================================================================================
        // 2. MENÚ PRINCIPAL (CRUD e Importación)
        // =================================================================================
        while (true)
        {
            Console.WriteLine("\n=== MENÚ PRINCIPAL ===");
            Console.WriteLine("1. Listar vehículos (GET)");
            Console.WriteLine("2. Buscar vehículo por ID (GET)");
            Console.WriteLine("3. Crear vehículo (POST)");
            Console.WriteLine("4. Eliminar vehículo (DELETE)");
            Console.WriteLine("5. Importar CSV (POST)");
            Console.WriteLine("0. Salir");
            Console.Write("Opción: ");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    var vehiculos = await httpClient.GetFromJsonAsync<List<Coche>>("/api/Vehiculo");
                    Console.WriteLine($"Se han encontrado {vehiculos?.Count ?? 0} vehículos.");
                    foreach (var v in vehiculos!.Take(5)) // Mostramos solo 5 para no saturar
                        Console.WriteLine($" - ID {v.Id}: {v.Marca} {v.Modelo} ({v.Año})");
                    break;

                case "2":
                    Console.Write("ID del vehículo: ");
                    var id = Console.ReadLine();
                    var vehiculo = await httpClient.GetFromJsonAsync<Coche>($"/api/Vehiculo/{id}");
                    if (vehiculo != null)
                        Console.WriteLine($"Vehículo: {vehiculo.Marca} {vehiculo.Modelo}, Precio: {vehiculo.Precio}€");
                    else
                        Console.WriteLine("❌ No encontrado.");
                    break;

                case "3":
                    var nuevoCoche = new Coche
                    {
                        Marca = "Toyota",
                        Modelo = "Corolla",
                        Año = 2023,
                        Precio = 25000,
                        TipoCombustible = "Hybrid",
                        CategoriaVehiculo = "Sedan",
                        NumPuertas = 4,
                        NumPlazas = 5
                    };
                    var createResponse = await httpClient.PostAsJsonAsync("/api/Vehiculo", nuevoCoche);
                    Console.WriteLine(createResponse.IsSuccessStatusCode ? "✅ Creado." : "❌ Error.");
                    break;

                case "4":
                    Console.Write("ID a eliminar: ");
                    var deleteId = Console.ReadLine();
                    var deleteResponse = await httpClient.DeleteAsync($"/api/Vehiculo/{deleteId}");
                    Console.WriteLine(deleteResponse.IsSuccessStatusCode ? "✅ Eliminado." : "❌ Error.");
                    break;

                case "5":
                    var importResponse = await httpClient.PostAsync("/api/Vehiculo/import", null);
                    Console.WriteLine(importResponse.IsSuccessStatusCode ? "✅ Importado." : "❌ Error.");
                    break;

                case "0":
                    return;
            }
        }
    }
}

