using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoFinal.WPFClient.Services
{
    public class McpService
    {
        private McpClient? _client;
        private Client? _geminiClient;
        private string _modelName = "gemini-2.5-flash-lite";
        private List<Google.GenAI.Types.Tool> _geminiTools = new();

        public async Task InitializeAsync()
        {
            // 1. Leer configuración (API Key de Gemini)
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var apiKey = config["Gemini:ApiKey"]!;
            _modelName = config["Gemini:Model"] ?? _modelName;

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "TU_API_KEY_DE_GEMINI_AQUI")
                throw new Exception("Debes configurar tu API Key de Gemini en el appsettings.json del WPF.");

            // 2. Conectar con el Servidor MCP (lanza el proceso hijo)
            var serverProjectPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", "ProyectoFinal.MCP.Server", "ProyectoFinal.MCP.Server.csproj"));

            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "ProyectoFinal.MCP.Server",
                Command = "dotnet",
                Arguments = ["run", "--project", serverProjectPath]
            });

            _client = await McpClient.CreateAsync(transport);

            // 3. Descubrir herramientas y preparar Gemini
            var mcpTools = await _client.ListToolsAsync();
            _geminiTools.Clear();
            foreach (var tool in mcpTools)
            {
                _geminiTools.Add(new Google.GenAI.Types.Tool
                {
                    FunctionDeclarations = new List<FunctionDeclaration>
                    {
                        new FunctionDeclaration
                        {
                            Name = tool.Name,
                            Description = tool.Description,
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

            _geminiClient = new Client(apiKey: apiKey);
        }

        public async Task<string> AskQuestionAsync(string userInput)
        {
            if (_client == null || _geminiClient == null)
                throw new InvalidOperationException("El servicio MCP no está inicializado.");

            // --- FASE 1: ENRUTADOR DE REGLAS (Rápido, sin IA) ---
            if (userInput.Contains("importa", StringComparison.OrdinalIgnoreCase) ||
                userInput.Contains("carga", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _client.CallToolAsync("import_vehicles_from_csv", new Dictionary<string, object?>());
                return GetTextFromResult(result);
            }

            if (userInput.Contains("cuántos", StringComparison.OrdinalIgnoreCase) ||
                userInput.Contains("cuantos", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _client.CallToolAsync("get_vehicle_count", new Dictionary<string, object?>());
                return GetTextFromResult(result);
            }

            if (userInput.StartsWith("busca", StringComparison.OrdinalIgnoreCase) ||
                userInput.Contains("marca", StringComparison.OrdinalIgnoreCase))
            {
                var words = userInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var brand = words.Last();
                var args = new Dictionary<string, object?> { { "brand", brand } };
                var result = await _client.CallToolAsync("find_vehicles_by_brand", args);
                return GetTextFromResult(result);
            }

            // --- FASE 2: LLM (Gemini) ---
            try
            {
                var response = await _geminiClient.Models.GenerateContentAsync(
                    model: _modelName,
                    contents: userInput,
                    config: new GenerateContentConfig { Tools = _geminiTools }
                );

                var functionCall = response.Candidates?[0].Content?.Parts?.FirstOrDefault(p => p.FunctionCall != null)?.FunctionCall;

                if (functionCall != null)
                {
                    var argsParaMcp = new Dictionary<string, object?>();
                    if (functionCall.Args != null)
                    {
                        foreach (var kvp in functionCall.Args)
                            argsParaMcp[kvp.Key] = kvp.Value?.ToString();
                    }

                    var result = await _client.CallToolAsync(functionCall.Name, argsParaMcp);
                    return GetTextFromResult(result);
                }
                else
                {
                    return response.Text ?? "Gemini no ha devuelto texto.";
                }
            }
            catch (Exception ex)
            {
                return $"Error de Gemini: {ex.Message}";
            }
        }

        private string GetTextFromResult(CallToolResult result)
        {
            if (result.Content.Count > 0 && result.Content[0] is TextContentBlock textBlock)
                return textBlock.Text;
            return "No se ha obtenido respuesta.";
        }
    }
}

