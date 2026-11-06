using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Domain.Entities;
using ProyectoFinal.Domain.Interfaces;

namespace ProyectoFinal.API.Controllers;

[Authorize] // <-- Esto exige el token JWT para acceder
[ApiController]
[Route("api/[controller]")]
public class VehiculoController : ControllerBase
{
    private readonly IVehiculoRepository _repository;

    public VehiculoController(IVehiculoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var vehiculos = await _repository.GetAllAsync();
        return Ok(vehiculos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var vehiculo = await _repository.GetByIdAsync(id);
        if (vehiculo == null) return NotFound(new { message = "Vehículo no encontrado" });
        return Ok(vehiculo);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Vehiculo vehiculo)
    {
        await _repository.AddAsync(vehiculo);
        return Ok(new { message = "Vehículo creado correctamente" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Vehiculo vehiculo)
    {
        if (id != vehiculo.Id) return BadRequest(new { message = "El ID no coincide" });
        await _repository.UpdateAsync(vehiculo);
        return Ok(new { message = "Vehículo actualizado correctamente" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        return Ok(new { message = "Vehículo eliminado correctamente" });
    }

    // POST: api/vehiculo/import
    [HttpPost("import")]
    public async Task<IActionResult> ImportCsv([FromServices] IDataImporter importer, [FromServices] IWebHostEnvironment env)
    {
        try
        {
            // Construimos la ruta al archivo CSV dentro de la carpeta Data de la API
            string filePath = Path.Combine(env.ContentRootPath, "Data", "vehicle_specification_dataset_10000_rows.csv");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = $"No se encontró el archivo en: {filePath}" });
            }

            // Usamos el importador que inyectamos
            var vehiculos = await importer.ImportAsync(filePath);

            // Los guardamos en el repositorio (DataSet)
            await _repository.ImportAsync(vehiculos);

            return Ok(new { message = "Importación completada con éxito", count = vehiculos.Count() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al importar", error = ex.Message });
        }
    }

}

