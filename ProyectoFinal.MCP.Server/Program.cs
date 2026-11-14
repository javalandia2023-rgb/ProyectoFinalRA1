using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ProyectoFinal.Domain.Interfaces;
using ProyectoFinal.Persistence.Repositories;
using ProyectoFinal.Persistence.Importers;

// =================================================================================
// 1. CONFIGURACIÓN DEL SERVIDOR MCP (El "Host")
// =================================================================================
// Imagina que este código es el "recepcionista" del servidor. Su trabajo es 
// arrancar el servidor y dejarlo escuchando por la consola (stdio).
var builder = Host.CreateApplicationBuilder(args);

// --- INYECCIÓN DE DEPENDENCIAS (DI) ---
// Le decimos al servidor: "Cuando una herramienta te pida un IVehiculoRepository,
// dale la instancia de InMemoryVehiculoRepository".
// Usamos AddSingleton porque queremos que el DataSet (los 2000 vehículos) 
// viva en memoria mientras el servidor esté encendido.
builder.Services.AddSingleton<IVehiculoRepository, InMemoryVehiculoRepository>();
// el Servidor MCP arranca como un proceso nuevo. Aunque el Cliente le pida ejecutar
// la herramienta, el Servidor no sabe cómo construir el IDataImporter 
// Al añadir AddTransient, le decimos: "Cuando alguien te pida un IDataImporter, crea
// una instancia de VehicleSpecificationImporter y pásasela".
builder.Services.AddTransient<IDataImporter, VehicleSpecificationImporter>();

// --- CONFIGURACIÓN DEL SDK DE MCP ---
builder.Services
    .AddMcpServer()                 // Habilita el servidor MCP.
    .WithStdioServerTransport()     // Configura la comunicación por consola (stdin/stdout). ¡No hay HTTP, ni puertos, ni Swagger!
    .WithToolsFromAssembly();       // Busca automáticamente todas las clases con [McpServerToolType] 
                                    // y registra sus herramientas.

// Arranca el servidor. A partir de aquí, el cliente MCP puede conectarse.
await builder.Build().RunAsync();


// =================================================================================
// 2. DEFINICIÓN DE LAS HERRAMIENTAS (TOOLS)
// =================================================================================
// El atributo [McpServerToolType] marca esta clase como un contenedor de herramientas.
// El SDK buscará aquí dentro métodos decorados con [McpServerTool].
[McpServerToolType]
public static class VehiculoTools
{
    // -----------------------------------------------------------------------------
    // HERRAMIENTA 1: Contar vehículos
    // -----------------------------------------------------------------------------
    // [McpServerTool] -> Esto es lo que el cliente (o Gemini) verá en el catálogo de herramientas.
    // [Description]   -> Es CRÍTICO. El LLM no lee el código, solo lee esto. Si la descripción
    //                    es mala, la IA no sabrá cuándo usar esta herramienta.
    [McpServerTool, Description("Devuelve el número total de vehículos que hay en la base de datos")]
    public static async Task<string> GetVehicleCount(IVehiculoRepository repo)
    {
        // El SDK de MCP inyecta automáticamente el repositorio (DI) cuando el cliente llama a esta herramienta.
        var vehiculos = await repo.GetAllAsync();

        // Devolvemos un string formateado. El cliente MCP lo recibirá y podrá mostrarlo o pasárselo a Gemini.
        return $"En total hay {vehiculos.Count()} vehículos en la base de datos.";
    }

    // -----------------------------------------------------------------------------
    // HERRAMIENTA 2: Buscar vehículos por marca
    // -----------------------------------------------------------------------------
    [McpServerTool, Description("Busca vehículos por su marca (ej. 'Seat', 'Toyota') y devuelve los modelos encontrados")]
    public static async Task<string> FindVehiclesByBrand(string brand, IVehiculoRepository repo)
    {
        // 1. Obtenemos todos los vehículos del DataSet.
        var vehiculos = await repo.GetAllAsync();

        // 2. Aplicamos LINQ para filtrar. Usamos StringComparison.OrdinalIgnoreCase 
        //    para que no importe si el usuario escribe "seat" o "SEAT".
        var filtrados = vehiculos
            .Where(v => v.Marca.Contains(brand, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // 3. Si no hay resultados, devolvemos un mensaje de error legible.
        if (!filtrados.Any())
            return $"No he encontrado ningún vehículo de la marca '{brand}'.";

        // 4. Si hay resultados, devolvemos un resumen con los modelos.
        //    Usamos string.Join para unir los modelos en una sola frase.
        return $"He encontrado {filtrados.Count} vehículos de la marca '{brand}': "
               + string.Join(", ", filtrados.Select(v => v.Modelo));
    }

    // -----------------------------------------------------------------------------
    // HERRAMIENTA 3: Buscar vehículo por ID
    // -----------------------------------------------------------------------------
    [McpServerTool, Description("Obtiene los datos detallados de un vehículo usando su ID numérico")]
    public static async Task<string> GetVehicleById(int id, IVehiculoRepository repo)
    {
        var vehiculo = await repo.GetByIdAsync(id);

        if (vehiculo == null)
            return $"No existe ningún vehículo con el ID {id}.";

        // Devolvemos los datos clave del vehículo. 
        return $"Vehículo con ID {id}: {vehiculo.Marca} {vehiculo.Modelo} (Año {vehiculo.Año}, Precio {vehiculo.Precio}€).";
    }
    [McpServerTool, Description("Importa los vehículos desde el archivo CSV a la base de datos en memoria del servidor MCP")]
    public static async Task<string> ImportVehiclesFromCsv(IDataImporter importer, IVehiculoRepository repo)
    {
        try
        {
            // Construimos la ruta usando AppContext.BaseDirectory (la carpeta donde se ejecuta el servidor)
            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "vehicle_specification_dataset_10000_rows.csv");

            if (!System.IO.File.Exists(filePath))
                return $"❌ No se encontró el archivo CSV en: {filePath}";

            // Usamos el importador que ya programamos en la Fase 1
            var vehiculos = await importer.ImportAsync(filePath);

            // Guardamos los vehículos en el repositorio (DataSet) del servidor MCP
            await repo.ImportAsync(vehiculos);

            return $"✅ ¡Importación completada! Se han cargado {vehiculos.Count()} vehículos en el servidor MCP.";
        }
        catch (Exception ex)
        {
            return $"❌ Error al importar: {ex.Message}";
        }
    }

}
