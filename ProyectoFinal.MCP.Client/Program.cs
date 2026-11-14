using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text;
using System.Text.Json;


// =================================================================================
// 1. CONFIGURACIÓN INICIAL (Leer API Key de Gemini)
// =================================================================================
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var apiKey = config["Gemini:ApiKey"]!;
var modelName = config["Gemini:Model"] ?? "gemini-2.5-flash-lite";

if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "AQUI_PEGA_TU_API_KEY_DE_GOOGLE_AI_STUDIO")
{
    Console.WriteLine("❌ ERROR: Debes configurar tu API Key de Gemini en el archivo appsettings.json");
    return;
}

// =================================================================================
// 2. CONECTAR CON EL SERVIDOR MCP (Lanzar proceso hijo por stdio)
// =================================================================================
Console.WriteLine("[CLIENTE] Lanzando el servidor MCP...");

// Construimos la ruta al proyecto del servidor para lanzarlo con dotnet run
var serverProjectPath = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..", "ProyectoFinal.MCP.Server", "ProyectoFinal.MCP.Server.csproj"));

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "ProyectoFinal.MCP.Server",
    Command = "dotnet",
    Arguments = ["run", "--project", serverProjectPath]
});

// Creamos el cliente (esto hace el handshake: initialize)
await using var client = await McpClient.CreateAsync(transport);
Console.WriteLine("[CLIENTE] ¡Conectado al servidor MCP!");

// =================================================================================
// 3. DESCUBRIMIENTO DE HERRAMIENTAS (tools/list)
// =================================================================================
var mcpTools = await client.ListToolsAsync();
Console.WriteLine($"\n[CLIENTE] Herramientas descubiertas en el servidor:");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"  - {tool.Name}: {tool.Description}");
}

// =================================================================================
// 4. PREPARAR GEMINI CON LAS HERRAMIENTAS
// =================================================================================
// Convertimos las herramientas de MCP al formato que entiende Gemini (Function Calling)
//var geminiTools = new List<Tool>();
var geminiTools = new List<Google.GenAI.Types.Tool>();

foreach (var tool in mcpTools)
{
    //geminiTools.Add(new Tool
    geminiTools.Add(new Google.GenAI.Types.Tool
    {
        FunctionDeclarations = new List<FunctionDeclaration>
        {
            new FunctionDeclaration
            {
                Name = tool.Name,
                Description = tool.Description,
                // Nota didáctica: Para simplificar, Gemini no verá los parámetros exactos 
                // en esta demo. En un proyecto real se mapearía tool.InputSchema.
                Parameters = new Schema
                {
                    Type = "object",
                    Properties = new Dictionary<string, Schema>
                    {
                        { "brand", new Schema { Type = "string", Description = "Marca a buscar" } },
                        { "id", new Schema { Type = "integer", Description = "ID del vehículo" } }
                    }
                }
            }
        }
    });
}

var geminiClient = new Client(apiKey: apiKey);

// =================================================================================
// 5. BUCLE PRINCIPAL (Enrutador Híbrido)
// =================================================================================
Console.WriteLine("\n=======================================================");
Console.WriteLine("🧠 CEREBRO MCP INICIADO. Escribe tu pregunta (o 'salir').");
Console.WriteLine("=======================================================");

while (true)
{
    Console.Write("\n> ");
    var userInput = Console.ReadLine();

    // REGLA DE IMPORTACIÓN
    if (userInput.Contains("importa", StringComparison.OrdinalIgnoreCase) ||
        userInput.Contains("carga", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("[ENRUTADOR: REGLAS] Detectada intención de importar CSV...");
        var result = await client.CallToolAsync("import_vehicles_from_csv", new Dictionary<string, object?>());
        PrintResult(result);
        continue;
    }


    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "salir") break;

    // -----------------------------------------------------------------------------
    // FASE 1: ENRUTADOR DE REGLAS (Rápido, sin IA)
    // -----------------------------------------------------------------------------
    if (userInput.Contains("cuántos", StringComparison.OrdinalIgnoreCase) || userInput.Contains("cuantos", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("[ENRUTADOR: REGLAS] Detectada intención de contar...");

        //var result = await client.CallToolAsync("GetVehicleCount", new Dictionary<string, object?>());
        var result = await client.CallToolAsync("get_vehicle_count", new Dictionary<string, object?>());
        PrintResult(result);
        continue; // Saltamos a la siguiente pregunta (no llamamos a Gemini)
    }
    else if (userInput.StartsWith("busca", StringComparison.OrdinalIgnoreCase) || userInput.Contains("marca", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("[ENRUTADOR: REGLAS] Detectada intención de buscar por marca...");

        // Extraemos la última palabra como marca (ej. "busca Toyota" -> "Toyota")
        var words = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var brand = words.Last();

        //var args = new Dictionary<string, object?> { { "brand", brand } };

        //var result = await client.CallToolAsync("FindVehiclesByBrand", args);
        // AHORA: Cambiamos el nombre a argumentosRegla
        var argumentosRegla = new Dictionary<string, object?> { { "brand", brand } };

        //var result = await client.CallToolAsync("FindVehiclesByBrand", argumentosRegla);
        var result = await client.CallToolAsync("find_vehicles_by_brand", argumentosRegla);
        PrintResult(result);
        continue;
    }

    // -----------------------------------------------------------------------------
    // FASE 2: ENRUTADOR LLM (Gemini) - Solo si las reglas fallan
    // -----------------------------------------------------------------------------
    Console.WriteLine("[ENRUTADOR: LLM] Las reglas no coinciden. Consultando a Gemini...");

    try
    {
        // Le pasamos la pregunta y las herramientas a Gemini
        var response = await geminiClient.Models.GenerateContentAsync(
            model: modelName,
            contents: userInput,
            config: new GenerateContentConfig { Tools = geminiTools }
        );

        // Comprobamos si Gemini ha decidido usar una herramienta
        var functionCall = response.Candidates?[0].Content?.Parts?.FirstOrDefault(p => p.FunctionCall != null)?.FunctionCall;

        if (functionCall != null)
        {
            Console.WriteLine($"[GEMINI] Ha decidido usar la herramienta: {functionCall.Name}");

            // Convertimos los argumentos de Gemini a un Diccionario para MCP
            var argsParaMcp = new Dictionary<string, object?>();
            if (functionCall.Args != null)
            {
                foreach (var kvp in functionCall.Args)
                    argsParaMcp[kvp.Key] = kvp.Value?.ToString();
            }

            // Ejecutamos la herramienta en el Servidor MCP
            var result = await client.CallToolAsync(functionCall.Name, argsParaMcp);


            Console.WriteLine("[MCP] Resultado de la herramienta:");
            PrintResult(result);

            // (Opcional) Podríamos devolver el resultado a Gemini para que redacte una respuesta natural.
            // Por ahora, mostramos el resultado directo del servidor.
        }
        else
        {
            // Gemini ha respondido con texto normal, sin usar herramientas
            Console.WriteLine($"[GEMINI] Respuesta: {response.Text}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR GEMINI] No se pudo conectar: {ex.Message}");
    }
}

// =================================================================================
// FUNCIÓN AUXILIAR PARA IMPRIMIR RESULTADOS
// =================================================================================
static void PrintResult(CallToolResult result)
{
    if (result.Content.Count > 0 && result.Content[0] is TextContentBlock textBlock)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SERVIDOR MCP]: {textBlock.Text}");
        Console.ResetColor();
    }
}

